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
    private readonly Platform _platform;
    private readonly string _sourceFolder;

    public readonly string OutputFolder;

    public ConverterEngine(string sourceFolderPath, string? output = null)
    {
        _platform = ModFolderUtils.IsSwitchMod(sourceFolderPath) ? Platform.Switch : Platform.WiiU;
        _sourceFolder = sourceFolderPath;
        OutputFolder = output ?? $"{sourceFolderPath}-{_platform}";
    }

    public bool Convert()
    {
        foreach (var file in ModFolderUtils.EnumerateFiles(_sourceFolder, OutputFolder, _platform)) {
            Convert(file.Input, file.Output);
        }

        return true;
    }

    public bool ConvertParallel()
    {
        var res = Parallel.ForEach(
            ModFolderUtils.EnumerateFiles(_sourceFolder, OutputFolder, _platform),
            (file, _) => Convert(file.Input, file.Output)
        );

        if (res.IsCompleted) {
            return true;
        }

        // TODO: Log failure
        return false;
    }

    public bool Convert(string filePath, string outputFolderPath)
    {
        var file = ModFile.FromFile(filePath, outputFolderPath);
        return Convert(ref file);
    }

    public bool Convert(ref ModFile file)
    {
        if (ConverterLookup.Find(file.OutputFileName.ToCanon()) is { } converter) {
            return _platform switch {
                Platform.Switch => converter.ToWiiu(this, ref file),
                Platform.WiiU => converter.ToSwitch(this, ref file),
                _ => throw new NotSupportedException($"Unsupported platform: {_platform}")
            };
        }

        using var headerBuffer = file.Rent(0x20);
        if (ConverterLookup.Find(file.OutputFileName.ToCanon()) is { } converterFromData) {
            return _platform switch {
                Platform.Switch => converterFromData.ToWiiu(this, ref file),
                Platform.WiiU => converterFromData.ToSwitch(this, ref file),
                _ => throw new NotSupportedException($"Unsupported platform: {_platform}")
            };
        }

        file.WriteCopy();
        return true;
    }

    public Stream OpenWrite(string relativeFilePath, bool isCompressed, out Action? compress, ModOutputFolder outputFolder)
    {
        var absoluteFilePath = Path.Combine(
            ModFolderUtils.GetPlatformOutput(OutputFolder, _platform, outputFolder), relativeFilePath);

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