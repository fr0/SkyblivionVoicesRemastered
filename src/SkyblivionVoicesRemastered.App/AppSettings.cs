using System.Text.Json;

namespace SkyblivionVoicesRemastered.App;

/// <summary>
/// Persisted form state, stored per user under %LocalAppData%
/// </summary>
public class AppSettings
{
  public string RemasterPath { get; set; } = "";
  public string SkyblivionDataPath { get; set; } = "";
  public string SkyblivionPlugins { get; set; } = Constants.SkyblivionESM;
  public string WorkDir { get; set; } = "";
  public string OutputDir { get; set; } = "";
  public string PluginName { get; set; } = Constants.PluginName;
  public string XwmaEncodePath { get; set; } = "";
  public string LipGeneratorPath { get; set; } = "";
  public string LipMode { get; set; } = "Generate";
  public int Parallelism { get; set; } = Math.Max(1, Environment.ProcessorCount / 2);
  public bool AllVoiceTypes { get; set; }
  public bool Esl { get; set; } = true; // the patch only adds a few voice types, so it always fits the light range
  public bool Force { get; set; }
  public int Limit { get; set; }
  public bool ShowAdvanced { get; set; }

  private static string FilePath => Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
    "SkyblivionVoicesRemastered",
    "settings.json"
  );

  public static AppSettings Load()
  {
    try
    {
      if (File.Exists(FilePath))
        return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
    }
    catch
    {
      // corrupt settings are ignored (should probably log this though)
    }
    return new AppSettings();
  }

  private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

  public void Save()
  {
    try
    {
      Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
      File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonSerializerOptions));
    }
    catch
    {
      // not fatal, but we should probably log this somewhere?
    }
  }
}
