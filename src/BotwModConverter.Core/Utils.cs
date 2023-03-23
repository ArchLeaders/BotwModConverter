using BotwModConverter.Core.Converters;
using Cead;
using Standart.Hash.xxHash;
using System.Buffers.Binary;
using System.Reflection;

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

    public static Span<byte> Decompress(Span<byte> data, out bool isYaz0)
    {
        isYaz0 = data.Length > 4 && data[0..4].SequenceEqual("Yaz0"u8);
        return isYaz0 ? Yaz0.Decompress(data) : data;
    }

    public static bool IsModded(ReadOnlySpan<byte> data, BotwPlatform platform)
    {
        return !GetHashes(platform).Contains(xxHash3.ComputeHash(data, data.Length));
    }

    public static Converter GetConverter(string path, bool isYaz0)
    {
        // Custom converter for the actorinfo
        if (Path.GetFileName(path) == "ActorInfo.product.sbyml") {
            return Converter.Init<ActorInfoConverter>(path);
        }

        string ext = Path.GetExtension(path).Remove(0, isYaz0 ? 2 : 1);
        return ext switch {
            "bars" => Converter.Init<BarsConverter>(path),

            "bcamanim" or "bfres" or "bitemico" or
            "bmapopen" or "bmaptex" or "breviewtex" or
            "bstftex" => Converter.Init<BfresConverter>(path),

            // Binary Ecosystem ("beco")
            // Binary Loop Asset List ("blal")
            // BFEV, BFSTM, Sound, etc ("bfstm" or "bsftp")

            "baischedule" or "baniminfo" or "bgdata" or
            "bgsvdata" or "bquestpack" or "byml" or
            "mubin" => Converter.Init<BymlConverter>(path),

            // AnimationDrivenSpeed/AnimalUnitSpeed (".bin") ???
            // Emitter Set List (".esetlist")
            // Grass Colour Layout ("grass.extm")

            "hkcl" or "hknm2" or "hkrb" or
            "hkrg" or "hksc" or
            "hktm" => Converter.Init<HavokConverter>(path),

            // MATE (".mate") ???
            "msbt" => Converter.Init<MsbtConverter>(path),
            // ResourceSizeTable (".rstb")

            "bactorpack" or "beventpack" or "bgenv" or
            "blarc" or "bmodelsh" or "genvb" or
            "sarc" or "stats" or "stera" or
            "pack" => Converter.Init<SarcConverter>(path),

            // Terrain Scene Binary (".tscb")
            // Water Layout ("water.extm")

            _ => throw new NotSupportedException($"Could not find a converter for the file '{path}'"),
        };
    }
}
