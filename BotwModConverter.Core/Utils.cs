using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yaz0Library;

namespace BotwModConverter.Core
{
    public static class Utils
    {
        public static Dictionary<string[], IBotwConverter> Converters { get; } = new() {
            // Binary Archive Resource Stream
            { new string[] {
                ".bcamanim", ".bfres", ".bitemico", ".bmapopen", ".bmaptex", ".breviewtex", ".bstftex"
            }, new Converters.Bfres() },
            // Binary Ecosystem (".beco")
            // Binary Format Event Flow (".bfevfl", ".bfevtm")
            // Binary Loop Asset List (".blal")
            { new string[] {
                ".baischedule", ".baniminfo", ".bgdata", ".bgsvdata", ".bquestpack", ".byml", ".mubin"
            }, new Converters.Byml() },
            // AnimationDrivenSpeed/AnimalUnitSpeed (".bin") ???
            // Emitter Set List (".esetlist")
            // Grass Colour Layout ("grass.extm")
            { new string[] {
                "hkcl", "hknm2", "hkrb", "hkrg", "hksc", "hktm"
            }, new Converters.Havok() },
            // MATE (".mate") ???
            // Message Studio Binary Text (".msbt")
            // ResourceSizeTable (".rstb")
            { new string[] {
                ".bactorpack", ".beventpack", ".bgenv", ".blarc", ".bmodelsh", ".genvb", ".pack",
                ".sarc", ".stats", ".stera",
            }, new Converters.Sarc() },
            // Terrain Scene Binary (".tscb")
            // Water Layout ("water.extm")
        };

        public static Span<byte> DecompressYaz0(Span<byte> data, out bool isYaz0)
        {
            if (data.Length > 4 && data[0..4].SequenceEqual("Yaz0"u8)) {
                isYaz0 = true;
                return Yaz0.Decompress(data);
            }

            isYaz0 = false;
            return data;
        }
    }
}
