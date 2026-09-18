using System.Text;
using System.Text.RegularExpressions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Loads a Skyblivion plugin set (with masters) from a Skyrim Data folder
/// </summary>
public class SkyblivionEnvironment
{
  public string DataPath { get; }
  public IReadOnlyList<ISkyrimModGetter> LoadOrder { get; }
  public IReadOnlyList<ISkyrimModGetter> TargetPlugins { get; }
  public ILinkCache<ISkyrimMod, ISkyrimModGetter> LinkCache { get; }

  public SkyblivionEnvironment(string dataPath, IEnumerable<string> pluginNames, TextWriter log)
  {
    DataPath = Path.GetFullPath(dataPath);
    var readParams = new BinaryReadParameters { StringsParam = new StringsReadParameters { TargetLanguage = Language.English } };
    var loaded = new Dictionary<ModKey, ISkyrimModGetter>();
    var order = new List<ISkyrimModGetter>();
    var targets = new List<ISkyrimModGetter>();

    void Load(ModKey key, bool isTarget)
    {
      if (loaded.ContainsKey(key))
      {
        if (isTarget)
          targets.Add(loaded[key]);
        return;
      }

      var path = Path.Combine(DataPath, key.FileName);
      if (!File.Exists(path))
      {
        if (isTarget)
          throw new FileNotFoundException($"Plugin not found: {path}");
        log.WriteLine($"  master {key.FileName} not present in {DataPath}; links into it will not resolve");
        return;
      }

      var mod = SkyrimMod.CreateFromBinaryOverlay(path, SkyrimRelease.SkyrimSE, readParams);
      foreach (var master in mod.ModHeader.MasterReferences)
        Load(master.Master, false);
      loaded[key] = mod;
      order.Add(mod);
      if (isTarget)
        targets.Add(mod);
      log.WriteLine($"  loaded {key.FileName}");
    }

    foreach (var name in pluginNames)
      Load(ModKey.FromFileName(name), true);
    LoadOrder = order;
    TargetPlugins = targets;
    LinkCache = order.ToImmutableLinkCache<ISkyrimMod, ISkyrimModGetter>();
  }
}

/// <summary>
/// Matches Oblivion dialogue responses and NPCs onto their Skyblivion counterparts and derives the Skyrim voice file names
/// Several strategies are tried per line, from exact identifiers down to text similarity
/// (It's possible this is trying to be way too clever; I won't know until I actually get ahold of Skyblivion's data)
/// </summary>
public class SkyblivionMapper
{
  private readonly TextWriter _log;
  private readonly SkyblivionEnvironment _env;
  private readonly List<SkyEntry> _entries = [];
  private readonly Dictionary<uint, List<SkyEntry>> _byFormId = [];
  private readonly Dictionary<string, List<SkyEntry>> _byTag = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<string, List<SkyEntry>> _byQuestTopic = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<string, List<SkyEntry>> _byTopicSuffixKey = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<string, List<SkyEntry>> _byText = new(StringComparer.Ordinal);
  private readonly Dictionary<string, List<SkyEntry>> _byTextPrefix = new(StringComparer.Ordinal);
  private readonly Dictionary<string, INpcGetter> _npcByEdid = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<string, List<INpcGetter>> _npcBySuffix = new(StringComparer.OrdinalIgnoreCase);
  private readonly HashSet<string> _existingVoiceNames = new(StringComparer.OrdinalIgnoreCase);
  private int _voiceTypeConditionCount;

  public SkyblivionMapper(SkyblivionEnvironment env, TextWriter log)
  {
    _env = env;
    _log = log;
    BuildIndexes();
  }

