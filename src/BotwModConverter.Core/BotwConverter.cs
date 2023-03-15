using System.Runtime.CompilerServices;
using Yaz0Library;

namespace BotwModConverter.Core;

public enum ThreadMode { Single, Parallel }

public class BotwConverter
{
    private readonly BotwMod _mod;
    private readonly bool _parallel;
    private int _counter = 0;

    private BotwConverter(BotwMod mod, ThreadMode mode)
    {
        _mod = mod;
        _parallel = mode == ThreadMode.Parallel;
    }

    public static async Task<int> ConvertMod(BotwMod mod, string outputRoot, ThreadMode threadMode = ThreadMode.Parallel)
    {
        BotwConverter converter = new(mod, threadMode);
        await converter.ConvertRoot(outputRoot);
        return converter._counter;
    }

    public async Task ConvertRoot(string outputRoot)
    {
        IEnumerable<string?> modFolders = _mod.GetModFolders();
        if (_parallel) {
            await Parallel.ForEachAsync(modFolders, async (folderName, _) => {
                await Process(folderName!);
            });
        }
        else {
            foreach (var folderName in modFolders) {
                await Process(folderName!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task Process(string folderName)
        {
            string folder = Path.Combine(_mod.Root, folderName);
            string output = Path.Combine(outputRoot, folderName);
            await ConvertFolder(folder, output);
        }
    }

    private async Task ConvertFolder(string path, string outputRoot)
    {
        await ConvertFiles(path, outputRoot);
        IEnumerable<string> dirs = Directory.EnumerateDirectories(path);
        if (_parallel) {
            await Parallel.ForEachAsync(dirs, async (folder, cancellationToken) => {
                await Process(folder);
            });
        }
        else {
            foreach (var folder in dirs) {
                await Process(folder);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task Process(string folder)
        {
            string output = Path.Combine(outputRoot, Path.GetFileName(folder));
            await ConvertFolder(folder, output);
        }
    }

    private async Task ConvertFiles(string path, string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        IEnumerable<string> files = Directory.EnumerateFiles(path);
        if (_parallel) {
            await Parallel.ForEachAsync(files, async (file, cancellationToken) => {
                await Process(file);
            });
        }
        else {
            foreach (var file in files) {
                await Process(file);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task Process(string file)
        {
            string output = Path.Combine(outputRoot, Path.GetFileName(file));
            await ConvertFile(file, output);
            _counter++;
        }
    }

    internal static Task ConvertFile(string file, string output)
    {
        using FileStream src = File.OpenRead(file);
        Span<byte> data = src.Length < 0x100000 ? stackalloc byte[(int)src.Length] : new byte[src.Length];
        src.Read(data);

        ReadOnlySpan<byte> converted = ConvertData(data, file, out Yaz0SafeHandle? handle);

        // Some converters (namely BFRES) return a NULL
        // value to indicate that the file should not be written
        if (converted != null) {
            using FileStream fs = File.Create(output, data.Length);
            fs.Write(converted);
            handle?.Dispose();
        }

        // This should write to a custom logger instead
        Console.WriteLine($"{file} >> {output} : {data.Length}");
        return Task.CompletedTask;
    }

    internal static Span<byte> ConvertData(Span<byte> data, string path, out Yaz0SafeHandle? handle)
    {
        ReadOnlySpan<byte> raw = Utils.Decompress(data, out bool isYaz0);
        IDataConverter converter = Utils.GetConverter(path, isYaz0);

        Span<byte> converted = converter.ConvertToWiiu(raw);

        if (isYaz0) {
            return Yaz0.Compress(converted, out handle);
        }
        else {
            handle = null;
            return converted;
        }
    }
}
