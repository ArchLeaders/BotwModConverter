using Nintendo.Sarc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwConverter.Core
{
    public class Converter
    {
        public string root { get; set; }

        public Converter(string mod)
        {
            root = Bcml.Install.OpenMod(mod);
        }

        public async Task Convert(string _mod)
        {
            string mod = Bcml.Install.OpenMod(_mod);
            var meta = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>($"{mod}/info.json")!;

            if (meta["platform"].GetString() == "switch") {
                throw new NotImplementedException("Convertion from Switch to WiiU is not yet supported.");
            }

            foreach (var file in Directory.EnumerateFiles(mod)) {
                string path = Path.GetRelativePath(mod, file);
                if (path.StartsWith("content") || path.StartsWith("aoc")) {
                    // add task when we understand :)
                    await TryConvertFile(file);
                }
            }
        }

        public async Task<byte[]?> TryConvertFile(string file, byte[]? data = null)
        {
            data ??= File.ReadAllBytes(file);
            string canon = Bcml.Utils.GetCanonicalPath(Path.GetRelativePath(root, file));

            if (data.Length > 0) {
                var ext = Path.GetExtension(file);
                if (Utils.IsFileModded(canon, data)) {
                    // add task when we understand :)
                    return await ConvertFile(file, data);
                }
                else if (Meta.UnSupported.Contains(ext) || ext == ".bcamanim") {
                    var bytes = Bcml.Utils.GetGameFile(file);

                    try {
                        // *visible confusion*
                        Debug.WriteLine($"Event/{Path.GetFileNameWithoutExtension(file.Split("//")[0]).Replace("Event_", "").Replace("_Open", "_0")}.sbeventpack//{file.Split("//")[1]}");
                        bytes ??= Bcml.Utils.GetGameFile($"Actor/Pack/{Path.GetFileNameWithoutExtension(file.Split("//")[0]).Replace("_A", "")}.sbactorpack//{file.Split("//")[1]}");
                    } catch { }

                    try {
                        // *visible confusion*
                        Debug.WriteLine($"Event/{Path.GetFileNameWithoutExtension(file.Split("//")[0]).Replace("Event_", "").Replace("_Open", "_0")}.sbeventpack//{file.Split("//")[1]}");
                        bytes ??= Bcml.Utils.GetGameFile($"Event/{Path.GetFileNameWithoutExtension(file.Split("//")[0]).Replace("Event_", "").Replace("_Open", "_0")}.sbeventpack//{file.Split("//")[1]}");
                    }
                    catch { }

                    bytes ??= await ConvertFile(file, data);
                    return bytes;
                }
            }

            return null;
        }

        public async Task<byte[]?> ConvertFile(string file, byte[] data)
        {
            string ext = Path.GetExtension(file).ToLower();

            if (Meta.BfresExt.Contains(ext) && ext != ".tex2") {
                return await Converters.BFRES.Convert(data);
            }
            else if (ext == ".bars") {
                return await Converters.BARS.Convert(data);
            }
            else if (ext == ".bfstm") {
                return await Converters.BFSTM.Convert(data);
            }
            else if (ext.Contains("pack") && ext != ".sbquestpack") {
                return await ConvertSarcFile(file, data);
            }
            else if (Meta.HavokExt.Contains(ext)) {
                return await Converters.Havok.Convert(data);
            }
            else if (Path.GetFileName(file) == "BootUp.sblarc") {
                File.Delete(file);
            }
            else if (ext == ".sblarc") {
                return await Converters.BLARC.Convert(data, file);
            }

            return null;
        }

        public async Task<byte[]> ConvertSarcFile(string file, byte[] data)
        {
            SarcFile sarc = new(Utils.UnYaz(data));
            List<string> remove = new();

            foreach ((var name, var bytes) in sarc.Files) {
                if (Meta.Supported.Contains(Path.GetExtension(name).ToLower())) {
                    var converted = await TryConvertFile($"{file}//{name}", bytes);
                    if (converted != null) {
                        sarc.Files[name] = converted;
                    }
                    else {
                        remove.Add(name);
                    }
                }
            }

            foreach (var name in remove) sarc.Files.Remove(name);
            return sarc.ToBinary();
        }
    }
}