  private void BuildIndexes()
  {
    foreach (var mod in _env.TargetPlugins)
    {
      foreach (var topic in mod.DialogTopics)
      {
        var questEdid = topic.Quest.TryResolve(_env.LinkCache, out var quest) ? quest.EditorID ?? "" : "";
        var topicEdid = topic.EditorID ?? "";
        foreach (var info in topic.Responses)
        {
          foreach (var cond in info.Conditions)
            if (cond.Data is IGetIsVoiceTypeConditionData)
              _voiceTypeConditionCount++;

          var firstResponseOrNull = info.Responses.FirstOrDefault();
          var notes = string.Join(" ", new[] {
            info.EditorID,
            firstResponseOrNull?.ScriptNotes,
            firstResponseOrNull?.Edits
          }.Where(s => !string.IsNullOrEmpty(s)));

          foreach (var resp in info.Responses)
          {
            var text = resp.Text?.String ?? "";
            var e = new SkyEntry(info.FormKey, questEdid, topicEdid, resp.ResponseNumber, text, NormalizeText(text));
            _entries.Add(e);
            Add(_byFormId, info.FormKey.ID, e);
            Add(_byQuestTopic, $"{questEdid}|{topicEdid}|{resp.ResponseNumber}", e);
            Add(_byText, e.NormalizedText, e);
            Add(_byTextPrefix, TextPrefix(e.NormalizedText), e);
            foreach (Match m in HexTag.Matches(notes + " " + (resp.ScriptNotes ?? "") + " " + (resp.Edits ?? "")))
              Add(_byTag, m.Value.Length == 8 ? m.Value[2..] : m.Value, e);
          }
        }
      }
      foreach (var npc in mod.Npcs)
      {
        if (npc.EditorID == null) continue;
        _npcByEdid.TryAdd(npc.EditorID, npc);
        foreach (var suffix in Suffixes(npc.EditorID)) Add(_npcBySuffix, suffix, npc);
      }
    }
    // topic suffix keys let a prefixed Skyblivion editor ID (e.g. TES4MQ15Greeting) match the Oblivion one
    foreach (var e in _entries)
      foreach (var suffix in Suffixes(e.TopicEditorId))
        Add(_byTopicSuffixKey, $"{suffix}|{e.ResponseNumber}", e);

    // existing voice files of the target plugins, to validate the naming rule against real Skyblivion data
    var pluginNames = _env.TargetPlugins.Select(p => p.ModKey.FileName.String).ToHashSet(StringComparer.OrdinalIgnoreCase);
    foreach (var bsaPath in Directory.EnumerateFiles(_env.DataPath, "*.bsa"))
    {
      var stem = Path.GetFileNameWithoutExtension(bsaPath);
      if (!pluginNames.Any(p => stem.StartsWith(Path.GetFileNameWithoutExtension(p), StringComparison.OrdinalIgnoreCase))) continue;
      try
      {
        using var reader = new BsaReader(bsaPath);
        foreach (var entry in reader.Entries)
          if (entry.Folder.StartsWith("sound\\voice\\", StringComparison.OrdinalIgnoreCase))
            _existingVoiceNames.Add(Path.GetFileNameWithoutExtension(entry.FileName));
      }
      catch (Exception ex)
      {
        _log.WriteLine($"  could not read {Path.GetFileName(bsaPath)}: {ex.Message}");
      }
    }
    foreach (var plugin in pluginNames)
    {
      var loose = Path.Combine(_env.DataPath, "Sound", "Voice", plugin);
      if (!Directory.Exists(loose))
        continue;
      foreach (var f in Directory.EnumerateFiles(loose, "*.*", SearchOption.AllDirectories))
        _existingVoiceNames.Add(Path.GetFileNameWithoutExtension(f));
    }
    _log.WriteLine($"Indexed {_entries.Count} Skyblivion responses, {_npcByEdid.Count} NPCs, {_existingVoiceNames.Count} existing voice files");
  }

  public MapResult Map(ScanResult scan)
  {
    var result = new MapResult
    {
      SkyblivionDataPath = _env.DataPath,
      MappedAtUtc = DateTime.UtcNow,
      SkyblivionPlugins = _env.TargetPlugins.Select(p => p.ModKey.FileName.String).ToList(),
    };
    if (_voiceTypeConditionCount > 0)
      result.Warnings.Add($"{_voiceTypeConditionCount} Skyblivion dialogue conditions use GetIsVoiceType; NPCs moved to a new voice type may lose those lines. Review before building.");

    var strategyCounts = new Dictionary<string, int>();
    _claimed.Clear();
    foreach (var line in scan.Lines.GroupBy(l => l.OblivionName).Select(g => g.First()).OrderBy(l => l.OblivionName, StringComparer.Ordinal))
      result.Lines.Add(MapLine(line));
    ResolveCollisions(result.Lines);
    foreach (var mapping in result.Lines)
      strategyCounts[mapping.Strategy] = strategyCounts.GetValueOrDefault(mapping.Strategy) + 1;
    foreach (var npc in scan.Npcs)
    {
      var mapping = MapNpc(npc);
      result.Npcs.Add(mapping);
      strategyCounts["npc:" + mapping.Strategy] = strategyCounts.GetValueOrDefault("npc:" + mapping.Strategy) + 1;
    }
    foreach (var kv in strategyCounts.OrderBy(k => k.Key)) result.Stats[kv.Key] = kv.Value;
    result.Stats["linesMapped"] = result.Lines.Count(l => l.IsMapped);
    result.Stats["linesUnmatched"] = result.Lines.Count(l => !l.IsMapped);
    result.Stats["npcsMapped"] = result.Npcs.Count(n => n.IsMapped);
    result.Stats["npcsUnmatched"] = result.Npcs.Count(n => !n.IsMapped);
    if (_existingVoiceNames.Count > 0)
    {
      var mapped = result.Lines.Where(l => l.IsMapped).ToList();
      var confirmed = mapped.Count(l => _existingVoiceNames.Contains(l.SkyrimName!));
      result.Stats["namesConfirmedByExistingFiles"] = confirmed;
      if (mapped.Count > 0 && confirmed < mapped.Count / 2)
        result.Warnings.Add($"Only {confirmed} of {mapped.Count} predicted Skyrim voice file names exist in Skyblivion's own voice files; the naming rule or the mapping may be off.");
    }
    return result;
  }

