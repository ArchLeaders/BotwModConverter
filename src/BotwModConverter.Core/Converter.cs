using Cead.Interop;

namespace BotwModConverter.Core;

public abstract class Converter
{
    public PtrHandle? NativeHandle { get; protected set; }
    public string? Path { get; private set; }

    public static T Init<T>(string path) where T : Converter, new()
    {
        return new T() {
            Path = path
        };
    }

    public abstract Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data);
    public abstract Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data);
}
