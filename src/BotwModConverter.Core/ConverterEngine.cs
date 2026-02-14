using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BotwModConverter.Core.Common;
using BotwModConverter.Core.Utils;
using CsYaz0;

namespace BotwModConverter.Core;

public enum Platform
{
    Switch,
    WiiU
}

public enum ModOutputFolder
{
    Content,
    Aoc
}

public sealed class ConverterEngine : IRentHolder
{
    private readonly ConcurrentBag<RentedBuffer> _rentedBuffers = [];
    private readonly Dictionary<string, Stream> _waitingToConvert = new();
    private readonly string _sourceFolder;

    public readonly Platform Platform;
    public readonly string OutputFolder;

    public ConverterEngine(string sourceFolderPath, string? output = null)
    {
        Platform = ModFolderUtils.IsSwitchMod(sourceFolderPath) ? Platform.Switch : Platform.WiiU;
        _sourceFolder = sourceFolderPath;
        OutputFolder = output ?? $"{sourceFolderPath}-{Platform}";
    }

    public bool Convert()
    {
        foreach (var file in ModFolderUtils.EnumerateFiles(_sourceFolder, OutputFolder, Platform)) {
            Convert(file.Input, file.RelativePath, file.Output);
        }

        return true;
    }

    public bool ConvertParallel()
    {
        var res = Parallel.ForEach(
            ModFolderUtils.EnumerateFiles(_sourceFolder, OutputFolder, Platform),
            (file, _) => Convert(file.Input, file.RelativePath, file.Output)
        );

        if (res.IsCompleted) {
            return true;
        }

        // TODO: Log failure
        return false;
    }

    public ConvertResult Convert(string filePath, string relativeFilePath, string outputFolderPath)
    {
        var file = ModFile.FromFile(filePath, relativeFilePath, outputFolderPath);
        return Convert(ref file);
    }

    public ConvertResult Convert<T>(ref T file) where T : IModFile, allows ref struct 
    {
        if (ConverterLookup.Find(file.Canon) is { } converter) {
            return Platform switch {
                Platform.Switch => converter.ToWiiu(this, ref file),
                Platform.WiiU => converter.ToSwitch(this, ref file),
                _ => throw new NotSupportedException($"Unsupported platform: {Platform}")
            };
        }

        Span<byte> headerBuffer = stackalloc byte[0x20];
        T.Peek(file, headerBuffer);
        
        if (ConverterLookup.Find(headerBuffer) is { } converterFromData) {
            return Platform switch {
                Platform.Switch => converterFromData.ToWiiu(this, ref file),
                Platform.WiiU => converterFromData.ToSwitch(this, ref file),
                _ => throw new NotSupportedException($"Unsupported platform: {Platform}")
            };
        }

        file.WriteCopy();
        return ConvertResult.Copied;
    }

    public Stream OpenWrite(string relativeFilePath, bool isCompressed, out Action? compress, ModOutputFolder outputFolder)
    {
        var absoluteFilePath = Path.Combine(
            ModFolderUtils.GetPlatformOutput(OutputFolder, Platform, outputFolder), relativeFilePath);

        return OpenWrite(absoluteFilePath, isCompressed, out compress);
    }

    public static Stream OpenWrite(string absoluteFilePath, bool isCompressed, out Action? compress)
    {
        if (Path.GetDirectoryName(absoluteFilePath) is { } folderPath) {
            Directory.CreateDirectory(folderPath);
        }
        
        if (!isCompressed) {
            compress = null;
            return File.OpenWrite(absoluteFilePath);
        }

        var ms = new MemoryStream();

        compress = () => {
            if (!ms.TryGetBuffer(out var buffer)) {
                buffer = ms.ToArray();
            }

            using var compressed = Yaz0.Compress(buffer);
            File.WriteAllBytes(absoluteFilePath, compressed);
        };

        return ms;
    }

    public bool HasDependency(ReadOnlySpan<char> canon, [MaybeNullWhen(false)] out Stream data)
    {
        return _waitingToConvert.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(canon, out data);
    }

    public void StoreDependency(ReadOnlySpan<char> canon, Stream data)
    {
        if (!_waitingToConvert.GetAlternateLookup<ReadOnlySpan<char>>().TryAdd(canon, data)) {
            throw new Exception($"Duplicate dependency: '{canon.ToString()}'");
        }
    }

    public void Hold(RentedBuffer buffer) => _rentedBuffers.Add(buffer);

    public void Dispose()
    {
        foreach (var rented in _rentedBuffers) {
            rented.Release();
        }
    }
}