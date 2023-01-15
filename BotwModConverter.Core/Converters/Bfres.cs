namespace BotwModConverter.Core.Converters
{
    public class Bfres : IBotwConverter
    {
        public static Bfres Shared { get; } = new();

        public Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
