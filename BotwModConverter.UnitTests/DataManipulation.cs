using BotwModConverter.Core;
using BotwModConverter.Core.Helpers;
using Microsoft.VisualStudio.TestPlatform.Common.Interfaces;
using Nintendo.Sarc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotwModConverter.UnitTests
{
    [TestClass]
    public class DataManipulation
    {
        public record Info(string Header, bool IsYaz0);
        public record FileInfo(string FilePath, string Ext, bool IsYaz0);

        [TestMethod]
        [DataRow("D:\\Botw\\Cemu (Stable)\\mlc01\\usr\\title\\00050000\\101c9500\\content", "../../../test-data/Dlc.json")]
        [DataRow("D:\\Botw\\Cemu (Stable)\\mlc01\\usr\\title\\0005000e\\101c9500\\content", "../../../test-data/Update.json")]
        [DataRow("D:\\Botw\\Cemu (Stable)\\mlc01\\usr\\title\\0005000c\\101c9500\\content", "../../../test-data/BaseGame.json")]
        public void Collect(string gamePath, string outPath)
        {
            Dictionary<string, List<FileInfo>> entries = new();

            foreach (var file in Directory.EnumerateFiles(gamePath, "*.*", SearchOption.AllDirectories)) {

                string path = Path.GetRelativePath(gamePath, file).ToCommonPath();
                string ext = Path.GetExtension(file);

                byte[] data = File.ReadAllBytes(file);

                if (data.Length < 4) {
                    continue;
                }

                bool isYaz0 = Utils.IsYaz0Compressed(ref data);
                string magic = Encoding.UTF8.GetString(data[0..4]);

                if (isYaz0) {
                    ext = ext.Remove(1, 1);
                }

                if (!entries.ContainsKey(magic)) {
                    entries.Add(magic, new());
                }

                entries[magic].Add(new(path, ext, isYaz0));

                if (magic == "SARC") {
                    SarcFile sarc = new(data);
                    foreach ((var name, var fileData) in sarc.Files) {
                        ext = Path.GetExtension(name);
                        string subPath = $"{path}//{name}";

                        byte[] _instanceData = fileData;
                        isYaz0 = Utils.IsYaz0Compressed(ref _instanceData);
                        magic = Encoding.UTF8.GetString(_instanceData[0..4]);

                        if (isYaz0) {
                            ext = ext.Remove(1, 1);
                        }

                        if (!entries.ContainsKey(magic)) {
                            entries.Add(magic, new());
                        }

                        entries[magic].Add(new(subPath, ext, isYaz0));
                    }
                }
            }

            entries = entries.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            File.WriteAllText(outPath, JsonSerializer.Serialize(entries, new JsonSerializerOptions() {
                WriteIndented = true,
            }));
        }

        [TestMethod]
        public void Format()
        {
            string[] paths = new string[] {
                "../../../test-data/Dlc.json",
                "../../../test-data/Update.json",
                "../../../test-data/BaseGame.json",
            };

            Dictionary<string, Info> filtered = new();

            foreach (var path in paths) {
                List<Tuple<string, string, bool>> data = (List<Tuple<string, string, bool>>)JsonSerializer.Deserialize(File.ReadAllText(path), typeof(List<Tuple<string, string, bool>>))!;
                foreach ((var ext, var header, bool isYaz0) in data) {
                    if (!filtered.ContainsKey(ext)) {
                        filtered.Add(ext, new(header, isYaz0));
                    }
                }
            }

            filtered = filtered.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            File.WriteAllText("../../../test-data/BotwTypesFiltered.json", JsonSerializer.Serialize(filtered, new JsonSerializerOptions() {
                WriteIndented = true,
            }));
        }

        [TestMethod]
        public void BuildTable()
        {
            string[] paths = new string[] {
                "../../../test-data/Dlc.json",
                "../../../test-data/Update.json",
                "../../../test-data/BaseGame.json",
            };

            Dictionary<string, List<FileInfo>> merged = new();

            foreach (var path in paths) {
                Dictionary<string, List<FileInfo>> entries = (Dictionary<string, List<FileInfo>>)JsonSerializer.Deserialize(File.ReadAllText(path), typeof(Dictionary<string, List<FileInfo>>))!;

                foreach (var entry in entries) {
                    if (!merged.ContainsKey(entry.Key)) {
                        merged.Add(entry.Key, new());
                    }

                    merged[entry.Key] = merged[entry.Key].Concat(entry.Value.Where(x => merged[entry.Key].Select(x => x.FilePath).Contains(x.FilePath))).ToList();
                }
            }

            static string Align(string insert, int max)
            {
                int padding = (int)Math.Floor((double)(max - insert.Length) / 2);
                return insert.PadLeft(padding, ' ');
            }

            int maxColMagic = 8;
            int maxColExt = merged.Max(x => x.Value.Max(y => y.Ext))!.Length;
            maxColExt = "Extension".Length > maxColExt ? "Extension".Length : maxColExt;
            int maxColYaz = "Yaz0 Compressed".Length;
            int maxColDesc = 40;
            int maxColState = 12;
            int maxColInfo = 20;

            StringBuilder mdDoc = new("# Botw Data Types\n\n");

            StringBuilder table = new(
                $"|{Align("Magic", maxColMagic)}|{Align("Extensions", maxColExt)}" +
                $"|{Align("Yaz0 Compressed", maxColYaz)}|:{Align("Description", maxColDesc)}" +
                $"|{Align("IO State", maxColState)}|:{Align("Conversion Info", maxColInfo)}"
            );
            table.AppendLine(
                $"|:{new string('-', maxColMagic-2)}:|:{new string('-', maxColExt-2)}:" +
                $"|:{new string('-', maxColYaz-2)}:|{new string('-', maxColDesc)}" +
                $"|:{new string('-', maxColState-2)}:|{new string('-', maxColInfo)}"
            );

            StringBuilder types = new();

            foreach (var entry in merged) {
                types.AppendLine($"## {entry.Key}\n");

                types.AppendLine($"### Extensions:\n");
                foreach (var item in entry.Value.OrderBy(x => x.Ext)) {
                    types.AppendLine($"- {item.Ext} | Yaz0: `{item.IsYaz0}`");
                }
                types.AppendLine();

                types.AppendLine($"### File Paths:\n");
                foreach (var item in entry.Value.OrderBy(x => x.FilePath)) {
                    types.AppendLine($"- *\"{item.FilePath}\"*");
                }
                types.AppendLine();
                types.AppendLine($"\n<br>\n");

                table.AppendLine(
                    $"|{Align(entry.Key, maxColMagic)}|{Align(entry.Value.OrderBy(x => x.Ext).First().Ext, maxColExt)}" +
                    $"|{Align(entry.Value.OrderBy(x => x.Ext).First().IsYaz0.ToString(), maxColYaz)}" +
                    $"|{new string(' ', maxColDesc)}|{new string(' ', maxColState)}|{new string(' ', maxColInfo)}"
                );
            }

            mdDoc.AppendLine(table.ToString());
            mdDoc.AppendLine(types.ToString());

            File.WriteAllText("../../../test-data/BotwDataTypes.md", mdDoc.ToString());
        }
    }
}
