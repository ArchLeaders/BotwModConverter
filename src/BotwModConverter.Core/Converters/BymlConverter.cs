using Cead;

namespace BotwModConverter.Core.Converters;

public class BymlConverter : Converter
{
    public override Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data)
    {
        Byml byml = Byml.FromBinary(data);
        NativeHandle = byml.ToBinary(out Span<byte> converted, bigEndian: false);
        return converted;
    }

    public override Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data)
    {
        Byml byml = Byml.FromBinary(data);
        NativeHandle = byml.ToBinary(out Span<byte> converted, bigEndian: true);
        return converted;
    }
}
