namespace BotwModConverter.Core
{
    public interface IBotwConverter
    {
        public Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data);
        public Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data);
    }
}
