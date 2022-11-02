using Nintendo.Sarc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwModConverter.Core.Converters
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
        }
    }
}
