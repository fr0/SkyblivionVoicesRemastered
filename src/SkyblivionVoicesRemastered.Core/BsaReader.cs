using Mutagen.Bethesda;
using Mutagen.Bethesda.Archives;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Read access to a Bethesda archive using Mutagen
/// </summary>
public class BsaReader : IDisposable
{
  private readonly IArchiveReader _reader;
  private readonly Dictionary<string, BsaEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

  public string Path { get; }
  public GameRelease Release { get; }
  public IReadOnlyCollection<BsaEntry> Entries => _entries.Values;

  // If release is not provided, we'll try to auto-detect it
  public BsaReader(string path, GameRelease? release = null)
  {
    Path = path;
    Release = release ?? DetectRelease(path);
    _reader = Archive.CreateReader(Release, path);
    foreach (var file in _reader.Files)
    {
      var full = file.Path.Replace('/', '\\'); // I hate Windows
      var slash = full.LastIndexOf('\\');
      var hasSlash = slash >= 0;
      var entry = new BsaEntry(!hasSlash ? "" : full[..slash], !hasSlash ? full : full[(slash + 1)..], file.Size, file);
      _entries[entry.FullPath] = entry;
    }
  }

  public bool TryGet(string fullPath, out BsaEntry entry) => _entries.TryGetValue(fullPath.Replace('/', '\\'), out entry!);

  public static byte[] Extract(BsaEntry entry) => entry.File.GetBytes();

  /// <summary>
  /// Maps the archive's header version to the game release
  /// </summary>
  public static GameRelease DetectRelease(string path)
  {
    Span<byte> header = stackalloc byte[8];
    using (var fs = File.OpenRead(path))
    {
      if (fs.Read(header) < 8)
        throw new InvalidDataException($"Not a BSA archive: {path}");
    }
    if (header[0] != 'B' || header[1] != 'S' || header[2] != 'A' || header[3] != 0)
      throw new InvalidDataException($"Not a BSA archive: {path}");
    var version = BitConverter.ToUInt32(header[4..]);
    return version switch
    {
      103 => GameRelease.Oblivion,
      104 => GameRelease.SkyrimLE,
      105 => GameRelease.SkyrimSE,
      _ => throw new InvalidDataException($"Unsupported BSA version {version}: {path}"),
    };
  }

  public void Dispose()
  {
    if (_reader is IDisposable d)
      d.Dispose();
    // no, I'm not using SuppressFinalize here, deal with it
  }
}

public record BsaEntry(string Folder, string FileName, uint Size, IArchiveFile File)
{
  public string FullPath => Folder.Length == 0 ? FileName : Folder + "\\" + FileName;
}
