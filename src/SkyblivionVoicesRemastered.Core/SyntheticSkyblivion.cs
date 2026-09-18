using Mutagen.Bethesda;
using Ob = Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Development aid:
/// creates a stand-in "Skyblivion.esm" from the Remaster's Oblivion plugins so the mapping and build pipeline can be exercised before the real Skyblivion is available
/// Quests, topics, dialogue responses and the NPCs from a scan are recreated as Skyrim records with fresh form IDs and (optionally) prefixed editor IDs.
/// Will probably delete this when Skyblivion releases (or if I can get ahold of their data to check against)
/// </summary>
public static class SyntheticSkyblivion
{
  public static void Create(ScanResult scan, string outputPluginPath, string editorIdPrefix, double tagFraction, TextWriter log)
  {
    var dataPath = scan.RemasterDataPath;
    var mods = scan.Plugins
      .Select(p => Ob.OblivionMod.CreateFromBinaryOverlay(Path.Combine(dataPath, p), Ob.OblivionRelease.Oblivion))
      .ToList();
    var cache = mods.ToImmutableLinkCache<Ob.IOblivionMod, Ob.IOblivionModGetter>();

    var modKey = ModKey.FromFileName(Path.GetFileName(outputPluginPath));
    var sky = new SkyrimMod(modKey, SkyrimRelease.SkyrimSE);
    sky.ModHeader.Author = "Nobody";
    sky.ModHeader.Description = "Synthetic stand-in for testing SkyblivionVoicesRemastered";

    var neededInfos = scan.Lines
      .Where(l => l.InfoResolved)
      .Select(l => new FormKey(ModKey.FromFileName(l.InfoPlugin!), l.InfoFormIdLower))
      .ToHashSet();
    var quests = new Dictionary<FormKey, Quest>();
    var rng = new Random(12345);
    int topics = 0, infos = 0, responses = 0;

    foreach (var mod in mods)
    {
      foreach (var topic in mod.DialogTopics)
      {
        var items = topic.Items.Where(i => neededInfos.Contains(i.FormKey)).ToList();
        if (items.Count == 0)
          continue;
        // an Oblivion topic is shared by many quests; a Skyrim topic belongs to one quest
        var byQuest = items.GroupBy(i => i.Quest.FormKey).ToList();
        foreach (var questGroup in byQuest)
        {
          Quest? skyQuest = null;
          if (cache.TryResolve<Ob.IQuestGetter>(questGroup.Key, out var quest))
          {
            if (!quests.TryGetValue(quest.FormKey, out skyQuest))
            {
              skyQuest = sky.Quests.AddNew(editorIdPrefix + (quest.EditorID ?? $"Quest{quest.FormKey.ID:X6}"));
              skyQuest.Name = quest.Name ?? "";
              quests[quest.FormKey] = skyQuest;
            }
          }
          var topicEdid = topic.EditorID ?? $"Topic{topic.FormKey.ID:X6}";
          if (byQuest.Count > 1 && skyQuest != null)
            topicEdid = (quest!.EditorID ?? "") + topicEdid;
          var skyTopic = sky.DialogTopics.AddNew(editorIdPrefix + topicEdid);
          skyTopic.Name = topic.Name ?? "";
          skyTopic.Priority = 50;
          skyTopic.Category = DialogTopic.CategoryEnum.Topic;
          skyTopic.Subtype = DialogTopic.SubtypeEnum.Custom;
          skyTopic.SubtypeName = new RecordType("CUST");
          if (skyQuest != null)
            skyTopic.Quest.SetTo(skyQuest);
          topics++;
          foreach (var item in questGroup)
          {
            var info = new DialogResponses(sky.GetNextFormKey(), SkyrimRelease.SkyrimSE);
            if (rng.NextDouble() < tagFraction)
              info.EditorID = $"TES4_{item.FormKey.ID:X8}";
            foreach (var resp in item.Responses)
            {
              info.Responses.Add(new DialogResponse
              {
                ResponseNumber = resp.Data?.ResponseNumber ?? 0,
                Text = resp.ResponseText ?? "",
                Emotion = Emotion.Neutral,
                EmotionValue = 50
              });
              responses++;
            }
            skyTopic.Responses.Add(info);
            infos++;
          }
        }
      }
    }

    var voiceTypes = new Dictionary<string, VoiceType>();
    foreach (var npc in scan.Npcs)
    {
      var vtKey = $"{npc.RaceFolder}/{npc.Sex}";
      if (!voiceTypes.TryGetValue(vtKey, out var vt))
      {
        vt = sky.VoiceTypes.AddNew(editorIdPrefix + (npc.Sex == "f" ? "Female" : "Male") + npc.RaceEditorId);
        vt.Flags = VoiceType.Flag.AllowDefaultDialog | (npc.Sex == "f" ? VoiceType.Flag.Female : 0);
        voiceTypes[vtKey] = vt;
      }
      var skyNpc = sky.Npcs.AddNew(editorIdPrefix + npc.EditorId);
      skyNpc.Name = npc.Name ?? npc.EditorId;
      skyNpc.Voice.SetTo(vt);
      if (npc.Sex == "f")
        skyNpc.Configuration.Flags |= NpcConfiguration.Flag.Female;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPluginPath))!);
    sky.BeginWrite.ToPath(outputPluginPath).WithNoLoadOrder().Write();
    log.WriteLine($"Wrote synthetic {outputPluginPath}: {quests.Count} quests, {topics} topics, {infos} infos, {responses} responses, {scan.Npcs.Count} NPCs, {voiceTypes.Count} voice types");
  }
}
