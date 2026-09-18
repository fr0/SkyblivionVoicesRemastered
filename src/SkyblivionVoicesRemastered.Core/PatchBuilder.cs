using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace SkyblivionVoicesRemastered;

public class PatchBuilder(SkyblivionEnvironment env, TextWriter log)
{
  private readonly SkyblivionEnvironment _env = env;
  private readonly TextWriter _log = log;

  const string Author = "SkyblivionVoicesRemastered";
  const string Description = "Adds Oblivion Remastered alternate and beggar voices to Skyblivion NPCs. Generated; do not edit by hand.";

  public PatchSummary Build(ScanResult scan, MapResult map, string outputPluginPath, bool includeUnusedVoiceTypes, bool esl)
  {
    var modKey = ModKey.FromFileName(Path.GetFileName(outputPluginPath)!);
    var patch = new SkyrimMod(modKey, SkyrimRelease.SkyrimSE);
    patch.ModHeader.Author = Author;
    patch.ModHeader.Description = Description;
    if (esl)
      patch.IsSmallMaster = true;

    var npcByEdid = map.Npcs
      .Where(n => n.IsMapped)
      .ToDictionary(n => n.OblivionEditorId, StringComparer.OrdinalIgnoreCase);
    var voiceTypes = new Dictionary<string, VoiceType>(StringComparer.OrdinalIgnoreCase);
    var summary = new PatchSummary();

    foreach (var group in scan.Groups.OrderBy(g => g.Key))
    {
      var npcs = scan.Npcs
        .Where(n => n.GroupKey == group.Key && npcByEdid.ContainsKey(n.EditorId))
        .ToList();
      if (npcs.Count == 0 && !includeUnusedVoiceTypes)
        continue;

      var vt = patch.VoiceTypes.AddNew(group.VoiceTypeEditorId);
      vt.Flags = VoiceType.Flag.AllowDefaultDialog | (group.Sex == "f" ? VoiceType.Flag.Female : 0);  // misogyny
      voiceTypes[group.Key] = vt;
      summary.VoiceTypes++;

      foreach (var npc in npcs)
      {
        var mapping = npcByEdid[npc.EditorId];
        var key = new FormKey(ModKey.FromFileName(mapping.TargetPlugin!), mapping.TargetFormIdLower);
        if (!_env.LinkCache.TryResolve<INpcGetter>(key, out var target))
        {
          // maybe we should be louder about this?
          summary.NpcsSkipped++;
          _log.WriteLine($"  NPC {npc.EditorId}: target {key} not found");
          continue;
        }
        var over = patch.Npcs.GetOrAddAsOverride(target);
        over.Voice.SetTo(vt);
        summary.NpcOverrides++;
      }
    }

    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPluginPath))!);
    patch.BeginWrite.ToPath(outputPluginPath).WithLoadOrder(_env.LoadOrder.ToArray()).Write();
    _log.WriteLine($"Wrote {outputPluginPath}: {summary.VoiceTypes} voice types, {summary.NpcOverrides} NPC overrides, light (ESL) flag {(patch.IsSmallMaster ? "set" : "not set")}");
    return summary;
  }
}

public class PatchSummary
{
  public int VoiceTypes { get; set; }
  public int NpcOverrides { get; set; }
  public int NpcsSkipped { get; set; }
}
