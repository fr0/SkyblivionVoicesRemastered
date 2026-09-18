using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Result of scanning an Oblivion Remastered installation
/// </summary>
public class ScanResult
{
  public string RemasterDataPath { get; set; } = "";
  public DateTime ScannedAtUtc { get; set; }
  public List<string> Plugins { get; set; } = [];
  public List<VoiceLine> Lines { get; set; } = [];
  public List<NpcAssignment> Npcs { get; set; } = [];
  public List<VoiceGroup> Groups { get; set; } = [];
  public Dictionary<string, int> Stats { get; set; } = [];
  public List<string> Warnings { get; set; } = [];
}

public class VoiceLine
{
  /// <summary>Oblivion voice file base name without extension</summary>
  public string OblivionName { get; set; } = "";
  public string Archive { get; set; } = "";
  public string ArchivePath { get; set; } = "";
  public string? LipArchive { get; set; }
  public string? LipArchivePath { get; set; }
  /// <summary>Plugin folder in the archive path, e.g. oblivion.esm</summary>
  public string SourcePlugin { get; set; } = "";
  public string RaceFolder { get; set; } = "";
  public string Sex { get; set; } = "";
  public string Variant { get; set; } = "";
  public string QuestEditorId { get; set; } = "";
  public string TopicEditorId { get; set; } = "";
  public uint InfoFormIdLower { get; set; }
  public int ResponseNumber { get; set; }
  public string? InfoPlugin { get; set; }
  public string? ResponseText { get; set; }
  public bool InfoResolved { get; set; }

  [JsonIgnore]
  public string GroupKey => VoiceGroup.MakeKey(RaceFolder, Sex, Variant);
}

public class NpcAssignment
{
  public string EditorId { get; set; } = "";
  public string Plugin { get; set; } = "";
  public uint FormIdLower { get; set; }
  public string? Name { get; set; }
  public string RaceEditorId { get; set; } = "";
  public string RaceFolder { get; set; } = "";
  public string Sex { get; set; } = "";
  public string Variant { get; set; } = "";

  [JsonIgnore]
  public string GroupKey => VoiceGroup.MakeKey(RaceFolder, Sex, Variant);
}

/// <summary>
/// A race/sex/variant combination, which becomes one voice type in the patch
/// </summary>
public class VoiceGroup
{
  public string Key { get; set; } = "";
  public string RaceFolder { get; set; } = "";
  public string Sex { get; set; } = "";
  public string Variant { get; set; } = "";
  public int LineCount { get; set; }
  public int NpcCount { get; set; }
  public string VoiceTypeEditorId { get; set; } = "";

  public static string MakeKey(string raceFolder, string sex, string variant) => $"{raceFolder}/{sex}/{variant}".ToLowerInvariant();

  public static string MakeVoiceTypeEditorId(string raceFolder, string sex, string variant)
  {
    var race = string.Concat(raceFolder.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
    var variantName = variant.Equals("altvoice", StringComparison.OrdinalIgnoreCase) ? "Alt" : char.ToUpperInvariant(variant[0]) + variant[1..];
    var sexName = sex.Equals("f", StringComparison.OrdinalIgnoreCase) ? "Female" : "Male";
    return $"SVR{variantName}{sexName}{race}";
  }
}

/// <summary>
/// Result of mapping scanned lines and NPCs onto a Skyblivion plugin
/// </summary>
public class MapResult
{
  public string SkyblivionDataPath { get; set; } = "";
  public List<string> SkyblivionPlugins { get; set; } = [];
  public DateTime MappedAtUtc { get; set; }
  public List<LineMapping> Lines { get; set; } = [];
  public List<NpcMapping> Npcs { get; set; } = [];
  public Dictionary<string, int> Stats { get; set; } = [];
  public List<string> Warnings { get; set; } = [];
}

public class LineMapping
{
  public string OblivionName { get; set; } = "";
  public string Strategy { get; set; } = "unmatched";
  public string? TargetPlugin { get; set; }
  public uint TargetFormIdLower { get; set; }
  public string? TargetQuestEditorId { get; set; }
  public string? TargetTopicEditorId { get; set; }
  public int TargetResponseNumber { get; set; }
  public string? TargetText { get; set; }
  /// <summary>Skyrim voice file base name (without extension) inside the voice type folder</summary>
  public string? SkyrimName { get; set; }
  public double Confidence { get; set; }

  [JsonIgnore] public bool IsMapped => SkyrimName != null;
}

public class NpcMapping
{
  public string OblivionEditorId { get; set; } = "";
  public string Strategy { get; set; } = "unmatched";
  public string? TargetPlugin { get; set; }
  public uint TargetFormIdLower { get; set; }
  public string? TargetEditorId { get; set; }
  public string? CurrentVoiceType { get; set; }

  [JsonIgnore] public bool IsMapped => TargetEditorId != null;
}

static class Json
{
  public static readonly JsonSerializerOptions Options = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };

  public static void Save<T>(string path, T value)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
    using var fs = File.Create(path);
    JsonSerializer.Serialize(fs, value, Options);
  }

  public static T Load<T>(string path)
  {
    using var fs = File.OpenRead(path);
    return JsonSerializer.Deserialize<T>(fs, Options) ??
      throw new InvalidDataException($"Empty or invalid JSON: {path}");
  }
}
