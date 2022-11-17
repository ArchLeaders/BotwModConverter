using BotwModConverter.Core.Helpers;
using Microsoft.VisualBasic.FileIO;
using Nintendo.Yaz0;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwModConverter.Core
{
    public enum BotwPlatform { Switch, Wiiu }

    public class BotwConverter
    {
        public static Task Convert(string mod, bool createBackup = true)
        {
            if (createBackup) {
                DirectoryHelper.Copy(mod, $"{mod}/../{Path.GetFileName(mod)}-Backups/{DateTime.Now:yyyy.M.m.s}/");
            }

            return Parallel.ForEachAsync(new string[] { "aoc", "content" }, async (folderName, _) => {
                string folder = $"{mod}/{folderName}";
                if (Directory.Exists(folder)) {
                    await ConvertFolder(folder);
                }
            });
        }

        public static Task ConvertFolder(string folder)
        {
            return Parallel.ForEachAsync(new IEnumerable<string>[] { Directory.EnumerateDirectories(folder), Directory.EnumerateFiles(folder) },
                async (paths, _) => await Parallel.ForEachAsync(paths, async (path, _) => {
                    if (Directory.Exists(path)) {
                        await ConvertFolder(path);
                    }
                    else if (File.Exists(path)) {
                        await ConvertFile(path);
                    }
                    else {
                        throw new Exception("Fatal Error: A file or folder was removed during the conversion process.");
                    }
                })
            );
        }

        public static async Task ConvertFile(string file)
        {
            byte[] data = File.ReadAllBytes(file);
            string ext = Path.GetExtension(file);
            await File.WriteAllBytesAsync(file, await ConvertData(data, ext));
        }

        public static async Task<byte[]> ConvertData(byte[] data, string ext)
        {
            foreach ((var keys, var converter) in Utils.Converters) {
                if (keys.Contains(ext)) {
                    bool isYaz0 = Utils.IsYaz0Compressed(ref data);
                    byte[] converted = await converter.ConvertToWiiu(data);
                    return isYaz0 ? Yaz0.CompressFast(converted) : converted;
                }
            }

            _ = Task.Run(() => Debug.WriteLine($"The file extension '{ext}' is not yet supported by the converter."));
            return data;
        }
    }
}
