
namespace SkyblivionVoicesRemastered;

/// <summary>
/// Paths may be left empty to use auto-detection or default
/// </summary>
public class PipelineSettings
{
  public string? RemasterPath { get; set; }
  public string? SkyblivionDataPath { get; set; }
  public string SkyblivionPlugins { get; set; } = Constants.SkyblivionESM;
  public string WorkDir { get; set; } = "work";
  public string? OutputDir { get; set; }
  public string PluginName { get; set; } = Constants.PluginName;
  public string? SkyrimRoot { get; set; }
  public string? XwmaEncodePath { get; set; }
  public string? LipGeneratorPath { get; set; }
  public LipMode LipMode { get; set; } = LipMode.Generate;
  public int? XwmBitrate { get; set; }
  public int Parallelism { get; set; } = Math.Max(1, Environment.ProcessorCount / 2);
  public bool Force { get; set; }
  public int? Limit { get; set; }
  /// <summary>
  /// Restrict the build to these group keys (race/sex/variant); null or empty = all groups
  /// </summary>
  public List<string>? Groups { get; set; }
  public bool AllVoiceTypes { get; set; }
  /// <summary>
  /// Flag the patch as a light (ESL) plugin. On by default: it only adds a handful of voice types, and NPC overrides don't count toward the light limit
  /// </summary>
  public bool Esl { get; set; } = true;
  public string? TempDir { get; set; }

  public string ScanPath => Path.Combine(WorkDir, "scan.json");
  public string MapPath => Path.Combine(WorkDir, "map.json");
  public string UnmatchedPath => Path.Combine(WorkDir, "unmatched.txt");
  public string ResolvedOutputDir => string.IsNullOrWhiteSpace(OutputDir) ? Path.Combine(WorkDir, "SkyblivionVoicesRemastered") : OutputDir;
  public IEnumerable<string> PluginList => SkyblivionPlugins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

  public string ResolveRemaster() => (string.IsNullOrWhiteSpace(RemasterPath) ? ToolLocator.FindOblivionRemastered() : RemasterPath)
      ?? throw new ArgumentException("Oblivion Remastered was not found. Choose its install folder.");

  public string ResolveSkyblivionData()
  {
    if (string.IsNullOrWhiteSpace(SkyblivionDataPath))
      throw new ArgumentException("Choose the Skyrim SE Data folder that contains Skyblivion.");
    if (!Directory.Exists(SkyblivionDataPath))
      throw new DirectoryNotFoundException($"Skyblivion data folder not found: {SkyblivionDataPath}");
    return SkyblivionDataPath;
  }

  public string? ResolveSkyrimRoot()
  {
    if (!string.IsNullOrWhiteSpace(SkyrimRoot))
      return SkyrimRoot;
    if (!string.IsNullOrWhiteSpace(SkyblivionDataPath))
      return Directory.GetParent(SkyblivionDataPath)?.FullName;
    return ToolLocator.FindSkyrimSpecialEdition();
  }

  public string ResolveXwmaEncode() => (string.IsNullOrWhiteSpace(XwmaEncodePath) ? ToolLocator.FindTool(ToolLocator.XwmaEncodeRelative, ResolveSkyrimRoot()) : XwmaEncodePath)
      ?? throw new ArgumentException("xWMAEncode.exe was not found. Install the Skyrim SE Creation Kit or choose the file.");

  public string? ResolveLipGenerator() => string.IsNullOrWhiteSpace(LipGeneratorPath) ? ToolLocator.FindTool(ToolLocator.LipGeneratorRelative, ResolveSkyrimRoot()) : LipGeneratorPath;
}

public class BuildResult
{
  public required PatchSummary Patch { get; init; }
  public required VoiceBuildSummary Voices { get; init; }
  public required string OutputDir { get; init; }
  public bool Cancelled => Voices.Cancelled;
}

/// <summary>
/// The scan / map / build / verify steps shared by the command line and the desktop app
/// </summary>
public static class Pipeline
{
  public static ScanResult Scan(PipelineSettings s, TextWriter log)
  {
    var result = new RemasterScanner(log).Scan(s.ResolveRemaster());
    Json.Save(s.ScanPath, result);
    log.WriteLine($"Scan written to {Path.GetFullPath(s.ScanPath)}");

    foreach (var kv in result.Stats)
      log.WriteLine($"  {kv.Key}: {kv.Value}");

    foreach (var g in result.Groups)
      log.WriteLine($"  {g.Key,-28} lines={g.LineCount,5} npcs={g.NpcCount,4} -> {g.VoiceTypeEditorId}");

    foreach (var w in result.Warnings)
      log.WriteLine($"  WARNING: {w}");

    return result;
  }

  public static ScanResult LoadScan(PipelineSettings s)
  {
    if (!File.Exists(s.ScanPath))
      throw new FileNotFoundException("Run Scan first; no scan.json in the work folder.", s.ScanPath);
    return Json.Load<ScanResult>(s.ScanPath);
  }

  public static MapResult LoadMap(PipelineSettings s)
  {
    if (!File.Exists(s.MapPath))
      throw new FileNotFoundException("Run Map first; no map.json in the work folder.", s.MapPath);
    return Json.Load<MapResult>(s.MapPath);
  }

