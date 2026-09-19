using System.Collections.Concurrent;

namespace SkyblivionVoicesRemastered;

public enum LipMode
{
  Generate,
  Reuse,
  None
}

public class VoiceBuildOptions
{
  public required string OutputRoot { get; init; }
  public required string TargetPlugin { get; init; }
  public required string XwmaEncodePath { get; init; }
  public string? LipGeneratorPath { get; init; }
  public LipMode LipMode { get; init; } = LipMode.Generate;
  public int? XwmBitrate { get; init; }
  public int Parallelism { get; init; } = Math.Max(1, Environment.ProcessorCount / 2);
  public bool Force { get; init; }
  public int? Limit { get; init; }
  /// <summary>
  /// Group keys (race/sex/variant) to build; null = every group
  /// </summary>
  public IReadOnlyCollection<string>? Groups { get; init; }
  public bool IncludeUnusedGroups { get; init; }
  public string? TempRoot { get; init; }
  public IProgress<VoiceBuildProgress>? Progress { get; init; }
  public CancellationToken Cancellation { get; init; }
}

public readonly record struct VoiceBuildProgress(int Done, int Total, int Written, int Skipped, int Failed, double PerSecond);

public class VoiceBuildSummary
{
  public int Planned;
  public int Written;
  public int Skipped;
  public int Failed;
  public int LipGenerated;
  public int LipReused;
  public int LipMissing;
  public bool Cancelled;
  public ConcurrentBag<string> Errors { get; } = [];
  /// <summary>
  /// Lines whose lip file could not be generated and fell back to the original lip (or none)
  /// </summary>
  public ConcurrentBag<string> LipFallbacks { get; } = [];
}

/// <summary>
/// Converts each mapped Remaster recording into a Skyrim .fuz inside the right voice type folder
/// </summary>
public class VoiceBuildRunner(TextWriter log)
{
  private readonly TextWriter _log = log;

  const int ReportInterval = 25;
  const int ReportsPerLog = 20;

