using BotwModConverter.Core;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace BotwModConverter.UnitTests;

[TestClass]
public class DataCollection
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

            Span<byte> data = File.ReadAllBytes(file).AsSpan();

            if (data.Length < 4) {
                continue;
            }

            data = Utils.Decompress(data, out bool isYaz0);
            string magic = Encoding.UTF8.GetString(data[0..4]);

            if (isYaz0) {
                ext = ext.Remove(1, 1);
            }

            if (!entries.ContainsKey(magic)) {
                entries.Add(magic, new());
            }

            entries[magic].Add(new(path, ext, isYaz0));

            if (magic == "SARC") {
                SarcFile sarc = new(data.ToArray()); // really sarc lib?
                foreach ((var name, var fileData) in sarc.Files) {
                    ext = Path.GetExtension(name);
                    string subPath = $"{path}//{name}";

                    Span<byte> _instanceData = fileData.AsSpan();
                    _instanceData = Utils.Decompress(_instanceData, out isYaz0);
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
        static string Align(string insert, int max)
        {
            decimal factor = (decimal)(max - insert.Length) / 2;
            int padding = (int)Math.Floor(factor);
            return insert.PadLeft(insert.Length + padding, ' ').PadRight(max, ' ');
        }

        static string ToLiteral(string valueTextForCompiler)
        {
            return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(valueTextForCompiler, false);
        }

        string[] paths = new string[] {
            "../../../test-data/Dlc.json",
            "../../../test-data/Update.json",
            "../../../test-data/BaseGame.json",
        };

        string[] skip = new string[] {
            ".extm", ".mate", ".hght",
            ".fmc", ".mp4", ".bin",
            ".txt", ".bflim", ".beco",
            ".fxparam"
        };

        Dictionary<string, List<FileInfo>> merged = new();

        foreach (var path in paths) {
            Dictionary<string, List<FileInfo>> entries = (Dictionary<string, List<FileInfo>>)JsonSerializer.Deserialize(File.ReadAllText(path), typeof(Dictionary<string, List<FileInfo>>))!;

            foreach (var entry in entries) {
                if (entry.Value.Select(x => x.Ext).Where(x => skip.Contains(x)).Any()) {
                    continue;
                }

                string key = ToLiteral(entry.Key.Replace("BY\0\u0002", "BY").Replace("W��W", "W??W"));
                if (!merged.ContainsKey(key)) {
                    merged.Add(key, new());
                }

                merged[key] = merged[key].Concat(entry.Value.Where(x => !merged[key].Select(x => x.FilePath).Contains(x.FilePath))).ToList();
            }
        }

        int maxColMagic = merged.Max(x => x.Key.Length) + 2;
        int maxColExt = merged.Max(x => x.Value.Max(x => x.Ext.Length)) + 2;
        maxColExt = "Extension".Length > maxColExt ? "Extension".Length + 2 : maxColExt;
        int maxColYaz = "Yaz0 Compressed".Length + 2;
        int maxColDesc = 40;
        int maxColState = 12;
        int maxColInfo = 20;

        StringBuilder mdDoc = new("# Botw Data Types\n\n");

        StringBuilder table = new(
            $"|{Align("Magic", maxColMagic)}|{Align("Extension", maxColExt)}" +
            $"|{Align("Yaz0 Compressed", maxColYaz)}|{Align("Description", maxColDesc)}" +
            $"|{Align("IO State", maxColState)}|{Align("Conversion Info", maxColInfo)}|\n"
        );
        table.AppendLine(
            $"|:{new string('-', maxColMagic - 2)}:|:{new string('-', maxColExt - 2)}:" +
            $"|:{new string('-', maxColYaz - 2)}:|{new string('-', maxColDesc)}" +
            $"|:{new string('-', maxColState - 2)}:|{new string('-', maxColInfo)}|"
        );

        StringBuilder types = new();

        foreach (var entry in merged) {
            types.AppendLine($"## {entry.Key}\n");

            types.AppendLine($"### Extensions:\n");
            foreach (var item in entry.Value.DistinctBy(x => x.Ext).OrderBy(x => x.Ext)) {
                types.AppendLine($"- {item.Ext} | Yaz0: `{item.IsYaz0}`");
            }
            types.AppendLine();

            // Overkill
            // 
            types.AppendLine($"### File Paths:\n");
            int counter = 1;
            foreach (var item in entry.Value.OrderBy(x => x.FilePath)) {
                types.AppendLine($"- *\"{item.FilePath}\"*");

                counter++;
                if (counter > 3) {
                    break;
                }
            }
            types.AppendLine($"- `...`\n\n<br>\n");

            table.AppendLine(
                $"|{Align(entry.Key, maxColMagic)}|{Align(entry.Value.OrderBy(x => x.Ext).First().Ext, maxColExt)}" +
                $"|{Align(entry.Value.OrderBy(x => x.Ext).First().IsYaz0.ToString(), maxColYaz)}" +
                $"|{new string(' ', maxColDesc)}|{new string(' ', maxColState)}|{new string(' ', maxColInfo)}|"
            );
        }

        mdDoc.AppendLine(table.ToString());
        mdDoc.AppendLine(types.ToString());
        mdDoc.AppendLine("---\n");
        mdDoc.AppendLine($"#### Ignored File Extensions: \n- {string.Join("\n- ", skip.Order())}");

        File.WriteAllText("../../../test-data/BotwDataTypes.md", mdDoc.ToString());
    }

    [TestMethod]
    [DataRow("../../../test-data/wiiu", "../../../../BotwModConverter.Core/Data/wiiu.xxhash")]
    [DataRow("../../../test-data/switch", "../../../../BotwModConverter.Core/Data/switch.xxhash")]
    public void Hash(string path, string output)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using FileStream fs = File.Create(output);
        using BinaryWriter writer = new(fs);

        static void ReadSarc(ref uint count, BinaryWriter writer, Span<byte> sarcData)
        {
            SarcFile sarc = new(sarcData.ToArray()); // really sarc lib?
            foreach ((var name, var fileData) in sarc.Files) {
                Span<byte> data = fileData.AsSpan();
                data = Utils.Decompress(data, out bool isYaz0);
                if (data.Length > 4 && Enumerable.SequenceEqual(fileData[0..4], "SARC"u8.ToArray())) {
                    ReadSarc(ref count, writer, fileData);
                }

                writer.Write(Standart.Hash.xxHash.xxHash3.ComputeHash(fileData, fileData.Length));
                count++;
            }
        }

        // Reserve count offset
        writer.Write(0U);
        uint count = 0;

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {
            Span<byte> data = File.ReadAllBytes(file).AsSpan();
            data = Utils.Decompress(data, out bool isYaz0);
            if (data.Length > 4 && data[0..4].SequenceEqual("SARC"u8)) {
                ReadSarc(ref count, writer, data);
            }

            writer.Write(Standart.Hash.xxHash.xxHash3.ComputeHash(data, data.Length));
            count++;
        }

        writer.BaseStream.Position = 0;
        writer.Write(count);
        writer.Flush();
        writer.Dispose();

        File.WriteAllBytes(output, Yaz0.CompressFast(output));
    }

    [TestMethod]
    [DataRow("../../../../BotwModConverter.Core/Data/wiiu.xxhash")]
    [DataRow("../../../../BotwModConverter.Core/Data/switch.xxhash")]
    public void BenchmarkHashSet(string path)
    {
        var watch = Stopwatch.StartNew();
        byte[] data = Yaz0.Decompress(path);
        watch.Stop();
        Console.WriteLine($"Decompressed in: {watch.ElapsedMilliseconds}ms");

        watch.Restart();

        using MemoryStream fs = new(data);
        using BinaryReader reader = new(fs);
        Span<ulong> hashes = new ulong[(int)(fs.Length / 8)];

        uint count = reader.ReadUInt32();

        for (int i = 0; i < count; i++) {
            hashes[i] = reader.ReadUInt64();
        }

        watch.Stop();
        Console.WriteLine($"Loaded hashes: {watch.ElapsedMilliseconds}ms");
    }
}
