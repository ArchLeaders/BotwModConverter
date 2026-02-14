using System.IO.Hashing;
using System.Runtime.CompilerServices;
using BotwModConverter.Core.Common;
using BotwModConverter.Core.Utils;
using CommunityToolkit.HighPerformance;
using CsYaz0;
using Revrs;
using SarcLibrary;

Dictionary<ulong, HashSet<ulong>> hashes = [];

foreach (var root in args[..^1]) {
    foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)) {
        if (!IsSarc(file, out bool isCompressed)) {
            continue;
        }

        string relativeFilePath = Path.GetRelativePath(root, file);
        Console.WriteLine(relativeFilePath);

        using var buffer = RentedBuffer.Rent(file);
        if (!isCompressed) {
            ProcessSarc(buffer.Span, hashes);
            continue;
        }

        using var decompressed = RentedBuffer.Rent(Yaz0.GetDecompressedSize(buffer.Span));
        var data = decompressed.Span;
        Yaz0.Decompress(buffer.Span, data);
        ProcessSarc(data, hashes);
    }
}

using var fs = File.Create(args[^1]);
fs.Write(0x4C485642);
fs.Write(hashes.Count);

foreach (var (key, value) in hashes.OrderBy(x => x.Key)) {
    fs.Write(key);
    fs.Write(value.Count);
    foreach (ulong hash in value) {
        fs.Write(hash);
    }
}

return;

static bool IsSarc(string filePath, out bool isCompressed)
{
    using var fs = File.OpenRead(filePath);
    if (fs.Length < 0x20) {
        isCompressed = false;
        return false;
    }
    
    Span<byte> view = stackalloc byte[0x20]; 
    _ = fs.Read(view);

    return IsSarcFromView(view, out isCompressed);
}

static bool IsSarcFromView(Span<byte> view, out bool isCompressed)
{
    if (Unsafe.As<byte, int>(ref view[0]) == Sarc.MAGIC) {
        isCompressed = false;
        return true;
    }
    
    isCompressed = true;
    return Unsafe.As<byte, int>(ref view[0x11]) == Sarc.MAGIC;
}

static void ProcessFile(string relativeFilePath, Span<byte> buffer, Dictionary<ulong, HashSet<ulong>> hashes)
{
    if (relativeFilePath.EndsWith("Demo002_0.sbfres")) {
        File.WriteAllBytes("D:\\bin\\debug.sbfres", buffer);
    }
    
    if (!IsSarcFromView(buffer, out bool isCompressed)) {
        return;
    }

    Console.WriteLine(relativeFilePath);

    if (!isCompressed) {
        ProcessSarc(buffer, hashes);
        return;
    }

    using var decompressed = RentedBuffer.Rent(Yaz0.GetDecompressedSize(buffer));
    var data = decompressed.Span;
    Yaz0.Decompress(buffer, data);
    ProcessSarc(data, hashes);
}

static void ProcessSarc(Span<byte> buffer, Dictionary<ulong, HashSet<ulong>> hashes)
{
    var reader = RevrsReader.Native(buffer);
    var sarc = new ImmutableSarc(ref reader);

    foreach (var (name, data) in sarc) {
        var nameHash = XxHash3.HashToUInt64(name.ToCanon(trimToName: false).Cast<char, byte>());
        var dataHash = XxHash3.HashToUInt64(data);
        
        ProcessFile(name, data, hashes);

        if (!hashes.TryGetValue(nameHash, out var value)) {
            hashes[nameHash] = [dataHash];
            continue;
        }
        
        value.Add(dataHash);
    }
}