using BotwModConverter.Core.Helpers;
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
                DirectoryHelper.Copy(mod, $"{mod}/../{Path.GetFileName(mod)}-{DateTime.Now:yyyy.M.m.s}-Backup");
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
            // Get convert from file ext
            // Convert
            // Write the new file

            Debug.WriteLine(Path.GetFileName(file));
            await Task.Delay(5000);
        }
    }
}
