using CommunityToolkit.HighPerformance.Buffers;

namespace BotwModConverter.Core;

public interface IConverter
{
    SpanOwner<byte> ToSwitch(ArraySegment<byte> data, ref FileContext file, ModContext context);
    
    SpanOwner<byte> ToWiiu(ArraySegment<byte> data, ref FileContext file, ModContext context);
}