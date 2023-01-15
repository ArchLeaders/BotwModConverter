﻿using BotwModConverter.Core.Helpers;
using Syroot.BinaryData;
using System.Diagnostics;
using Yaz0Library;

namespace BotwModConverter.Core
{
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
            // this is stupid xD
            return Parallel.ForEachAsync(new IEnumerable<string>[] { Directory.EnumerateDirectories(folder), Directory.EnumerateFiles(folder) },
                async (paths, _) => await Parallel.ForEachAsync(paths, async (path, cancellationToken) => {
                    if (Directory.Exists(path)) {
                        await ConvertFolder(path);
                    }
                    else if (File.Exists(path)) {
                        ConvertFile(path);
                    }
                    else {
                        throw new Exception("Fatal Error: A file or folder was removed during the conversion process.");
                    }
                })
            );
        }

        public static void ConvertFile(string file)
        {
            // unavoidable allocation
            Span<byte> data = File.ReadAllBytes(file).AsSpan();
            string ext = Path.GetExtension(file);
            using FileStream fs = File.Create(file);
            fs.Write(ConvertData(data, ext, out Yaz0SafeHandle? _));
        }

        public static Span<byte> ConvertData(Span<byte> data, string ext, out Yaz0SafeHandle? handle)
        {
            Span<byte> decompressed = Utils.DecompressYaz0(data, out bool isYaz0);
            Span<byte> converted = Utils.GetConverter(ext).ConvertToWiiu(decompressed);

            if (isYaz0) {
                return Yaz0.Compress(converted, out handle);
            }
            else {
                handle = null;
                return converted;
            }
        }
    }
}
