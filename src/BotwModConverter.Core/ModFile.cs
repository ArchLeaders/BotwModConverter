using System.Runtime.CompilerServices;
using BotwModConverter.Core.Common;
using BotwModConverter.Core.Utils;
using CommunityToolkit.HighPerformance;
using CsYaz0;

namespace BotwModConverter.Core;

public ref struct ModFile : IModFile
{
    public readonly string AbsolutePath;
    public readonly string AbsoluteOutputPath;
    private bool _isCompressed;
    
    public ReadOnlySpan<char> Canon { get; }
    
    public string RelativeFilePath { get; }

    private ModFile(string absolutePath, string relativeFilePath, string absoluteOutputPath, ReadOnlySpan<char> canon)
    {
        AbsolutePath = absolutePath;
        RelativeFilePath = relativeFilePath;
        AbsoluteOutputPath = absoluteOutputPath;
        Canon = canon;
    }

    public static ModFile FromFile(string filePath, string relativeFilePath, string outputFilePath)
    {
        return new ModFile(
            filePath,
            relativeFilePath,
            outputFilePath,
            filePath.ToCanon()
        );
    }

    public static void Peek<T>(T file, in Span<byte> view) where T : IModFile, allows ref struct
    {
        var modFile = Unsafe.As<T, ModFile>(ref file);
        using var fs = File.OpenRead(modFile.AbsolutePath);
        fs.ReadExactly(view);
    }

    public RentedBuffer Rent()
    {
        var raw = RentedBuffer.Rent(AbsolutePath);
        var data = raw.Span;

        if (Unsafe.As<byte, int>(ref data[0]) != Yaz0.MAGIC) {
            return raw;
        }
        
        _isCompressed = true;

        try {
            var decompressed = RentedBuffer.Rent(Yaz0.GetDecompressedSize(data));
            Yaz0.Decompress(data, decompressed.Span);

            return decompressed;
        }
        finally {
            raw.Dispose();
        }
    }

    public Stream Stream()
    {
        var fs = File.OpenRead(AbsolutePath);

        if (fs.Read<int>() != Yaz0.MAGIC) {
            fs.Seek(0, SeekOrigin.Begin);
            return fs;
        }
        
        _isCompressed = true;

        fs.Seek(0, SeekOrigin.Begin);
        using var buffer = RentedBuffer.Rent(fs);
        var data = buffer.Span;
        
        var decompressed = new byte[Yaz0.GetDecompressedSize(data)];
        Yaz0.Decompress(data, decompressed);

        return new MemoryStream(decompressed, writable: false);
    }
    
    public void Write(ArraySegment<byte> data)
    {
        if (Path.GetDirectoryName(AbsoluteOutputPath) is { } folderPath) {
            Directory.CreateDirectory(folderPath);
        }

        if (_isCompressed) {
            using var compressed = Yaz0.Compress(data);
            File.WriteAllBytes(AbsoluteOutputPath, compressed);
        }

        File.WriteAllBytes(AbsoluteOutputPath, data);
    }

    public Stream OpenWrite<T>(out Action<T>? compress) where T : IModFile, allows ref struct
    {
        var result = ConverterEngine.OpenWrite(AbsoluteOutputPath, _isCompressed, out var compressSimple);
        compress = compressSimple is not null ? _ => compressSimple() : null;
        
        return result;
    }

    public void WriteCopy()
    {
        File.Copy(AbsolutePath, AbsoluteOutputPath);
    }
}