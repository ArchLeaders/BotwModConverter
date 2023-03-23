using Standart.Hash.xxHash;
using System.Buffers.Binary;
using System.Reflection;

namespace BotwModConverter.Core.Helpers;

public static class GameROM
{
    private const int NxCount = 128501;
    private const int WiiuCount = 126318;

    private static readonly Dictionary<BotwPlatform, HashSet<ulong>?> _hashes = new() {
        { BotwPlatform.Switch, null }, { BotwPlatform.Wiiu, null }
    };

    private static HashSet<ulong> GetHashes(BotwPlatform platform)
    {
        if (_hashes[platform] == null) {
            string file = $"{nameof(Core)}.Data.{platform}.xxhash";
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file)!;
            _hashes[platform] = new((int)stream.Length / 8);

            Span<byte> buffer = stackalloc byte[8];
            for (int i = 0; i < stream.Length / 8; i++) {
                stream.Read(buffer);
                _hashes[platform]!.Add(BinaryPrimitives.ReadUInt64LittleEndian(buffer));
            }
        }

        return _hashes[platform]!;
    }

    public static bool IsVanilla(ReadOnlySpan<byte> data, BotwPlatform platform)
    {
        return GetHashes(platform).Contains(xxHash3.ComputeHash(data, data.Length));
    }
}
