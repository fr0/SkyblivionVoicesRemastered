using System.Diagnostics;
using NLayer;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Decodes MP3 to 16-bit mono PCM using NLayer
/// </summary>
public static class Mp3Decoder
{
  // All Skyrim audio seems to be mono by convention
  // The source is mono but we're just being careful here since that's what the lip generator requires
  public static (short[] samples, int sampleRate) DecodeToMono(byte[] mp3)
  {
    using var ms = new MemoryStream(mp3);
    using var file = new MpegFile(ms);
    var channels = file.Channels;
    var rate = file.SampleRate;
    var buffer = new float[4096 * channels];
    var output = new List<short>(mp3.Length * 8);
    int read;
    while ((read = file.ReadSamples(buffer, 0, buffer.Length)) > 0)
    {
      for (var i = 0; i < read; i += channels)
      {
        var sum = 0f;
        for (var c = 0; c < channels && i + c < read; c++)
          sum += buffer[i + c];
        var v = sum / channels;
        output.Add((short)Math.Clamp((int)MathF.Round(v * 32767f), short.MinValue, short.MaxValue));
      }
    }
    return (output.ToArray(), rate);
  }
}

public static class WavWriter
{
  public static void WriteMono16(string path, short[] samples, int sampleRate)
  {
    using var fs = File.Create(path);
    using var bw = new BinaryWriter(fs);
    var dataBytes = samples.Length * 2;
    // PCM WAV header is 44 bytes
    // Reference I used for this: mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html
    bw.Write("RIFF"u8);
    bw.Write(36 + dataBytes);
    bw.Write("WAVE"u8); // RIFF type
    bw.Write("fmt "u8); // format sub-chunk ID (4 characters, intentionally includes trailing space)
    bw.Write(16);       // chunk size
    bw.Write((short)1); // WAVE_FORMAT_PCM
    bw.Write((short)1); // channel count (mono, in this case)
    bw.Write(sampleRate);
    bw.Write(sampleRate * 2); // byte rate = samplerate * channels * bytes-per-sample
    bw.Write((short)2);       // block-align = channels * bytes-per-sample
    bw.Write((short)16);      // bits-per-sample
    bw.Write("data"u8);       // data sub-chunk ID
    bw.Write(dataBytes);
    var bytes = new byte[dataBytes];
    Buffer.BlockCopy(samples, 0, bytes, 0, dataBytes);
    bw.Write(bytes);
  }
}

public static class FuzWriter
{
  // Reference: https://github.com/suglasp/pwsh_convert_fuz_to_xwm/blob/main/convert_fuz_to_xwm.ps1
  public static void Write(string path, byte[]? lip, byte[] xwm)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    // write to a temporary name and rename, so an interrupted run won't leave a truncated .fuz behind
    // (that a later incremental run would mistake for a finished file)
    var tmp = path + ".tmp";
    using (var fs = File.Create(tmp))
    using (var bw = new BinaryWriter(fs))
    {
      bw.Write("FUZE"u8);
      bw.Write(1u);
      bw.Write((uint)(lip?.Length ?? 0));
      if (lip != null)
        bw.Write(lip);
      bw.Write(xwm);
    }
    File.Move(tmp, path, overwrite: true);
  }
}

public static class ExternalTool
{
  [System.Runtime.InteropServices.DllImport("kernel32.dll")]
  private static extern uint SetErrorMode(uint mode); // TODO: SYSLIB1054

  private const uint SemFailCriticalErrors = 0x0001;
  private const uint SemNoGpFaultErrorBox = 0x0002;

  static ExternalTool()
  {
    // The Creation Kit tools occasionally crash on unusual input. Child processes inherit the error mode, so this
    // keeps Windows from showing a crash dialog.
    // TODO: I don't really like this solution; I'd like to spend more time discovering why this is happening.
    if (OperatingSystem.IsWindows())
    {
      try
      {
        _ = SetErrorMode(SetErrorMode(0) | SemFailCriticalErrors | SemNoGpFaultErrorBox);
      }
      catch
      {
      }
    }
  }

  public static (int exitCode, string output) Run(string exe, string arguments, string? workingDirectory = null, int timeoutMs = 120_000)
  {
    var psi = new ProcessStartInfo(exe, arguments)
    {
      UseShellExecute = false,
      CreateNoWindow = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(exe) ?? Environment.CurrentDirectory,
    };
    using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {exe}");
    var stdout = p.StandardOutput.ReadToEndAsync();
    var stderr = p.StandardError.ReadToEndAsync();
    if (!p.WaitForExit(timeoutMs))
    {
      try { p.Kill(true); } catch { /* ignore */ }
      return (-1, $"timed out after {timeoutMs} ms");
    }
    return (p.ExitCode, (stdout.Result + stderr.Result).Trim());
  }

  public static string Quote(string arg) => "\"" + arg.Replace("\"", "'") + "\"";
}

public class XwmaEncoder(string exePath, int? bitrate)
{
  public string ExePath { get; } = exePath;
  public int? Bitrate { get; } = bitrate;

  public byte[] Encode(string wavPath)
  {
    var xwmPath = Path.ChangeExtension(wavPath, ".xwm");
    var args = (Bitrate is { } b ? $"-b {b} " : "") + ExternalTool.Quote(wavPath) + " " + ExternalTool.Quote(xwmPath);
    var (code, output) = ExternalTool.Run(ExePath, args, Path.GetDirectoryName(wavPath));
    if (code != 0 || !File.Exists(xwmPath))
      throw new InvalidOperationException($"xWMAEncode failed ({code}): {output}");
    return File.ReadAllBytes(xwmPath);
  }
}

/// <summary>
/// Wraps the Creation Kit's LipGenerator.exe (FaceFX) to synthesize lip-sync data from audio and text
/// </summary>
public class LipGenerator(string exePath)
{
  public string ExePath { get; } = exePath;

  /// <summary>
  /// Returns the lip data, or null (+ a reason) when the generator failed or crashed for this input
  /// </summary>
  public byte[]? Generate(string wavPath, string text, out string? failure)
  {
    failure = null;
    var lipPath = Path.ChangeExtension(wavPath, ".lip");
    if (File.Exists(lipPath)) File.Delete(lipPath);
    var cleaned = text.Replace('"', '\'')
      .Replace('\r', ' ')
      .Replace('\n', ' ')
      .Trim();
    if (cleaned.Length == 0)
      cleaned = "...";
    // LipGenerator crashes (access violation) sometimes
    // Could be antivirus/Windows Defender
    // Retrying after a short, growing pause fixed every case I tested
    const int attempts = 4;
    for (var attempt = 1; attempt <= attempts; attempt++)
    {
      if (attempt > 1)
        Thread.Sleep(250 << (attempt - 2)); // 250 ms, 500 ms, 1 s
      // a normal run takes < 1 second, a crashed or hung generator should not hold a worker for too long (??? arbitrary)
      var (code, output) = ExternalTool.Run(ExePath, ExternalTool.Quote(wavPath) + " " + ExternalTool.Quote(cleaned), Path.GetDirectoryName(wavPath), 15_000);
      if (File.Exists(lipPath))
        return File.ReadAllBytes(lipPath);
      // don't judge me too much for this, we aren't writing medical device software here
      failure = (code == -1 ? "timed out" : $"exit code {code}{(code == unchecked((int)0xC0000005) ? " (access violation)" : "")}: {output}")
        + $" (after {attempt} attempt{(attempt == 1 ? "" : "s")})";
    }
    return null;
  }
}
