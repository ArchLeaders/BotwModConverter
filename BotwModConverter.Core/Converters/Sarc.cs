namespace BotwModConverter.Core.Converters;

public class Sarc : IDataConverter
{
    public static Sarc Shared { get; } = new();

    public Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data)
    {
        throw new NotImplementedException();
    }

    public Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data)
    {
        throw new NotImplementedException();
    }
}
