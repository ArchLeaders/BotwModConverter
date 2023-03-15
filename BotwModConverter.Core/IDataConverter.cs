namespace BotwModConverter.Core;

public interface IDataConverter
{
    public Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data);
    public Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data);
}
