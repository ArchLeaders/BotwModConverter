using BotwModConverter.Core.Attributes;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace BotwModConverter.Core.Converters;

[MatchesExtension(".fmc")]
public sealed class FarModelCullConverter : IConverter
{
    public SpanOwner<byte> ToSwitch(ArraySegment<byte> data, ref FileContext file, ModContext context)
    {
        Swap(data.AsSpan().Cast<byte, uint>());
        return default;
    }

    public SpanOwner<byte> ToWiiu(ArraySegment<byte> data, ref FileContext file, ModContext context)
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