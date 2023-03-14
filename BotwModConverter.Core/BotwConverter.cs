using Yaz0Library;

namespace BotwModConverter.Core;

public class BotwConverter
{
    public static Task Convert(string mod)
    {
        return Parallel.ForEachAsync(new string[] { "aoc", "content" }, async (folderName, _) => {
            string folder = Path.Combine(mod, folderName);
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

    internal static void ConvertFile(string file, string output)
    {
        using FileStream src = File.OpenRead(file);
        Span<byte> data = src.Length < 0x100000 ? stackalloc byte[(int)src.Length] : new byte[src.Length];
        src.Read(data);

        using FileStream fs = File.Create(output, data.Length);
        fs.Write(ConvertData(data, Path.GetExtension(file), out Yaz0SafeHandle? _));
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
