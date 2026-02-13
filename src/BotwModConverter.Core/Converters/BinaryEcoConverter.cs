using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using BotwModConverter.Core.Attributes;
using BotwModConverter.Core.FileFormats.BinaryEco;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Entish;

namespace BotwModConverter.Core.Converters;

[MatchesExtension(".beco")]
public sealed unsafe class BinaryEcoConverter : IConverter
{
    public bool ToSwitch(ConverterEngine engine, ref ModFile file)
    {
        using var buffer = file.Rent();
        var data = buffer.Span;
        
        fixed (byte* ptr = data) {
            int offset = SwapHeader((ResEcoHeader*)ptr, isSwappingToNative: BitConverter.IsLittleEndian);
            SwapSegments(offset, ref data);
        }

        return true;
    }

    public bool ToWiiu(ConverterEngine engine, ref ModFile file)
    {
        using var buffer = file.Rent();
        var data = buffer.Span;
        
        fixed (byte* ptr = data) {
            int offset = SwapHeader((ResEcoHeader*)ptr, isSwappingToNative: !BitConverter.IsLittleEndian);
            SwapSegments(offset, ref data);
        }

        return true;
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
    public static void SwapSegments(int offset, ref Span<byte> data)
    {
        var segments = data[offset..].Cast<byte, ushort>();
        for (int i = 0; i < segments.Length; i++) {
            EndianUtils.Swap(ref segments[i]);
        }
    }
}