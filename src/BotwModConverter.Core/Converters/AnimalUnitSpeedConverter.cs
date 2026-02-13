using BotwModConverter.Core.Attributes;
using CommunityToolkit.HighPerformance;
using Entish;

namespace BotwModConverter.Core.Converters;

[MatchesName("AnimalUnitSpeed.bin")]
public sealed class AnimalUnitSpeedConverter : IConverter
{
    public bool ToSwitch(ConverterEngine engine, ref ModFile file)
    {
        using var data = file.Rent();
        Swap(data.Span.Cast<byte, uint>());
        return true;
    }

    public bool ToWiiu(ConverterEngine engine, ref ModFile file)
    {
        using var data = file.Rent();
        Swap(data.Span.Cast<byte, uint>());
        return true;
    }

    private static void Swap(Span<uint> data)
    {
        for (int i = 0; i < data.Length; i++) {
            EndianUtils.Swap(ref data[i]);
        }
    }
}