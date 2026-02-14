using BotwModConverter.Core.Attributes;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace BotwModConverter.Core.Converters;

[MatchesExtension(".fmc")]
public sealed class FarModelCullConverter : IConverter
{
    public ConvertResult ToSwitch<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
    {
        using var data = file.Rent();
        Swap(data.Span.Cast<byte, uint>());
        return ConvertResult.Converted;
    }

    public ConvertResult ToWiiu<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
    {
        using var data = file.Rent();
        Swap(data.Span.Cast<byte, uint>());
        return ConvertResult.Converted;
    }

    private static void Swap(Span<uint> data)
    {
        for (int i = 0; i < data.Length; i++) {
            EndianUtils.Swap(ref data[i]);
        }
    }
}