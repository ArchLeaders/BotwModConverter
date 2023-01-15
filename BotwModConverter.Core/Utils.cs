using Standart.Hash.xxHash;
using System.Buffers.Binary;
using System.Reflection;
using Yaz0Library;

namespace BotwModConverter.Core
{
    public enum BotwPlatform { Switch, Wiiu }

    public static class Utils
    {
        private static BotwPlatform _platform;
        private static HashSet<ulong>? _hashes;

        private static HashSet<ulong> GetHashes(BotwPlatform platform)
        {
            if (_hashes == null || platform != _platform) {
                string file = $"{nameof(Core)}.Data.{platform}.xxhash";
                using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file)!;
                _hashes = new((int)stream.Length / 8);

                Span<byte> buffer = stackalloc byte[8];
                for (int i = 0; i < stream.Length / 8; i++) {
                    stream.Read(buffer);
                    _hashes.Add(BinaryPrimitives.ReadUInt64LittleEndian(buffer));
                }
            }

            _platform = platform;
            return _hashes;
        }

        public static Span<byte> DecompressYaz0(Span<byte> data, out bool isYaz0)
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

        public static IBotwConverter GetConverter(string ext)
        {
            return ext switch {

                // Binary Archive Resource Stream

                ".bcamanim" or ".bfres" or ".bitemico" or
                ".bmapopen" or ".bmaptex" or ".breviewtex" or
                ".bstftex" => Converters.Bfres.Shared,

                // Binary Ecosystem (".beco")
                // Binary Format Event Flow (".bfevfl", ".bfevtm")
                // Binary Loop Asset List (".blal")
                
                ".baischedule" or ".baniminfo" or ".bgdata" or
                ".bgsvdata" or ".bquestpack" or ".byml" or
                ".mubin" => Converters.Byml.Shared,

                // AnimationDrivenSpeed/AnimalUnitSpeed (".bin") ???
                // Emitter Set List (".esetlist")
                // Grass Colour Layout ("grass.extm")

                "hkcl" or "hknm2" or "hkrb" or
                "hkrg" or "hksc" or
                "hktm" => Converters.Havok.Shared,

                // MATE (".mate") ???
                // Message Studio Binary Text (".msbt")
                // ResourceSizeTable (".rstb")

                ".bactorpack" or ".beventpack" or ".bgenv" or
                ".blarc" or ".bmodelsh" or ".genvb" or
                ".pack" or ".sarc" or ".stats" or
                ".stera" => Converters.Sarc.Shared,

                // Terrain Scene Binary (".tscb")
                // Water Layout ("water.extm")

                _ => throw new NotSupportedException(),
            };
        }
    }
}
