using System.IO.Hashing;
using System.Runtime.CompilerServices;
using BotwModConverter.Core.Common;
using BotwModConverter.Core.Utils;
using CommunityToolkit.HighPerformance;
using CsYaz0;
using Revrs;
using SarcLibrary;

Dictionary<ulong, ulong> hashes = [];

foreach (var root in args[..^1]) {
    foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)) {
        if (!IsSarc(file, out bool isCompressed)) {
            continue;
        }

        Console.WriteLine(file);

        using var buffer = RentedBuffer.Rent(file);
        if (!isCompressed) {
            ProcessSarc(buffer, hashes);
            continue;
        }

        using var decompressed = RentedBuffer.Rent(Yaz0.GetDecompressedSize(buffer.Span));
        Yaz0.Decompress(buffer.Span, decompressed.Span);
        ProcessSarc(decompressed, hashes);
    }
}

using var fs = File.Create(args[^1]);
fs.Write(0x4C485642);
fs.Write(hashes.Count);

foreach (var (key, value) in hashes.OrderBy(x => x.Key)) {
    fs.Write(key);
    fs.Write(value);
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

    if (Unsafe.As<byte, int>(ref view[0]) == Sarc.MAGIC) {
        isCompressed = false;
        return true;
    }
    
    isCompressed = true;
    return Unsafe.As<byte, int>(ref view[0x11]) == Sarc.MAGIC;
}

static void ProcessSarc(RentedBuffer buffer, Dictionary<ulong, ulong> hashes)
{
    var reader = RevrsReader.Native(buffer.Span);
    var sarc = new ImmutableSarc(ref reader);

    foreach (var (name, data) in sarc) {
        var nameHash = XxHash3.HashToUInt64(name.ToCanon(trimToName: false).Cast<char, byte>());
        var dataHash = XxHash3.HashToUInt64(data);
        
        hashes[nameHash] = dataHash;
    }
}