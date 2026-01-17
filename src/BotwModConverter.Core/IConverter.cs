using CommunityToolkit.HighPerformance.Buffers;

namespace BotwModConverter.Core;

public interface IConverter
{
    static abstract SpanOwner<byte> ToSwitch(ArraySegment<byte> data);
    
    static abstract SpanOwner<byte> ToWiiu(ArraySegment<byte> data);
}