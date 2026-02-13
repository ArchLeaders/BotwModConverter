using System.Runtime.CompilerServices;
using BotwModConverter.Core.Common;
using BotwModConverter.Core.Utils;
using CommunityToolkit.HighPerformance;
using CsYaz0;

namespace BotwModConverter.Core;

public ref struct ModFile
{
    public readonly string? AbsolutePath;
    public readonly string? AbsoluteOutputPath;
    public readonly string? OutputFolder;
    public readonly string OutputFileName;
    public readonly ReadOnlySpan<char> Canon;
    public bool IsCompressed;

    private ModFile(string? absolutePath, string? absoluteOutputPath, string? outputFolder, string outputFileName, ReadOnlySpan<char> canon)
    {
        AbsolutePath = absolutePath;
        AbsoluteOutputPath = absoluteOutputPath;
        OutputFolder = outputFolder;
        OutputFileName = outputFileName;
        Canon = canon;
    }

    public static ModFile FromFile(string filePath, string outputFilePath)
    {
        return new ModFile(
            filePath,
            outputFilePath,
            Path.GetDirectoryName(outputFilePath),
            Path.GetFileName(outputFilePath),
            filePath.ToCanon()
        );
    }

    public RentedBuffer Rent(int? size = null)
    {
        if (AbsolutePath is null) {
            throw new NotSupportedException("Cannot rent virtual mod files.");
        }
        
        var raw = RentedBuffer.Rent(AbsolutePath, size);
        var data = raw.Span;

        if (Unsafe.As<byte, int>(ref data[0]) != Yaz0.MAGIC) {
            return raw;
        }
        
        IsCompressed = true;

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
        if (AbsolutePath is null) {
            throw new NotSupportedException("Cannot stream virtual mod files.");
        }
        
        var fs = File.OpenRead(AbsolutePath);

        if (fs.Read<int>() != Yaz0.MAGIC) {
            fs.Seek(0, SeekOrigin.Begin);
            return fs;
        }
        
        IsCompressed = true;

        fs.Seek(0, SeekOrigin.Begin);
        using var buffer = RentedBuffer.Rent(fs);
        var data = buffer.Span;
        
        var decompressed = new byte[Yaz0.GetDecompressedSize(data)];
        Yaz0.Decompress(data, decompressed);

        return new MemoryStream(decompressed, writable: false);
    }
    
    public void Write(ReadOnlySpan<byte> data)
    {
        if (AbsoluteOutputPath is null) {
            throw new NotSupportedException("Cannot write virtual mod files.");
        }
        
        if (Path.GetDirectoryName(AbsoluteOutputPath) is { } folderPath) {
            Directory.CreateDirectory(folderPath);
        }

        if (IsCompressed) {
            using var compressed = Yaz0.Compress(data);
            File.WriteAllBytes(AbsoluteOutputPath, compressed);
        }

        File.WriteAllBytes(AbsoluteOutputPath, data);
    }

    public Stream OpenWrite(out Action? compress)
    {
        if (AbsoluteOutputPath is null) {
            throw new NotSupportedException("Cannot write virtual mod files.");
        }

        return ConverterEngine.OpenWrite(AbsoluteOutputPath, IsCompressed, out compress);
    }

    public void WriteCopy()
    {
        if (AbsoluteOutputPath is null) {
            throw new NotSupportedException("Cannot copy virtual mod files.");
        }
        
        string outputFilePath = OutputFolder is { } folder ? Path.Combine(folder, OutputFileName) : OutputFileName;
        File.Copy(AbsoluteOutputPath, outputFilePath);
    }
}