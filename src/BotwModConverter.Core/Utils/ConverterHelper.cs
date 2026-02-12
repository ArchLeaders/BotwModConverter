using Revrs.Buffers;

namespace BotwModConverter.Core.Utils;

public enum ConverterOperation
{
    Copy,
    Convert
}

public class ConverterHelper
{
    public static ConverterOperation ConvertOrCopy(string file, string outputFilePath, ModContext context)
    {
        if (Path.GetDirectoryName(outputFilePath) is { } folderPath) {
            Directory.CreateDirectory(folderPath);
        }
        
        using var fs = File.OpenRead(file);
        using var buffer = ArraySegmentOwner<byte>.Allocate((int)fs.Length);
        fs.ReadExactly(buffer.Segment);

        if (FindConverter(ModHelper.GetCanonName(file), buffer.Segment) is not { } converter) {
            File.WriteAllBytes(outputFilePath, buffer.Segment);
            return ConverterOperation.Copy;
        }

        using var result = context.IsSwitch
            ? converter.ToWiiu(buffer.Segment, context)
            : converter.ToSwitch(buffer.Segment, context);

        File.WriteAllBytes(outputFilePath, result.Span.IsEmpty ? buffer.Segment : result.Span);
        
        return ConverterOperation.Convert;
    }

    private static IConverter? FindConverter(ReadOnlySpan<char> fileName, ReadOnlySpan<byte> data)
    {
        return ConverterLookup.Find(fileName) ?? ConverterLookup.Find(data);
    }
}