  // the confidence values are pretty much arbitrary
  private static class MappingStrategy
  {
    public const string Unmatched = "unmatched";

    public const string FormIdTag = "formid-tag";
    public const double FormIdTag_Confidence = 0.95;

    public const string QuestTopic = "quest-topic";
    public const double QuestTopic_Confidence = 0.9;

    public const string FormId = "formid";
    public const double FormId_Confidence_Min = 0.8;

    public const string TopicSuffix = "topic-suffix";

    public const string Text = "text";
    public const double Text_Confidence = 0.85;

    public const string TextAndTopic = "text+topic";
    public const double TextAndTopic_Confidence = 0.8;

    public const string TextFuzzy = "text-fuzzy";
    public const double TextFuzzy_Confidence_Multiplier = 0.9;

    public const string EditorId = "editorid";

    public const string EditorIdSuffix = "editorid-suffix";
  }

  // This is the clever part that's trying to avoid a bunch of manual mappings (or maybe just a bunch of nonsense, we'll find out soon enough)
  private LineMapping MapLine(VoiceLine line)
  {
    var m = new LineMapping { OblivionName = line.OblivionName };
    var oblivionText = NormalizeText(line.ResponseText ?? "");

    SkyEntry? best = null;
    string strategy = MappingStrategy.Unmatched;
    double confidence = 0;

    // 1. a Skyblivion record explicitly tagged with the Oblivion form ID
    if (_byTag.TryGetValue(line.InfoFormIdLower.ToString("x6"), out var tagged))
    {
      var c = tagged.Where(e => e.ResponseNumber == line.ResponseNumber).ToList();
      var pick = PickByText(c, oblivionText, out var sim);
      // Tags carry only the low 24 bits, which DLC plugins reuse, so the text has to agree too
      if (pick != null && (sim >= 0.5 || oblivionText.Length == 0)) 
      {
        best = pick;
        strategy = MappingStrategy.FormIdTag;
        confidence = Math.Max(sim, MappingStrategy.FormIdTag_Confidence);
      }
    }

    // 2. same quest and topic editor IDs and response number
    if (best == null && line.InfoResolved && _byQuestTopic.TryGetValue($"{line.QuestEditorId}|{line.TopicEditorId}|{line.ResponseNumber}", out var qt))
    {
      var pick = PickByText(qt, oblivionText, out var sim);
      if (pick != null && (qt.Count == 1 || sim >= 0.6)) // I dunno man
      {
        best = pick;
        strategy = MappingStrategy.QuestTopic;
        confidence = qt.Count == 1 ? Math.Max(MappingStrategy.QuestTopic_Confidence, sim) : sim;
      }
    }

    // 3. same local form ID (converters that preserve IDs) confirmed by text
    if (best == null && _byFormId.TryGetValue(line.InfoFormIdLower, out var sameId))
    {
      var c = sameId
        .Where(e => e.ResponseNumber == line.ResponseNumber)
        .ToList();
      var pick = PickByText(c, oblivionText, out var sim);
      if (pick != null && sim >= MappingStrategy.FormId_Confidence_Min)
      {
        best = pick;
        strategy = MappingStrategy.FormId;
        confidence = sim;
      }
    }

    // 4. topic editor ID as a suffix (prefixed IDs) confirmed by text
    if (best == null && line.InfoResolved && line.TopicEditorId.Length >= 4 && _byTopicSuffixKey.TryGetValue($"{line.TopicEditorId}|{line.ResponseNumber}", out var suffixed))
    {
      var c = line.QuestEditorId.Length == 0 ? suffixed
          : suffixed.Where(e => e.QuestEditorId.EndsWith(line.QuestEditorId, StringComparison.OrdinalIgnoreCase)).ToList();
      var pick = PickByText(c, oblivionText, out var sim);
      if (pick != null && sim >= MappingStrategy.FormId_Confidence_Min)
      {
        best = pick;
        strategy = MappingStrategy.TopicSuffix;
        confidence = sim;
      }
    }

    // 5. exact text match
    if (best == null && oblivionText.Length > 0 && _byText.TryGetValue(oblivionText, out var exact))
    {
      var c = exact.Where(e => e.ResponseNumber == line.ResponseNumber).ToList();
      if (c.Count == 0) c = exact;
      if (c.Count == 1)
      {
        best = c[0];
        strategy = MappingStrategy.Text;
        confidence = MappingStrategy.Text_Confidence;
      }
      else
      {
        // prefer a candidate whose topic resembles the Oblivion topic
        var topical = c.Where(e => e.TopicEditorId.Contains(line.TopicEditorId, StringComparison.OrdinalIgnoreCase))
          .ToList();
        if (topical.Count == 1) 
        {
          best = topical[0];
          strategy = MappingStrategy.TextAndTopic;
          confidence = MappingStrategy.TextAndTopic_Confidence;
        }
      }
    }

    // 6. fuzzy text match among entries sharing the first words
    if (best == null && oblivionText.Length >= 12 && _byTextPrefix.TryGetValue(TextPrefix(oblivionText), out var similar))
    {
      var pick = PickByText(similar.Where(e => e.ResponseNumber == line.ResponseNumber).ToList(), oblivionText, out var sim);
      if (pick != null && sim >= 0.9)
      {
        best = pick;
        strategy = MappingStrategy.TextFuzzy;
        confidence = sim * MappingStrategy.TextFuzzy_Confidence_Multiplier;
      }
    }

    if (best == null)
      return m;
    _claimed.Add(best);
    m.Strategy = strategy;
    m.Confidence = Math.Round(confidence, 3);
    m.TargetPlugin = best.FormKey.ModKey.FileName;
    m.TargetFormIdLower = best.FormKey.ID;
    m.TargetQuestEditorId = best.QuestEditorId;
    m.TargetTopicEditorId = best.TopicEditorId;
    m.TargetResponseNumber = best.ResponseNumber;
    m.TargetText = best.Text;
    m.SkyrimName = VoiceFileNaming.Skyrim(best.QuestEditorId, best.TopicEditorId, best.FormKey.ID, best.ResponseNumber);
    return m;
  }

