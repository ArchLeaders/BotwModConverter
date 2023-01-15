﻿using BotwModConverter.Core.Helpers;
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

        internal static Task ConvertFolder(string path)
        {
            return Parallel.ForEachAsync(Directory.EnumerateFiles(path), (file, cancellationToken) => {
                ConvertFile(file);
                return new();
            });
        }

        internal static async Task ConvertFolderRecursively(string path)
        {
            await ConvertFolder(path);
            await Parallel.ForEachAsync(Directory.EnumerateDirectories(path), async (folder, cancellationToken) => {
                await ConvertFolder(folder);
            });
        }

        internal static void ConvertFile(string file)
        {
            Span<byte> data = File.ReadAllBytes(file).AsSpan();
            string ext = Path.GetExtension(file);
            using FileStream fs = File.Create(file);
            fs.Write(ConvertData(data, ext, out Yaz0SafeHandle? _));
        }

        internal static Span<byte> ConvertData(Span<byte> data, string ext, out Yaz0SafeHandle? handle)
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
