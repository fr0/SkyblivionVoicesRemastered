using System.Diagnostics;
using SkyblivionVoicesRemastered;

var log = Console.Error;
if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
  PrintUsage();
  return 0;
}

var opts = ParseOptions(args.Skip(1));
var settings = BuildSettings(opts);
var sw = Stopwatch.StartNew();
try
{
  switch (args[0].ToLowerInvariant())
  {
    case "scan": Pipeline.Scan(settings, log); break;
    case "map": Pipeline.Map(settings, log); break;
    case "build": Pipeline.Build(settings, log); break;
    case "all":
      Pipeline.Scan(settings, log);
      Pipeline.Map(settings, log);
      Pipeline.Build(settings, log);
      break;
    case "verify": return Pipeline.Verify(settings, log) ? 0 : 1;
    case "synth":
      Pipeline.Synth(settings, Opt("synth-out", Path.Combine(settings.WorkDir, "synthetic", Constants.SkyblivionESM)), Opt("prefix", ""),
          double.Parse(Opt("tag-fraction", "0.5"), System.Globalization.CultureInfo.InvariantCulture), log);
      break;
    case "tools":
      log.WriteLine($"Oblivion Remastered: {settings.RemasterPath ?? ToolLocator.FindOblivionRemastered() ?? "not found"}");
      log.WriteLine($"Skyrim Special Edition: {settings.ResolveSkyrimRoot() ?? "not found"}");
      log.WriteLine($"xWMAEncode: {settings.XwmaEncodePath ?? ToolLocator.FindTool(ToolLocator.XwmaEncodeRelative, settings.ResolveSkyrimRoot()) ?? "not found"}");
      log.WriteLine($"LipGenerator: {settings.ResolveLipGenerator() ?? "not found"}");
      break;
    default:
      log.WriteLine($"Unknown command '{args[0]}'.");
      PrintUsage();
      return 2;
  }
}
catch (Exception ex)
{
  log.WriteLine($"ERROR: {ex.Message}");
  if (opts.ContainsKey("verbose"))
    log.WriteLine(ex.ToString());
  return 1;
}
log.WriteLine($"Done in {sw.Elapsed:mm\\:ss}");
return 0;

string Opt(string name, string fallback) => opts.TryGetValue(name, out var v) ? v : fallback;

static PipelineSettings BuildSettings(Dictionary<string, string> o)
{
  string? Get(string name) => o.TryGetValue(name, out var v) ? v : null;
  var s = new PipelineSettings
  {
    RemasterPath = Get("remaster"),
    SkyblivionDataPath = Get("skyblivion"),
    SkyblivionPlugins = Get("plugin") ?? Constants.SkyblivionESM,
    WorkDir = Get("work") ?? "work",
    OutputDir = Get("out"),
    PluginName = Get("plugin-name") ?? Constants.PluginName,
    SkyrimRoot = Get("skyrim"),
    XwmaEncodePath = Get("xwmaencode"),
    LipGeneratorPath = Get("lipgenerator"),
    LipMode = Enum.Parse<LipMode>(Get("lip") ?? "generate", ignoreCase: true),
    XwmBitrate = Get("xwm-bitrate") is { } b ? int.Parse(b) : null,
    Force = o.ContainsKey("force"),
    Limit = Get("limit") is { } l ? int.Parse(l) : null,
    Groups = Get("group") is { } g ? g.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() : null,
    AllVoiceTypes = o.ContainsKey("all-voicetypes"),
    Esl = !o.ContainsKey("no-esl"),
    TempDir = Get("temp")
  };
  if (Get("parallel") is { } p)
    s.Parallelism = int.Parse(p);
  return s;
}

static Dictionary<string, string> ParseOptions(IEnumerable<string> args)
{
  var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
  var list = args.ToList();
  for (var i = 0; i < list.Count; i++)
  {
    var a = list[i];
    if (!a.StartsWith("--"))
      throw new ArgumentException($"Unexpected argument '{a}'");
    var name = a[2..];
    var eq = name.IndexOf('=');
    if (eq >= 0)
    {
      dict[name[..eq]] = name[(eq + 1)..];
      continue;
    }
    if (i + 1 < list.Count && !list[i + 1].StartsWith("--"))
    {
      dict[name] = list[++i];
      continue;
    }
    dict[name] = "true";
  }
  return dict;
}

static void PrintUsage()
{
  Console.WriteLine($"""
SkyblivionVoicesRemastered (command line) - bring Oblivion Remastered's new voice recordings into Skyblivion.
The desktop app (SkyblivionVoicesRemastered.App.exe) provides the same steps with a graphical interface.

This tool supports multiple commands, each part of the overall pipeline (scan, map, build) and a command
to check the results (verify). Use `all` to run the entire pipeline.

Commands:
  scan   Inventory the Remaster install: new recordings, dialogue lines, NPC voice assignments -> scan.json
  map    Match those lines and NPCs onto a Skyblivion plugin -> map.json (+ unmatched.txt)
  build  Convert audio to .fuz (xWMA + lip) into voice type folders and write the patch plugin
  all    scan + map + build
  verify Read the generated mod folder and check it (plugin masters, voice types, NPC overrides, fuz files)
  synth  (dev) fabricate a stand-in {Constants.SkyblivionESM} from the Remaster data for testing
  tools  Show which game installs and Creation Kit tools were detected

Common options:
  --remaster <dir>       Oblivion Remastered install (auto-detected from Steam if not provided)
  --skyblivion <dir>     Skyrim SE Data folder that contains Skyblivion.esm (required for map/build)
  --plugin <names>       Skyblivion plugin(s), comma separated (default is {Constants.SkyblivionESM})
  --work <dir>           Working folder for scan.json/map.json (default is ./work)
  --out <dir>            Output mod folder for build (default is <work>/SkyblivionVoicesRemastered)
  --plugin-name <file>   Name of the generated patch plugin (default is {Constants.PluginName})

Build options:
  --lip generate|reuse|none   generate = Creation Kit LipGenerator (default), reuse = original Oblivion lip files
  --xwmaencode <exe>     Path to xWMAEncode.exe (default is Skyrim SE\Tools\Audio\xWMAEncode.exe)
  --lipgenerator <exe>   Path to LipGenerator.exe (default is Skyrim SE\Tools\LipGen\LipGenerator\LipGenerator.exe)
  --skyrim <dir>         Skyrim SE install folder used to find the tools above
  --xwm-bitrate <bps>    xWMA bitrate (default is the encoder's default)
  --parallel <n>         Worker count (default is half the available CPU cores)
  --group <keys>         Only build these voice groups, comma separated, e.g. "breton/f/altvoice,nord/m/beggar"
  --limit <n>            Only convert the first n recordings (for testing)
  --force                Rebuild files that already exist
  --all-voicetypes       Also emit voice types/files for groups that aren't used by any mapped NPC
  --no-esl               Do not flag the patch plugin as light (ESL); it is flagged by default
  --verbose              Print stack traces on errors

Synth options: --synth-out <file> --prefix <editorIdPrefix> --tag-fraction <0..1>
""");
}
