using System.Runtime.CompilerServices;
using BotwModConverter.Core.Attributes;
using BotwModConverter.Core.FileFormats.BinaryEco;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace BotwModConverter.Core.Converters;

[MatchesExtension(".beco")]
public sealed class BinaryEcoConverter : IConverter
{
    public unsafe SpanOwner<byte> ToSwitch(ArraySegment<byte> data, ref FileContext file, ModContext context)
    {
        fixed (byte* ptr = data.AsSpan()) {
            int offset = SwapHeader((ResEcoHeader*)ptr, isSwappingToNative: BitConverter.IsLittleEndian);
            SwapSegments(offset, ref data);
        }

        return default;
    }

    public unsafe SpanOwner<byte> ToWiiu(ArraySegment<byte> data, ref FileContext file, ModContext context)
    {
        fixed (byte* ptr = data.AsSpan()) {
            int offset = SwapHeader((ResEcoHeader*)ptr, isSwappingToNative: !BitConverter.IsLittleEndian);
            SwapSegments(offset, ref data);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int SwapHeader(ResEcoHeader* ptr, bool isSwappingToNative)
    {
        var header = *ptr;
        ResEcoHeader.Swap(ptr);

        if (isSwappingToNative) {
            header = *ptr;
        }

        uint* offsets = (uint*)(++ptr);
        for (int i = 0; i < header.RowCount; i++) {
            EndianUtils.Swap(ref offsets[i]);
        }

        return sizeof(ResEcoHeader) + header.RowCount * sizeof(uint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapSegments(int offset, ref ArraySegment<byte> data)
    {
        var segments = data.AsSpan(offset..).Cast<byte, ushort>();
        for (int i = 0; i < segments.Length; i++) {
            EndianUtils.Swap(ref segments[i]);
        }
    }
}