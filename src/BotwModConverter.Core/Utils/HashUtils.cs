using System.Collections.Frozen;
using System.IO.Hashing;
using CommunityToolkit.HighPerformance;

namespace BotwModConverter.Core.Utils;

public static class HashUtils
{
    private static readonly Lazy<FrozenDictionary<ulong, ulong>> _wiiuHashes = new(() => Load("switch.hashes"));
    private static readonly Lazy<FrozenDictionary<ulong, ulong>> _switchHashes = new(() => Load("switch.hashes"));
    
    public static bool IsVanilla(string relativeFileName, ReadOnlySpan<byte> data, Platform platform)
    {
        var hashes = platform switch {
            Platform.Switch => _switchHashes.Value,
            Platform.WiiU => _wiiuHashes.Value,
            _ => throw new NotSupportedException($"Unsupported platform: {platform}")
        };

        return hashes.TryGetValue(XxHash3.HashToUInt64(relativeFileName.ToCanon().Cast<char, byte>()), out var value)
            && value == XxHash3.HashToUInt64(data);
    }

    private static FrozenDictionary<ulong, ulong> Load(string fileName)
    {
        Dictionary<ulong, ulong> hashes = [];

        using var stream = typeof(HashUtils).Assembly
            .GetManifestResourceStream($"BotwModConverter.Core.Resources.{fileName}")!;

        _ = stream.Read<int>();
        var count = stream.Read<int>();
        
        for (int i = 0; i < count; i++) {
            hashes[stream.Read<ulong>()] = stream.Read<ulong>();
        }
        
        return hashes.ToFrozenDictionary();
    }
}