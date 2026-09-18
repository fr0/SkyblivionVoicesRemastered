using System.Text.RegularExpressions;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Parsed voice line file name: quest_topic_formid_response (extension stripped)
/// </summary>
public record VoiceLineName(string QuestPart, string TopicPart, uint FormIdLower, int ResponseNumber)
{
  private static readonly Regex Pattern = new(@"^(?<q>[^_]*)_(?<t>.*)_(?<id>[0-9a-fA-F]{8})_(?<n>\d+)$", RegexOptions.Compiled);

  public static VoiceLineName? TryParse(string fileNameWithoutExtension)
  {
    var m = Pattern.Match(fileNameWithoutExtension);
    if (!m.Success)
      return null;
    return new VoiceLineName(m.Groups["q"].Value, m.Groups["t"].Value,
        Convert.ToUInt32(m.Groups["id"].Value, 16) & 0x00FFFFFF, int.Parse(m.Groups["n"].Value));
  }
}

public static class VoiceFileNaming
{
  /// <summary>
  /// Oblivion voice file base name. Quest and topic editor IDs are used verbatim; form ID is the record's local ID with a 00 index
  /// </summary>
  public static string Oblivion(string questEditorId, string topicEditorId, uint formIdLower, int responseNumber)
      => $"{questEditorId}_{topicEditorId}_{formIdLower & 0xFFFFFF:x8}_{responseNumber}".ToLowerInvariant();

  /// <summary>
  /// Skyrim voice file base name.
  /// When the combined quest_topic prefix exceeds 26 characters the Creation Kit truncates the quest ID to 10 characters
  /// and the topic ID to whatever remains (max 25 characters)
  /// </summary>
  public static string Skyrim(string questEditorId, string topicEditorId, uint formIdLower, int responseNumber)
  {
    var q = questEditorId;
    var t = topicEditorId;
    if (q.Length + 1 + t.Length > 26)
    {
      if (q.Length > 10)
        q = q[..10];
      var budget = 25 - q.Length;
      if (t.Length > budget)
        t = t[..budget];
    }
    return $"{q}_{t}_{formIdLower & 0xFFFFFF:x8}_{responseNumber}".ToLowerInvariant();
  }
}
