using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Checker (returns a boolean but also writes logs)
/// </summary>
public static class PatchVerifier
{
  public static bool Verify(string modFolder, string pluginName, SkyblivionEnvironment? env, TextWriter log)
  {
    var ok = true; // cross your fingers
    var pluginPath = Path.Combine(modFolder, pluginName);
    if (!File.Exists(pluginPath)) 
    {
      log.WriteLine($"Plugin missing: {pluginPath}");
      return false;
    }
    var readParams = new BinaryReadParameters { StringsParam = new StringsReadParameters { TargetLanguage = Language.English } };
    var patch = SkyrimMod.CreateFromBinaryOverlay(pluginPath, SkyrimRelease.SkyrimSE, readParams);
    log.WriteLine($"Plugin {pluginName}: esl={patch.IsSmallMaster} masters=[{string.Join(", ", patch.ModHeader.MasterReferences.Select(m => m.Master.FileName))}]");

    var voiceTypes = patch.VoiceTypes.ToDictionary(v => v.FormKey, v => v.EditorID ?? "");
    log.WriteLine($"Voice types ({voiceTypes.Count}): {string.Join(", ", voiceTypes.Values.OrderBy(v => v))}");

    var npcs = patch.Npcs.ToList();
    var perVoice = npcs
      .GroupBy(n => n.Voice.FormKey)
      .ToDictionary(g => g.Key, g => g.Count());
    log.WriteLine($"NPC overrides: {npcs.Count}");
    foreach (var kv in perVoice.OrderByDescending(k => k.Value))
    {
      var name = voiceTypes.TryGetValue(kv.Key, out var edid)
        ? edid
        : (env != null && env.LinkCache.TryResolve<IVoiceTypeGetter>(kv.Key, out var vt) ? vt.EditorID ?? kv.Key.ToString() : kv.Key.ToString());
      log.WriteLine($"  {name,-28} {kv.Value,5} NPCs");
      if (!voiceTypes.ContainsKey(kv.Key))
      {
        log.WriteLine("    WARNING: voice type not defined in this plugin");
        ok = false;
      }
    }

    var foreignNpcs = npcs.Count(n => n.FormKey.ModKey == patch.ModKey);
    if (foreignNpcs > 0) 
    {
      log.WriteLine($"WARNING: {foreignNpcs} NPCs are new records rather than overrides");
      ok = false;
    }

    var voiceRoot = Path.Combine(modFolder, "Sound", "Voice");
    if (!Directory.Exists(voiceRoot))
    {
      log.WriteLine("No Sound\\Voice folder in the mod");
      return false;
    }
    foreach (var pluginDir in Directory.EnumerateDirectories(voiceRoot))
    {
      log.WriteLine($"Voice files for {Path.GetFileName(pluginDir)}:");
      foreach (var vtDir in Directory.EnumerateDirectories(pluginDir).OrderBy(d => d))
      {
        var files = Directory.EnumerateFiles(vtDir, "*.fuz")
          .ToList();
        long bytes = 0;
        var bad = 0;
        var noLip = 0;
        foreach (var f in files)
        {
          const int expectedHeaderLength = 12;
          var fi = new FileInfo(f);
          bytes += fi.Length;
          using var fs = fi.OpenRead();
          var header = new byte[expectedHeaderLength];
          if (fs.Read(header, 0, expectedHeaderLength) < expectedHeaderLength || !CheckFUZEHeader(header)) 
          {
            bad++;
            continue;
          }
          if (BitConverter.ToUInt32(header, 8) == 0)
            noLip++;
        }
        var defined = voiceTypes.Values.Contains(Path.GetFileName(vtDir), StringComparer.OrdinalIgnoreCase);
        // 1048576.0 = 1024 * 1024 (bytes per mb); using a double here so the result keeps its fraction
        log.WriteLine($"  {Path.GetFileName(vtDir),-28} {files.Count,6} files {bytes / 1048576.0,8:F1} MB  noLip={noLip} corrupt={bad}{(defined ? "" : "  (voice type not in plugin)")}");
        if (bad > 0) ok = false;
      }
    }
    return ok;
  }

  static bool CheckFUZEHeader(byte[] header)
  {
    return header[0] == 'F' && header[1] == 'U' && header[2] == 'Z' && header[3] == 'E';
  }
}
