using Cead.Interop;

namespace BotwModConverter.Core;

public abstract class Converter
{
    protected string _path = null!;

    public PtrHandle? NativeHandle { get; protected set; }

    public static T Init<T>(string path) where T : Converter, new()
    {
        return new T() {
            _path = path
        };
    }

    public abstract Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data);
    public abstract Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data);
}
