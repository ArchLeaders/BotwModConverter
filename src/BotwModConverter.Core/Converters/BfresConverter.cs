using BfresLibrary;
using BfresLibrary.PlatformConverters;
using BfresLibrary.WiiU;
using BotwModConverter.Core.Attributes;
using BotwModConverter.Core.Utils;
using CommunityToolkit.HighPerformance.Buffers;

namespace BotwModConverter.Core.Converters;

[MatchesExtension(".bfres")]
public sealed class BfresConverter : IConverter
{
    public SpanOwner<byte> ToSwitch(ArraySegment<byte> data, ref FileContext file, ModContext context)
    {
        if (file.Canon.EndsWith(".Tex2.bfres") && context.Has(file.Canon.ToTexN('1'), out var tex1Data)) {
            return TexToSwitch(tex1Data, data, ref file);
        }
        
        if (file.Canon.EndsWith(".Tex1.bfres") && context.Has(file.Canon.ToTexN('2'), out var tex2Data)) {
            return TexToSwitch(data, tex2Data, ref file);
        }
        
        ArgumentNullException.ThrowIfNull(data.Array, nameof(data));
        using var resMs = new MemoryStream(data.Array, data.Offset, data.Count, false, true);
        var res = new ResFile(resMs);
        
        res.ChangePlatform(
            isSwitch: true,
            alignment: 0x100,
            versionA: 0, 5, 0, 3,
            ConverterHandle.BOTW
        );

        using var ms = new MemoryStream();
        res.Save(ms);
        
        if (!ms.TryGetBuffer(out var buffer)) {
            buffer = ms.ToArray();
        }
        
        var result = SpanOwner<byte>.Allocate((int)ms.Length);
        buffer.AsSpan().CopyTo(result.Span);
        
        return result;
    }
    
    private static SpanOwner<byte> TexToSwitch(ArraySegment<byte> tex1Data, ArraySegment<byte> tex2Data, ref FileContext file)
    {
        ArgumentNullException.ThrowIfNull(tex1Data.Array, nameof(tex1Data));
        ArgumentNullException.ThrowIfNull(tex2Data.Array, nameof(tex2Data));
        
        using var tex1Ms = new MemoryStream(tex1Data.Array, tex1Data.Offset, tex1Data.Count, false, true);
        var tex1 = new ResFile(tex1Ms);
        
        using var tex2Ms = new MemoryStream(tex2Data.Array, tex2Data.Offset, tex2Data.Count, false, true);
        var tex2 = new ResFile(tex2Ms);

        foreach (var tex in tex2.Textures.Values) {
            ((Texture)tex1.Textures[tex.Name]).MipSwizzle = ((Texture)tex).Swizzle;
            ((Texture)tex1.Textures[tex.Name]).MipData = ((Texture)tex).MipData;
        }
        
        tex1.Name = tex1.Name.Replace("Tex1", "Tex");
        file.FileName = file.FileName.Replace("Tex1", "Tex");
        
        tex1.ChangePlatform(
            isSwitch: true,
            alignment: 4096,
            versionA: 0, 5, 0, 3,
            ConverterHandle.BOTW
        );
        
        tex1.Alignment = 0x0C;

        using var ms = new MemoryStream();
        tex1.Save(ms);
        
        if (!ms.TryGetBuffer(out var buffer)) {
            buffer = ms.ToArray();
        }
        
        var res = SpanOwner<byte>.Allocate((int)ms.Length);
        buffer.AsSpan().CopyTo(res.Span);
        
        return res;
    }

    public SpanOwner<byte> ToWiiu(ArraySegment<byte> data, ref FileContext file, ModContext context)
    {
        if (file.Canon.EndsWith(".Tex.bfres")) {
            return TexToWiiu(data, ref file);
        }
        
        throw new NotSupportedException("BFRES files can only be converted to Switch");
    }
    
    private static SpanOwner<byte> TexToWiiu(ArraySegment<byte> texData, ref FileContext file)
    {
        throw new NotSupportedException("BFRES texture files can only be converted to Switch");
    }
}