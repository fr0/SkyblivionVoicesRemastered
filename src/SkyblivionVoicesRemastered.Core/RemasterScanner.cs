using Mutagen.Bethesda;
using Mutagen.Bethesda.Archives;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Plugins;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Inventories an Oblivion Remastered installation: finds the alternate and beggar voice recordings in its archives,
/// resolves each to the dialogue response it belongs to, and lists the NPCs that use those voices
/// </summary>
public class RemasterScanner(TextWriter log)
{
  public static readonly string[] NewVoiceVariants = ["altvoice", "beggar"];

  private static readonly string[] PreferredPluginOrder =
  [
    "Oblivion.esm",
    "DLCBattlehornCastle.esp",
    "DLCFrostcrag.esp",
    "DLCHorseArmor.esp",
    "DLCMehrunesRazor.esp",
    "DLCOrrery.esp",
    "DLCShiveringIsles.esp",
    "DLCSpellTomes.esp",
    "DLCThievesDen.esp",
    "DLCVileLair.esp",
    "Knights.esp",
    "AltarESPMain.esp",
    "AltarDeluxe.esp"
  ];

  private readonly TextWriter _log = log;

  public static string ResolveDataPath(string remasterPath)
  {
    var candidates = new[]
    {
      remasterPath,
      Path.Combine(remasterPath, "Data"),
      Path.Combine(remasterPath, "OblivionRemastered", "Content", "Dev", "ObvData", "Data"),
      Path.Combine(remasterPath, "Content", "Dev", "ObvData", "Data")
    };
    foreach (var c in candidates)
      if (File.Exists(Path.Combine(c, "Oblivion.esm")))
        return Path.GetFullPath(c);
    throw new DirectoryNotFoundException($"Could not find Oblivion.esm under '{remasterPath}'. Pass the Oblivion Remastered install folder or its ObvData\\Data folder.");
  }

