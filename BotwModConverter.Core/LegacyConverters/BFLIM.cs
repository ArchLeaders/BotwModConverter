using Nintendo.Sarc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotwModConverter.Core.LegacyConverters
{
    public class BFLIM
    {
        public static async Task<byte[]> Convert(byte[] data)
        {
            SarcFile blarc = new(Utils.UnYaz(data));

            foreach ((var file, var bytes)in blarc.Files) {
                if (file.Contains("bflim")) {

                }
            }

            return new byte[0];
        }
    }
}