  public static SkyblivionEnvironment LoadSkyblivion(PipelineSettings s, TextWriter log)
  {
    var dataPath = s.ResolveSkyblivionData();
    log.WriteLine($"Loading Skyblivion plugins from {dataPath}");
    return new SkyblivionEnvironment(dataPath, s.PluginList, log);
  }

  public static MapResult Map(PipelineSettings s, TextWriter log)
  {
    var scan = LoadScan(s);
    var env = LoadSkyblivion(s, log);
    var result = new SkyblivionMapper(env, log).Map(scan);
    Json.Save(s.MapPath, result);

    log.WriteLine($"Map written to {Path.GetFullPath(s.MapPath)}");

    foreach (var kv in result.Stats)
      log.WriteLine($"  {kv.Key}: {kv.Value}");

    foreach (var w in result.Warnings)
      log.WriteLine($"  WARNING: {w}");

    File.WriteAllLines(s.UnmatchedPath,
      result.Lines
        .Where(l => !l.IsMapped)
        .Select(l => $"{l.OblivionName} ({l.Strategy})")
        .Concat(result.Npcs
                  .Where(n => !n.IsMapped)
                  .Select(n => "npc " + n.OblivionEditorId)));

    log.WriteLine($"Unmatched list written to {Path.GetFullPath(s.UnmatchedPath)}");

    return result;
  }

  public static BuildResult Build(PipelineSettings s, TextWriter log, IProgress<VoiceBuildProgress>? progress = null, CancellationToken cancellation = default)
  {
    var scan = LoadScan(s);
    var map = LoadMap(s);
    var env = LoadSkyblivion(s, log);
    var outRoot = s.ResolvedOutputDir;
    var xwma = s.ResolveXwmaEncode();
    var lipGen = s.ResolveLipGenerator();
    var targetPlugin = map.SkyblivionPlugins.First();

    var patch = new PatchBuilder(env, log).Build(scan, map, Path.Combine(outRoot, s.PluginName), s.AllVoiceTypes, s.Esl);
    var options = new VoiceBuildOptions
    {
      OutputRoot = outRoot,
      TargetPlugin = targetPlugin,
      XwmaEncodePath = xwma,
      LipGeneratorPath = lipGen,
      LipMode = s.LipMode,
      XwmBitrate = s.XwmBitrate,
      Parallelism = Math.Max(1, s.Parallelism),
      Force = s.Force,
      Limit = s.Limit,
      Groups = s.Groups is { Count: > 0 } ? s.Groups : null,
      IncludeUnusedGroups = s.AllVoiceTypes,
      TempRoot = s.TempDir,
      Progress = progress,
      Cancellation = cancellation
    };

    var voices = new VoiceBuildRunner(log).Run(scan, map, options);
    log.WriteLine($"Voice files: planned={voices.Planned} written={voices.Written} skipped={voices.Skipped} failed={voices.Failed} lipGenerated={voices.LipGenerated} lipReused={voices.LipReused} lipMissing={voices.LipMissing}{(voices.Cancelled ? " (cancelled)" : "")}");
    if (!voices.Errors.IsEmpty)
    {
      var errPath = Path.Combine(outRoot, "build-errors.txt");
      File.WriteAllLines(errPath, voices.Errors);
      log.WriteLine($"  {voices.Errors.Count} errors written to {errPath}");
    }

    if (!voices.LipFallbacks.IsEmpty)
    {
      var lipPath = Path.Combine(outRoot, "lip-fallbacks.txt");
      File.AppendAllLines(lipPath, voices.LipFallbacks);
      log.WriteLine($"  {voices.LipFallbacks.Count} recordings used a fallback lip file (LipGenerator failed); list appended to {lipPath}");
    }

    File.WriteAllText(Path.Combine(outRoot, "README-generated.txt"),
        $"Generated by SkyblivionVoicesRemastered on {DateTime.Now}.\nPlugin: {s.PluginName} ({patch.VoiceTypes} voice types, {patch.NpcOverrides} NPC overrides)\n" +
        $"Voice files: {voices.Written + voices.Skipped} under Sound\\Voice\\{targetPlugin}\\\nInstall this folder as a mod and load {s.PluginName} after {targetPlugin}.\n");
    log.WriteLine(voices.Cancelled ? "Build cancelled; run again to resume (existing files are kept)." : $"Mod folder ready: {outRoot}");
    return new BuildResult { Patch = patch, Voices = voices, OutputDir = outRoot };
  }

  public static bool Verify(PipelineSettings s, TextWriter log)
  {
    var env = string.IsNullOrWhiteSpace(s.SkyblivionDataPath) ? null : LoadSkyblivion(s, log);
    var ok = PatchVerifier.Verify(s.ResolvedOutputDir, s.PluginName, env, log);
    log.WriteLine(ok ? "Verification passed" : "Verification found problems");
    return ok;
  }

  public static void Synth(PipelineSettings s, string outputPlugin, string prefix, double tagFraction, TextWriter log)
      => SyntheticSkyblivion.Create(LoadScan(s), outputPlugin, prefix, tagFraction, log);
}
