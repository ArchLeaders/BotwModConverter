using Standart.Hash.xxHash;
using System.Buffers.Binary;
using System.Reflection;
using Yaz0Library;

namespace BotwModConverter.Core;

public enum BotwPlatform { Switch, Wiiu }

public static class Utils
{
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

    public static ReadOnlySpan<byte> Decompress(Span<byte> data, out bool isYaz0)
    {
        if (data.Length > 4 && data[0..4].SequenceEqual("Yaz0"u8)) {
            isYaz0 = true;
            return Yaz0.Decompress(data);
        }

        isYaz0 = false;
        return data;
    }

    public static bool IsModded(ReadOnlySpan<byte> data, BotwPlatform platform)
    {
        return !GetHashes(platform).Contains(xxHash3.ComputeHash(data, data.Length));
    }

    public static IDataConverter GetConverter(string path, bool isYaz0)
    {
        string ext = Path.GetExtension(path).Remove(0, isYaz0 ? 2 : 1);
        return ext switch {
            "bars" => Converters.Bars.Shared,

            "bcamanim" or "bfres" or "bitemico" or
            "bmapopen" or "bmaptex" or "breviewtex" or
            "bstftex" => Converters.Bfres.Shared,

            // Binary Ecosystem ("beco")
            // Binary Loop Asset List ("blal")
            // BFEV, BFSTM, Sound, etc ("bfstm" or "bsftp")

            "baischedule" or "baniminfo" or "bgdata" or
            "bgsvdata" or "bquestpack" or "byml" or
            "mubin" => Converters.Byml.Shared,

            // AnimationDrivenSpeed/AnimalUnitSpeed (".bin") ???
            // Emitter Set List (".esetlist")
            // Grass Colour Layout ("grass.extm")

            "hkcl" or "hknm2" or "hkrb" or
            "hkrg" or "hksc" or
            "hktm" => Converters.Havok.Shared,

            // MATE (".mate") ???
            // Message Studio Binary Text (".msbt")
            // ResourceSizeTable (".rstb")

            "bactorpack" or "beventpack" or "bgenv" or
            "blarc" or "bmodelsh" or "genvb" or
            "pack" or "sarc" or "stats" or
            "stera" => Converters.Sarc.Shared,

            // Terrain Scene Binary (".tscb")
            // Water Layout ("water.extm")

            _ => throw new NotSupportedException($"Could not find a converter for the file '{path}'"),
        };
    }
}
