using System.Collections.Frozen;
using System.IO.Hashing;
using CommunityToolkit.HighPerformance;

namespace BotwModConverter.Core.Utils;

public static class HashUtils
{
    private static readonly Lazy<FrozenDictionary<ulong, HashSet<ulong>>> _wiiuHashes = new(() => Load("wiiu.hashes"));
    private static readonly Lazy<FrozenDictionary<ulong, HashSet<ulong>>> _switchHashes = new(() => Load("switch.hashes"));
    
    public static unsafe bool IsVanilla(string relativeFileName, ReadOnlySpan<byte> data, Platform platform)
    {
        var hashes = platform switch {
            Platform.Switch => _switchHashes.Value,
            Platform.WiiU => _wiiuHashes.Value,
            _ => throw new NotSupportedException($"Unsupported platform: {platform}")
        };

        var key = relativeFileName.ToCanon(trimToName: false);
        var keyHash = XxHash3.HashToUInt64(key.Cast<char, byte>());
        
        return hashes.TryGetValue(keyHash, out var value)
            && value.Contains(XxHash3.HashToUInt64(data));
    }

    private static FrozenDictionary<ulong, HashSet<ulong>> Load(string fileName)
    {
        Dictionary<ulong, HashSet<ulong>> hashes = [];

        using var stream = typeof(HashUtils).Assembly
            .GetManifestResourceStream($"BotwModConverter.Core.Resources.{fileName}")!;

        _ = stream.Read<int>();
        var count = stream.Read<int>();
        
        for (int i = 0; i < count; i++) {
            var key = stream.Read<ulong>();
            var entryCount = stream.Read<int>(); 
            var entry = hashes[key] = new HashSet<ulong>(entryCount);

            for (int j = 0; j < entryCount; j++) {
                entry.Add(stream.Read<ulong>());
            }
        }
        
        return hashes.ToFrozenDictionary();
    }
}