  private NpcMapping MapNpc(NpcAssignment npc)
  {
    var m = new NpcMapping { OblivionEditorId = npc.EditorId };
    INpcGetter? target = null;
    var strategy = MappingStrategy.Unmatched;
    if (_npcByEdid.TryGetValue(npc.EditorId, out var exact))
    {
      target = exact;
      strategy = MappingStrategy.EditorId;
    }
    else if (_npcBySuffix.TryGetValue(npc.EditorId, out var bySuffix))
    {
      // prefer the candidate with the shortest prefix in front of the Oblivion ID; only accept an unambiguous winner
      var shortest = bySuffix.OrderBy(n => n.EditorID!.Length).Take(2).ToList();
      if (shortest.Count == 1 || shortest[0].EditorID!.Length < shortest[1].EditorID!.Length)
      {
        target = shortest[0];
        strategy = MappingStrategy.EditorIdSuffix;
      }
    }
    if (target == null)
      return m;
    m.Strategy = strategy;
    m.TargetPlugin = target.FormKey.ModKey.FileName;
    m.TargetFormIdLower = target.FormKey.ID;
    m.TargetEditorId = target.EditorID;
    m.CurrentVoiceType = target.Voice.TryResolve(_env.LinkCache, out var vt) ? vt.EditorID : null;
    return m;
  }

