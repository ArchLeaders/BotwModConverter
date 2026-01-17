using BotwModConverter.Core.Attributes;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace BotwModConverter.Core.Converters;

[MatchesName("AnimalUnitSpeed.bin")]
public sealed class AnimalUnitSpeedConverter : IConverter
{
    public static SpanOwner<byte> ToSwitch(ArraySegment<byte> data)
    {
        Swap(data.AsSpan().Cast<byte, uint>());
        return default;
    }

    public static SpanOwner<byte> ToWiiu(ArraySegment<byte> data)
    {
        Swap(data.AsSpan().Cast<byte, uint>());
        return default;
    }

    private static void Swap(Span<uint> data)
    {
        for (int i = 0; i < data.Length; i++) {
            EndianUtils.Swap(ref data[i]);
        }
    }
}