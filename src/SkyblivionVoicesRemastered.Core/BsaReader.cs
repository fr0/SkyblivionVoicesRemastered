using System.IO.Compression;
using System.Text;
using Mutagen.Bethesda;

namespace SkyblivionVoicesRemastered;

/// <summary>
/// Read access to a Bethesda archive (Oblivion v103, Skyrim LE v104, Skyrim SE v105).
///
/// Originally I used Mutagen's archive reader, but it can't read zlib-compressed Oblivion entries
/// (was getting "InflaterInputStream Length is not supported"). Some of the Remaster's bsa's are zlib-compressed archives (DLCVileLair and DLCOrrery).
/// </summary>
public class BsaReader
{
  private const uint FlagFolderNames = 0x1;
  private const uint FlagFileNames = 0x2;
  private const uint FlagCompressedByDefault = 0x4;
  private const uint FlagEmbedFileNames = 0x100;
  private const uint SizeCompressionToggle = 0x40000000;
  private const uint SizeMask = 0x3FFFFFFF;

  private readonly Dictionary<string, BsaEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
  private readonly bool _embedNames;
  private readonly uint _version;

  public string Path { get; }
  public GameRelease Release { get; }
  public IReadOnlyCollection<BsaEntry> Entries => _entries.Values;

  // If release is not provided, we'll try to auto-detect it
  public BsaReader(string path, GameRelease? release = null)
  {
    Path = path;
    using var fs = File.OpenRead(path);
    using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);
    var magic = br.ReadBytes(4);
    if (magic.Length < 4 || !Utilities.CheckHeader(magic, "BSA") || magic[3] != 0)
      throw new InvalidDataException($"Not a BSA archive: {path}");
    _version = br.ReadUInt32();
    Release = release ?? VersionToGameRelease(_version, path);
    var folderRecordOffset = br.ReadUInt32();
    var flags = br.ReadUInt32();
    var folderCount = br.ReadUInt32();
    var fileCount = br.ReadUInt32();
    _ = br.ReadUInt32(); // total folder name length (don't care)
    var totalFileNameLength = br.ReadUInt32();
    _ = br.ReadUInt32(); // file flags (don't care)
    var compressedByDefault = (flags & FlagCompressedByDefault) != 0;
    _embedNames = _version >= 104 && (flags & FlagEmbedFileNames) != 0;
    if ((flags & FlagFolderNames) == 0 || (flags & FlagFileNames) == 0)
      throw new InvalidDataException($"BSA without folder/file names is not supported: {path}");

    // folder records: hash, file count, (v105 has padding + 64-bit offset, otherwise, 32-bit offset)
    fs.Seek(folderRecordOffset, SeekOrigin.Begin);
    var folderFileCounts = new uint[folderCount];
    for (var i = 0; i < folderCount; i++)
    {
      _ = br.ReadUInt64();
      folderFileCounts[i] = br.ReadUInt32();
      if (_version >= 105) 
      {
        _ = br.ReadUInt32();
        _ = br.ReadUInt64();
      }
      else
        _ = br.ReadUInt32();
    }

    // file record blocks: folder name (length-prefixed AND null-terminated) followed by hash/size/offset per file
    var folderNames = new string[folderCount];
    var records = new List<(int folder, uint size, uint offset)>((int)fileCount);
    for (var i = 0; i < folderCount; i++)
    {
      var len = br.ReadByte();
      var nameBytes = br.ReadBytes(len);
      folderNames[i] = Encoding.ASCII.GetString(nameBytes, 0, Math.Max(0, len - 1));
      for (var j = 0; j < folderFileCounts[i]; j++)
      {
        _ = br.ReadUInt64();
        var size = br.ReadUInt32();
        var offset = br.ReadUInt32();
        records.Add((i, size, offset));
      }
    }

    // file name block: null-separated names in record order
    var nameBlock = br.ReadBytes((int)totalFileNameLength);
    var names = new List<string>((int)fileCount);
    var start = 0;
    for (var i = 0; i < nameBlock.Length; i++)
    {
      if (nameBlock[i] != 0)
        continue;
      names.Add(Encoding.ASCII.GetString(nameBlock, start, i - start));
      start = i + 1;
    }
    if (names.Count != records.Count)
      throw new InvalidDataException($"BSA name count {names.Count} != record count {records.Count}: {path}");

    for (var i = 0; i < records.Count; i++)
    {
      var (folder, size, offset) = records[i];
      var compressed = compressedByDefault ^ ((size & SizeCompressionToggle) != 0);
      var entry = new BsaEntry(this, folderNames[folder], names[i], size & SizeMask, offset, compressed);
      _entries[entry.FullPath] = entry;
    }
  }

  private static GameRelease VersionToGameRelease(uint version, string path)
  {
    return version switch
    {
      103 => GameRelease.Oblivion,
      104 => GameRelease.SkyrimLE,
      105 => GameRelease.SkyrimSE,
      _ => throw new InvalidDataException($"Unsupported BSA version {version}: {path}")
    };
  }

  public bool TryGet(string fullPath, out BsaEntry entry) => _entries.TryGetValue(fullPath.Replace('/', '\\'), out entry!);

  public static byte[] Extract(BsaEntry entry) => entry.Archive.Read(entry);

  private byte[] Read(BsaEntry entry)
  {
    using var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16);
    using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);
    fs.Seek(entry.Offset, SeekOrigin.Begin);
    var size = (int)entry.Size;
    if (_embedNames)
    {
      var len = br.ReadByte();
      _ = br.ReadBytes(len);
      size -= len + 1;
    }
    if (!entry.Compressed)
      return br.ReadBytes(size);

    var originalSize = br.ReadInt32();
    var payload = br.ReadBytes(size - 4);
    var result = new byte[originalSize];
    if (_version >= 105)
    {
      using var decoded = K4os.Compression.LZ4.Streams.LZ4Stream.Decode(new MemoryStream(payload));
      ReadFully(decoded, result);
    }
    else
    {
      using var zs = new ZLibStream(new MemoryStream(payload), CompressionMode.Decompress);
      ReadFully(zs, result);
    }
    return result;
  }

  private static void ReadFully(Stream s, byte[] buffer)
  {
    var read = 0;
    while (read < buffer.Length)
    {
      var n = s.Read(buffer, read, buffer.Length - read);
      if (n <= 0)
        break;
      read += n;
    }
  }
}

public record BsaEntry(BsaReader Archive, string Folder, string FileName, uint Size, uint Offset, bool Compressed)
{
  public string FullPath => Folder.Length == 0 ? FileName : Folder + "\\" + FileName;
}