  public ScanResult Scan(string remasterPath)
  {
    var dataPath = ResolveDataPath(remasterPath);
    var result = new ScanResult { RemasterDataPath = dataPath, ScannedAtUtc = DateTime.UtcNow };

    var pluginFiles = PreferredPluginOrder
      .Where(p => File.Exists(Path.Combine(dataPath, p)))
      .ToList();
    if (!pluginFiles.Contains("AltarESPMain.esp"))
      result.Warnings.Add("AltarESPMain.esp not found; alternate voice NPC assignments come from that plugin.");
    result.Plugins = pluginFiles;
    _log.WriteLine($"Loading {pluginFiles.Count} plugins from {dataPath}");
    var mods = pluginFiles
      .Select(p => OblivionMod.CreateFromBinaryOverlay(Path.Combine(dataPath, p), OblivionRelease.Oblivion))
      .ToList();
    var cache = mods.ToImmutableLinkCache<IOblivionMod, IOblivionModGetter>();

    // dialogue index keyed by Oblivion voice file base name
    // Some esps override dialogue records with a localization key, others (Oblivion.esm for example) have
    // the real English text. Discovered this the hard way.
    var infoIndex = new Dictionary<string, InfoEntry>(StringComparer.OrdinalIgnoreCase);
    var localizationKeys = 0;
    foreach (var mod in mods)
    {
      foreach (var topic in mod.DialogTopics)
      {
        var topicEdid = topic.EditorID ?? "";
        foreach (var item in topic.Items)
        {
          var questEdid = item.Quest.TryResolve(cache, out var quest) ? quest.EditorID ?? "" : "";
          foreach (var resp in item.Responses)
          {
            var responseNumber = resp.Data?.ResponseNumber ?? 0; // what would cause resp.Data to be null? this is the lazy solution to handling that
            var name = VoiceFileNaming.Oblivion(questEdid, topicEdid, item.FormKey.ID, responseNumber);
            var text = resp.ResponseText ?? "";
            if (IsLocalizationKey(text))
            {
              localizationKeys++;
              if (infoIndex.TryGetValue(name, out var previous) && !IsLocalizationKey(previous.Text))
                text = previous.Text;
            }
            infoIndex[name] = new InfoEntry(questEdid, topicEdid, item.FormKey, responseNumber, text);
          }
        }
      }
    }
    var unresolvedKeys = infoIndex.Values.Count(e => IsLocalizationKey(e.Text));
    _log.WriteLine($"Indexed {infoIndex.Count} dialogue responses ({localizationKeys} localization-key overrides seen, {unresolvedKeys} responses have no real text)");

    // race folder names and the race whose recordings a race shares in the original game
    var raceFolders = new Dictionary<FormKey, string>();
    var voiceSourceFolder = new Dictionary<(string folder, string sex), string>();
    foreach (var race in cache.PriorityOrder.WinningOverrides<IRaceGetter>())
    {
      var folder = RaceFolderName(race);
      if (folder.Length == 0)
        continue;
      raceFolders[race.FormKey] = folder;
    }
    foreach (var race in cache.PriorityOrder.WinningOverrides<IRaceGetter>())
    {
      if (!raceFolders.TryGetValue(race.FormKey, out var folder) || race.Voices == null)
        continue;
      if (race.Voices.Male.TryResolve(cache, out var vm) && raceFolders.TryGetValue(vm.FormKey, out var mf))
        voiceSourceFolder[(folder, "m")] = mf;
      if (race.Voices.Female.TryResolve(cache, out var vf) && raceFolders.TryGetValue(vf.FormKey, out var ff))
        voiceSourceFolder[(folder, "f")] = ff;
    }

    // Archives: collect new-variant recordings and every lip file
    var lipIndex = new Dictionary<string, (string archive, string path)>(StringComparer.OrdinalIgnoreCase); // plugin\race\sex\name -> lip
    // group|name -> line (later archives win)
    // archives load with their plugin ("DLCThievesDen.bsa" with DLCThievesDen.esp, "Oblivion - *.bsa" with Oblivion.esm)
    // so a path present in two archives is served from the later plugin's archive
    // Mutagen knows the game's rule
    var lineIndex = new Dictionary<string, VoiceLine>(StringComparer.OrdinalIgnoreCase);
    var archiveOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < pluginFiles.Count; i++)
    {
      foreach (var archive in Archive.GetApplicableArchivePaths(GameRelease.Oblivion, dataPath, ModKey.FromFileName(pluginFiles[i])))
        archiveOrder[Path.GetFullPath(archive)] = i;
    }
    foreach (var bsaPath in Directory.EnumerateFiles(dataPath, "*.bsa")
                 .OrderBy(p => archiveOrder.GetValueOrDefault(Path.GetFullPath(p), -1))
                 .ThenBy(p => p, StringComparer.OrdinalIgnoreCase))
    {
      BsaReader reader;
      try
      {
        reader = new BsaReader(bsaPath);
      }
      catch (Exception ex)
      {
        result.Warnings.Add($"Skipped archive {Path.GetFileName(bsaPath)}: {ex.Message}");
        continue;
      }

      var archive = Path.GetFileName(bsaPath);
      var found = 0;
      foreach (var entry in reader.Entries)
      {
        var parts = entry.Folder.Split('\\');
        if (parts.Length < 5 || !parts[0].Equals("sound", StringComparison.OrdinalIgnoreCase) || !parts[1].Equals("voice", StringComparison.OrdinalIgnoreCase))
          continue;
        var plugin = parts[2].ToLowerInvariant();
        var race = parts[3].ToLowerInvariant();
        var sex = parts[4].ToLowerInvariant();
        var ext = Path.GetExtension(entry.FileName).ToLowerInvariant();
        var baseName = Path.GetFileNameWithoutExtension(entry.FileName).ToLowerInvariant();
        if (parts.Length == 5 && ext == ".lip")
        {
          lipIndex[$"{plugin}\\{race}\\{sex}\\{baseName}"] = (archive, entry.FullPath);
          continue;
        }
        if (parts.Length != 6 || ext != ".mp3")
          continue;
        var variant = parts[5].ToLowerInvariant();
        if (!NewVoiceVariants.Contains(variant))
          continue;
        var parsed = VoiceLineName.TryParse(baseName);
        var line = new VoiceLine
        {
          OblivionName = baseName,
          Archive = archive,
          ArchivePath = entry.FullPath,
          SourcePlugin = plugin,
          RaceFolder = race,
          Sex = sex,
          Variant = variant,
        };
        if (parsed != null)
        {
          line.InfoFormIdLower = parsed.FormIdLower;
          line.ResponseNumber = parsed.ResponseNumber;
          line.QuestEditorId = parsed.QuestPart;
          line.TopicEditorId = parsed.TopicPart;
        }
        if (infoIndex.TryGetValue(baseName, out var info))
        {
          line.InfoResolved = true;
          line.QuestEditorId = info.QuestEditorId;
          line.TopicEditorId = info.TopicEditorId;
          line.InfoFormIdLower = info.FormKey.ID;
          line.InfoPlugin = info.FormKey.ModKey.FileName;
          line.ResponseNumber = info.ResponseNumber;
          line.ResponseText = info.Text;
        }
        lineIndex[$"{line.GroupKey}|{baseName}"] = line;
        found++;
      }
      if (found > 0) _log.WriteLine($"  {archive}: {found} new voice recordings");
    }
    var lines = lineIndex.Values.ToList();

    // attach lip files from the base recording of the same line (same race, or the race it shared a voice with)
    foreach (var line in lines)
    {
      var keys = new List<string> { $"{line.SourcePlugin}\\{line.RaceFolder}\\{line.Sex}\\{line.OblivionName}" };
      if (voiceSourceFolder.TryGetValue((line.RaceFolder, line.Sex), out var src))
        keys.Add($"{line.SourcePlugin}\\{src}\\{line.Sex}\\{line.OblivionName}");
      foreach (var key in keys)
      {
        if (!lipIndex.TryGetValue(key, out var lip)) continue;
        line.LipArchive = lip.archive;
        line.LipArchivePath = lip.path;
        break;
      }
    }