  /// <summary>
  /// Best candidate by text similarity; on ties an entry no other line has claimed yet is preferred
  /// </summary>
  private SkyEntry? PickByText(IReadOnlyList<SkyEntry> candidates, string normalizedText, out double similarity)
  {
    similarity = 0;
    SkyEntry? best = null;
    var bestClaimed = true;
    foreach (var c in candidates)
    {
      double s;
      if (c.NormalizedText == normalizedText)
        s = 1.0;
      else
      {
        // length alone bounds the similarity; skip candidates that cannot beat the current best
        var maxLen = Math.Max(normalizedText.Length, c.NormalizedText.Length);
        if (maxLen == 0)
          continue;
        var upperBound = 1.0 - (double)Math.Abs(normalizedText.Length - c.NormalizedText.Length) / maxLen;
        if (upperBound < similarity || upperBound < 0.5)
          continue;
        s = Similarity(normalizedText, c.NormalizedText);
      }
      var claimed = _claimed.Contains(c);
      if (s > similarity || (s == similarity && best != null && bestClaimed && !claimed))
      {
        similarity = s;
        best = c;
        bestClaimed = claimed;
        if (s >= 1.0 && !claimed)
          break;
      }
    }
    return best;
  }

  private readonly HashSet<SkyEntry> _claimed = new(ReferenceEqualityComparer.Instance);

  /// <summary>
  /// Two different Oblivion responses cannot both become the same Skyrim voice file. Keep the most confident
  /// mapping per target and mark the others as collisions so they are reported rather than silently overwritten.
  /// </summary>
  private static void ResolveCollisions(List<LineMapping> lines)
  {
    foreach (var group in lines.Where(l => l.IsMapped).GroupBy(l => l.SkyrimName!, StringComparer.OrdinalIgnoreCase))
    {
      if (group.Count() == 1) continue;
      foreach (var loser in group.OrderByDescending(l => l.Confidence).ThenBy(l => l.OblivionName, StringComparer.Ordinal).Skip(1))
      {
        loser.Strategy = "collision:" + loser.Strategy;
        loser.SkyrimName = null;
        loser.Confidence = 0;
      }
    }
  }

  private static readonly Regex HexTag = new(@"(?<![0-9a-fA-F])(?:[0-9a-fA-F]{8}|[0-9a-fA-F]{6})(?![0-9a-fA-F])", RegexOptions.Compiled);

  private static void Add<TKey, TValue>(Dictionary<TKey, List<TValue>> dict, TKey key, TValue value) where TKey : notnull
  {
    if (!dict.TryGetValue(key, out var list))
      dict[key] = list = [];
    list.Add(value);
  }

  private static IEnumerable<string> Suffixes(string editorId)
  {
    // Yield the ID itself and every suffix (at least 4 chars) that starts at a word boundary, so a prefixed
    // Skyblivion ID such as DABoethiaGREETING or TES4MQ15Greeting can be found from the Oblivion ID GREETING.
    yield return editorId;
    for (var i = 1; i <= editorId.Length - 4; i++)
      if ((char.IsUpper(editorId[i]) && !char.IsUpper(editorId[i - 1])) || (char.IsDigit(editorId[i]) && !char.IsDigit(editorId[i - 1])) || editorId[i - 1] == '_')
        yield return editorId[i..];
  }

  public static string NormalizeText(string text)
  {
    var sb = new StringBuilder(text.Length);
    var lastSpace = true;
    foreach (var ch in text.ToLowerInvariant())
    {
      if (char.IsLetterOrDigit(ch))
      {
        sb.Append(ch);
        lastSpace = false;
      }
      else if (!lastSpace)
      {
        sb.Append(' ');
        lastSpace = true;
      }
    }
    return sb.ToString().Trim();
  }

  private static string TextPrefix(string normalized) => normalized.Length <= 24 ? normalized : normalized[..24];

  /// <summary>
  /// Levenshtein similarity ratio in [0,1]
  /// https://en.wikipedia.org/wiki/Levenshtein_distance
  /// </summary>
  public static double Similarity(string a, string b)
  {
    if (a.Length == 0 && b.Length == 0)
      return 1;
    if (a.Length == 0 || b.Length == 0)
      return 0;
    var prev = new int[b.Length + 1];
    var cur = new int[b.Length + 1];
    for (var j = 0; j <= b.Length; j++) prev[j] = j;
    for (var i = 1; i <= a.Length; i++)
    {
      cur[0] = i;
      for (var j = 1; j <= b.Length; j++)
      {
        var cost = a[i - 1] == b[j - 1] ? 0 : 1;
        cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
      }
      (prev, cur) = (cur, prev);
    }
    return 1.0 - (double)prev[b.Length] / Math.Max(a.Length, b.Length);
  }

  private record SkyEntry(FormKey FormKey, string QuestEditorId, string TopicEditorId, int ResponseNumber, string Text, string NormalizedText);
}
