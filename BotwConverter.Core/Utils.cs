using Nintendo.Yaz0;
using Standart.Hash.xxHash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwConverter.Core
{
    public static class Utils
    {
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
            var table = Bcml.Utils.GetHashTable();

            if (!table.ContainsKey(name)) {
                return allowNew;
            }

            return !table[name].Contains(xxHash64.ComputeHash(UnYaz(bytes)));
        }

        public static byte[] GetStockBfstp(string name, string barsFile)
        {
            throw new NotImplementedException();
        }
    }
}