    // NPC assignments
    var altFaction = cache.PriorityOrder.WinningOverrides<IFactionGetter>().FirstOrDefault(f => string.Equals(f.EditorID, "AltVoiceFaction", StringComparison.OrdinalIgnoreCase));
    var beggarFactions = cache.PriorityOrder.WinningOverrides<IFactionGetter>()
        .Where(f => f.EditorID is "Beggars" or "BeggarsShivering").Select(f => f.FormKey)
        .ToHashSet();
    if (altFaction == null)
      result.Warnings.Add("AltVoiceFaction not found in the loaded plugins; no alternate voice NPCs will be assigned.");
    foreach (var npc in cache.PriorityOrder.WinningOverrides<INpcGetter>())
    {
      var isBeggar = npc.Factions.Any(f => beggarFactions.Contains(f.Faction.FormKey));
      var isAlt = altFaction != null && npc.Factions.Any(f => f.Faction.FormKey == altFaction.FormKey);
      if (!isBeggar && !isAlt)
        continue; // what?
      if (!npc.Race.TryResolve(cache, out var race) || !raceFolders.TryGetValue(race.FormKey, out var raceFolder))
        continue;
      var sex = npc.Configuration != null && npc.Configuration.Flags.HasFlag(Npc.NpcFlag.Female) ? "f" : "m"; // misogyny
      var name = npc.Name;
      if (name != null && name.StartsWith("LOC_", StringComparison.OrdinalIgnoreCase))
        name = null; // remaster plugins have localization keys, not names
      result.Npcs.Add(new NpcAssignment
      {
        EditorId = npc.EditorID ?? npc.FormKey.ToString(),
        Plugin = npc.FormKey.ModKey.FileName,
        FormIdLower = npc.FormKey.ID,
        Name = name,
        RaceEditorId = race.EditorID ?? "",
        RaceFolder = raceFolder,
        Sex = sex,
        Variant = isBeggar ? "beggar" : "altvoice"
      });
    }

    result.Lines = lines
      .OrderBy(l => l.GroupKey)
      .ThenBy(l => l.OblivionName)
      .ToList();
    result.Npcs = result.Npcs
      .OrderBy(n => n.GroupKey)
      .ThenBy(n => n.EditorId)
      .ToList();
    foreach (var g in lines.GroupBy(l => l.GroupKey).OrderBy(g => g.Key))
    {
      var first = g.First();
      result.Groups.Add(new VoiceGroup
      {
        Key = g.Key,
        RaceFolder = first.RaceFolder,
        Sex = first.Sex,
        Variant = first.Variant,
        LineCount = g.Count(),
        NpcCount = result.Npcs.Count(n => n.GroupKey == g.Key),
        VoiceTypeEditorId = VoiceGroup.MakeVoiceTypeEditorId(first.RaceFolder, first.Sex, first.Variant),
      });
    }
    result.Stats["lines"] = lines.Count;
    result.Stats["linesWithDialogue"] = lines.Count(l => l.InfoResolved);
    result.Stats["linesWithLip"] = lines.Count(l => l.LipArchivePath != null);
    result.Stats["npcs"] = result.Npcs.Count;
    result.Stats["groups"] = result.Groups.Count;
    return result;
  }

  /// <summary>
  /// Voice archive folder for a race. Oblivion names them after the race's display name in lower case ("dark elf");
  /// the Remaster plugins only carry localization keys as names, so derive it from the editor ID (DarkElf -> "dark elf")
  /// </summary>
  public static string RaceFolderName(IRaceGetter race)
  {
    var name = race.Name;
    if (!string.IsNullOrWhiteSpace(name) && !name.StartsWith("LOC_", StringComparison.OrdinalIgnoreCase))
      return name.Trim().ToLowerInvariant();
    var edid = race.EditorID ?? "";
    var words = new List<string>();
    var start = 0;
    for (var i = 1; i <= edid.Length; i++)
    {
      if (i == edid.Length || (char.IsUpper(edid[i]) && !char.IsUpper(edid[i - 1])))
      {
        words.Add(edid[start..i]);
        start = i;
      }
    }
    if (words.Count > 1 && words[^1].Equals("Race", StringComparison.OrdinalIgnoreCase))
      words.RemoveAt(words.Count - 1);
    return string.Join(' ', words).ToLowerInvariant();
  }

  // the Remaster's plugins replace display strings with keys such as LOC_RT_... / LOC_FN_... that are resolved on the Unreal side
  public static bool IsLocalizationKey(string? text) => text != null && text.StartsWith("LOC_", StringComparison.OrdinalIgnoreCase);

  private record InfoEntry(string QuestEditorId, string TopicEditorId, FormKey FormKey, int ResponseNumber, string Text);
}
