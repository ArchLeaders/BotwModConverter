using Nintendo.Yaz0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwModConverter.Core
{
    public static class Utils
    {
        public static Dictionary<string[], IBotwConverter> Converters { get; } = new() {
            { new string[] {
                ".bactcapt", ".baglblm", ".baglccr", ".baglclwd", ".baglcube", ".bagldof",
                ".baglenv", ".baglenvset", ".baglfila", ".bagllmap", ".bagllref", ".baglmf", ".baglshpp",
                ".baiprog", ".bas", ".baslist", ".bassetting", ".batcl", ".batcllist", ".bawareness", ".bawntable",
                ".bbonectrl", ".bchemical", ".bchmres", ".bdemo", ".bdgnenv", ".bdmgparam", ".bdrop",
                ".bgapkginfo", ".bgapkglist", ".bgenv", ".bglght", ".bgmsconf", ".bgparamlist", ".bgsdw",
                ".bksky", ".blifecondition", ".blod", ".bmodellist", ".bmscdef", ".bmscinfo", ".bnetfp",
                ".bphyscharcon", ".bphyscontact", ".bphysics", ".bphyslayer", ".bphysmaterial", ".bphyssb", ".bphyssubmat",
                ".bptclconf", ".brecipe", ".brgbw", ".brgcon", ".brgconfig", ".brgconfiglist", ".bsfbt",
                ".bsft", ".bshop", ".bumii", ".bvege", ".bxml",
            }, new Converters.Aamp() },
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

        public static bool IsYaz0Compressed(ref byte[] data)
        {
            if (Enumerable.SequenceEqual(data[0..4], "Yaz0"u8.ToArray())) {
                data = Yaz0.DecompressFast(data);
                return true;
            }
            else {
                return false;
            }
        }

        public static byte[] UnYaz(this byte[] raw) => UnYazReport(raw).Value;
        public static byte[] UnYaz(this byte[] raw, out bool wasDecompressed)
        {
            var yazInfo = UnYazReport(raw);
            wasDecompressed = yazInfo.Key;
            return yazInfo.Value;
        }

        public static KeyValuePair<bool, byte[]> UnYazReport(this byte[] raw)
        {
            if (Encoding.UTF8.GetString(raw[0..3]) == "Yaz0") {
                try {
                    return new(true, Yaz0.DecompressFast(raw));
                }
                catch {
                    return new(true, Yaz0.Decompress(raw));
                }
            }
            else {
                return new(false, raw);
            }
        }

        public static byte[] Yaz(this byte[] raw)
        {
            try {
                return Yaz0.DecompressFast(raw);
            }
            catch {
                return Yaz0.Decompress(raw);
            }
        }

        public static bool IsFileModded(string name, byte[] bytes, bool allowNew = true)
        {
            //var table = Bcml.Utils.GetHashTable();

            //if (!table.ContainsKey(name)) {
            //    return allowNew;
            //}

            //return !table[name].Contains(xxHash64.ComputeHash(UnYaz(bytes)));

            throw new NotImplementedException();
        }

        public static byte[] GetStockBfstp(string name, string barsFile)
        {
            throw new NotImplementedException();
        }
    }
}
