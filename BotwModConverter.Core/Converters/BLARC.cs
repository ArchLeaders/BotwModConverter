using Nintendo.Sarc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwModConverter.Core.Converters
{
    public class BLARC
    {
        public static async Task<byte[]> Convert(byte[] data, string file)
        {
            SarcFile blarc = new(Utils.UnYaz(data, out bool isYaz0));

            // get a Dictionary<string, byte[]> of bflim files
            Dictionary<string, byte[]> bflimFiles = (Dictionary<string, byte[]>)blarc.Files.Where(x => x.Key.Contains("blfim"));

            // if len = 0 return
            if (bflimFiles.Count == 0) {
                return data;
            }

            // get a stock sblarc to pull a bntx
            SarcFile blarc_nx = Path.GetFileName(file.Split("//")[0]) == "Bootup.pack"
                ? new(Bcml.Utils.GetGameFile("Pack\\Bootup.pack\\\\Layout\\Common.sblarc"))
                : new(Bcml.Utils.GetGameFile("Pack\\Title.pack\\\\Layout\\Title.sblarc"));
            byte[] bntxFile = blarc_nx.Files["timg/__Combined.bntx"];

            // inject the wiiu bflim files into the switch bntx
            foreach ((var bflim, var bytes) in bflimFiles) {

                // somehow inject the bflim 😅
                // BNTX.InjectBflim(bntxFile, bytes);

                blarc.Files.Remove(bflim);
            }

            // add the edited bntx
            blarc.Files.Add("timg/__Combined.bntx", bntxFile);

            // return the compiled sarc (blarc)


            return isYaz0 ? blarc.ToBinary().Yaz() : blarc.ToBinary();
        }
    }
}
