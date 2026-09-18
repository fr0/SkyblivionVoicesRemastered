using System.Text.RegularExpressions;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Finds the Creation Kit audio tools (xWMAEncode, LipGenerator) shipped with Skyrim Special Edition
/// This probably Windows-only; luckily, users can run this stuff on Windows and then use the outputted mod on Linux
/// (assuming Skyblivion even works on Linux?)
/// </summary>
public static class ToolLocator
{
  public static string? HKCURegistryString(string path, string name)
  {
    if (!OperatingSystem.IsWindows()) // sorry Linux users
      return null;
    using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(path);
    if (key?.GetValue("SteamPath") is string p)
      return p;
    return null;
  }

  public static IEnumerable<string> SteamLibraries()
  {
    var steamRoots = new List<string>();
    // most common cases
    foreach (var root in new[] { @"C:\Program Files (x86)\Steam", @"C:\Program Files\Steam" })
      if (Directory.Exists(root))
        steamRoots.Add(root);
    try
    {
      var p = HKCURegistryString(@"Software\Valve\Steam", "SteamPath");
      if (Directory.Exists(p))
        steamRoots.Insert(0, Path.GetFullPath(p));
    }
    catch
    {
      // hopefully the vdf detection below will work...
    }

    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var root in steamRoots)
    {
      if (seen.Add(root))
        yield return root;
      var vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
      if (!File.Exists(vdf))
        continue;
      // https://developer.valvesoftware.com/wiki/VDF
      // not gonna properly parse the thing here, so we're just doing a quick+dirty regex instead
      foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s+\"([^\"]+)\""))
      {
        var lib = m.Groups[1].Value.Replace(@"\\", @"\");
        if (Directory.Exists(lib) && seen.Add(lib))
          yield return lib;
      }
    }
  }

  public static string? FindSkyrimSpecialEdition()
  {
    foreach (var lib in SteamLibraries())
    {
      var p = Path.Combine(lib, "steamapps", "common", "Skyrim Special Edition");
      if (File.Exists(Path.Combine(p, "SkyrimSE.exe")))
        return p;
    }
    return null;
  }

  public static string? FindOblivionRemastered()
  {
    foreach (var lib in SteamLibraries())
    {
      var p = Path.Combine(lib, "steamapps", "common", "Oblivion Remastered");
      if (File.Exists(Path.Combine(p, "OblivionRemastered.exe")))
        return p;
    }
    return null;
  }

  /// <summary>
  /// Looks for a tool next to the given Skyrim install (or any detected one), then on PATH
  /// </summary>
  public static string? FindTool(string relativeToolPath, string? skyrimRoot)
  {
    var roots = new List<string>();

    if (skyrimRoot != null)
      roots.Add(skyrimRoot);

    var detected = FindSkyrimSpecialEdition();
    if (detected != null)
      roots.Add(detected);

    foreach (var root in roots)
    {
      var p = Path.Combine(root, relativeToolPath);
      if (File.Exists(p))
        return p;
    }

    var exe = Path.GetFileName(relativeToolPath);
    foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
      var p = Path.Combine(dir.Trim(), exe);
      if (File.Exists(p))
        return p;
    }
    return null;
  }

  // this is where they live on my machine, at least...
  public const string XwmaEncodeRelative = @"Tools\Audio\xWMAEncode.exe";
  public const string LipGeneratorRelative = @"Tools\LipGen\LipGenerator\LipGenerator.exe";
}
