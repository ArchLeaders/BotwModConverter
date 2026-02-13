using System.Runtime.CompilerServices;
using CsYaz0;
using Revrs.Buffers;

namespace BotwModConverter.Core.Utils;

public enum ConverterOperation
{
    Copy,
    Convert
}

public class ConverterHelper
{
    public static ConverterOperation ConvertOrCopy(string file, string outputFilePath, string relativePath, ModContext context)
    {
        if (Path.GetDirectoryName(outputFilePath) is { } folderPath) {
            Directory.CreateDirectory(folderPath);
        }
        
        using var fs = File.OpenRead(file);
        using var buffer = ArraySegmentOwner<byte>.Allocate((int)fs.Length);
        fs.ReadExactly(buffer.Segment);
        var data = buffer.Segment.AsSpan();

        var canon = ModHelper.GetCanonName(relativePath);
        if (FindConverter(canon, data) is not { } converter) {
            File.WriteAllBytes(outputFilePath, data);
            return ConverterOperation.Copy;
        }

        if (data.Length > 0x11 && Unsafe.As<byte, uint>(ref data[0]) == Yaz0.MAGIC) {
            var decompressed = ArraySegmentOwner<byte>.Allocate(Yaz0.GetDecompressedSize(data));
            Yaz0.Decompress(data, decompressed.Segment);
            
            using var result = context.IsSwitch
                ? converter.ToWiiu(decompressed.Segment, canon, context)
                : converter.ToSwitch(decompressed.Segment, canon, context);

            using var compressed = Yaz0.Compress(result.Length == 0 ? decompressed.Segment : result.Span);

            File.WriteAllBytes(outputFilePath, compressed.AsSpan());
            return ConverterOperation.Convert;
        }

        {
            using var result = context.IsSwitch
                ? converter.ToWiiu(buffer.Segment, canon, context)
                : converter.ToSwitch(buffer.Segment, canon, context);

            File.WriteAllBytes(outputFilePath, result.Length == 0 ? buffer.Segment : result.Span);
        }
        
        return ConverterOperation.Convert;
    }

    private static IConverter? FindConverter(ReadOnlySpan<char> fileName, ReadOnlySpan<byte> data)
    {
        return ConverterLookup.Find(fileName) ?? ConverterLookup.Find(data);
    }
}