  public VoiceBuildSummary Run(ScanResult scan, MapResult map, VoiceBuildOptions options)
  {
    var summary = new VoiceBuildSummary();
    var lineMap = map.Lines
      .Where(l => l.IsMapped)
      .ToDictionary(l => l.OblivionName, StringComparer.OrdinalIgnoreCase);
    var mappedNpcs = map.Npcs
      .Where(n => n.IsMapped)
      .Select(n => n.OblivionEditorId)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var usedGroups = scan.Npcs
      .Where(n => mappedNpcs.Contains(n.EditorId))
      .Select(n => n.GroupKey)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var groupVoiceType = scan.Groups
      .ToDictionary(g => g.Key, g => g.VoiceTypeEditorId, StringComparer.OrdinalIgnoreCase);
    var wantedGroups = options.Groups is { Count: > 0 }
      ? options.Groups.ToHashSet(StringComparer.OrdinalIgnoreCase) 
      : null;

    var work = new List<(VoiceLine line, LineMapping mapping, string outPath)>();
    foreach (var line in scan.Lines)
    {
      if (wantedGroups != null && !wantedGroups.Contains(line.GroupKey))
        continue;
      if (!options.IncludeUnusedGroups && !usedGroups.Contains(line.GroupKey))
        continue;
      if (!lineMap.TryGetValue(line.OblivionName, out var mapping))
        continue;
      var outPath = Path.Combine(options.OutputRoot, "Sound", "Voice", options.TargetPlugin, groupVoiceType[line.GroupKey], mapping.SkyrimName + ".fuz");
      work.Add((line, mapping, outPath));
    }
    if (options.Limit is { } limit)
      work = [.. work.Take(limit)];
    summary.Planned = work.Count;
    _log.WriteLine($"Converting {work.Count} recordings with {options.Parallelism} workers (lip mode: {options.LipMode})");

    var archives = new ConcurrentDictionary<string, BsaReader>(StringComparer.OrdinalIgnoreCase);
    BsaReader Archive(string name) => archives.GetOrAdd(name, n => new BsaReader(Path.Combine(scan.RemasterDataPath, n)));
    var tempRoot = options.TempRoot ?? Path.Combine(Path.GetTempPath(), $"SkyblivionVoicesRemastered-{Environment.ProcessId}");

    Directory.CreateDirectory(tempRoot);
    var encoder = new XwmaEncoder(options.XwmaEncodePath, options.XwmBitrate);
    var lipGen = options.LipMode == LipMode.Generate && options.LipGeneratorPath != null ? new LipGenerator(options.LipGeneratorPath) : null;
    if (options.LipMode == LipMode.Generate && lipGen == null)
      _log.WriteLine("LipGenerator.exe not available; falling back to reusing the original lip files.");

    var started = DateTime.UtcNow;
    void Report(int n)
    {
      var elapsed = DateTime.UtcNow - started;
      var perSecond = elapsed.TotalSeconds > 0 ? n / elapsed.TotalSeconds : 0;
      options.Progress?.Report(new VoiceBuildProgress(n, work.Count, summary.Written, summary.Skipped, summary.Failed, perSecond));
      if (n % (ReportInterval * ReportsPerLog) == 0 || n == work.Count)
        _log.WriteLine($"  {n}/{work.Count} ({perSecond:F1}/s) written={summary.Written} skipped={summary.Skipped} failed={summary.Failed}");
    }

    var done = 0;
    try
    {
      Parallel.ForEach(work, new ParallelOptions { MaxDegreeOfParallelism = options.Parallelism, CancellationToken = options.Cancellation }, item =>
      {
        var (line, mapping, outPath) = item;
        try
        {
          if (!options.Force && File.Exists(outPath))
          {
            Interlocked.Increment(ref summary.Skipped);
            return;
          }
          var reader = Archive(line.Archive);
          if (!reader.TryGet(line.ArchivePath, out var entry))
            throw new FileNotFoundException($"{line.ArchivePath} not in {line.Archive}");
          var mp3 = BsaReader.Extract(entry);
          var (samples, rate) = Mp3Decoder.DecodeToMono(mp3);

          var workDir = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
          Directory.CreateDirectory(workDir);
          try
          {
            var wavPath = Path.Combine(workDir, "v.wav");
            WavWriter.WriteMono16(wavPath, samples, rate);
            var xwm = encoder.Encode(wavPath);

            byte[]? lip = null;
            if (lipGen != null)
            {
              lip = lipGen.Generate(wavPath, line.ResponseText ?? mapping.TargetText ?? "", out var lipFailure);
              if (lip != null)
                Interlocked.Increment(ref summary.LipGenerated);
              else
                summary.LipFallbacks.Add($"{line.GroupKey} {line.OblivionName}: {lipFailure}");
            }
            if (lip == null && options.LipMode != LipMode.None && line.LipArchivePath != null)
            {
              var lipReader = Archive(line.LipArchive!);
              if (lipReader.TryGet(line.LipArchivePath, out var lipEntry))
              {
                lip = BsaReader.Extract(lipEntry);
                Interlocked.Increment(ref summary.LipReused);
              }
            }
            if (lip == null)
              Interlocked.Increment(ref summary.LipMissing);

            FuzWriter.Write(outPath, lip, xwm);
            Interlocked.Increment(ref summary.Written);
          }
          finally
          {
            try
            {
              Directory.Delete(workDir, true);
            }
            catch
            {
              // could probably log this, but that might fail too...
            }
          }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
          Interlocked.Increment(ref summary.Failed);
          summary.Errors.Add($"{line.GroupKey} {line.OblivionName}: {ex.Message}");
        }
        finally
        {
          var n = Interlocked.Increment(ref done);
          if (n % ReportInterval == 0 || n == work.Count)
            Report(n);
        }
      });
    }
    catch (OperationCanceledException)
    {
      summary.Cancelled = true;
      _log.WriteLine($"  cancelled after {done} recordings");
    }
    finally
    {
      try
      {
        Directory.Delete(tempRoot, true);
      }
      catch
      {
        // we tried, but it's just a temp dir
      }
    }
    return summary;
  }
}
