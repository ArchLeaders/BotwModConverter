using BfresLibrary;
using BfresLibrary.PlatformConverters;
using BfresLibrary.WiiU;
using BotwModConverter.Core.Attributes;
using BotwModConverter.Core.Utils;

namespace BotwModConverter.Core.Converters;

[MatchesExtension(".bfres")]
public sealed class BfresConverter : IConverter
{
    public bool ToSwitch(ConverterEngine engine, ref ModFile file)
    {
        if (file.Canon.EndsWith(".Tex2.bfres")) {
            if (engine.HasDependency(file.Canon.ToTexN('1'), out var tex1Data)) {
                using var data = file.Stream();
                TexToSwitch(engine, tex1Data, data);
                tex1Data.Dispose();
                return true;
            }

            engine.StoreDependency(file.Canon.ToTexN('2'), file.Stream());
            return false;
        }
        
        if (file.Canon.EndsWith(".Tex1.bfres")) {
            if (engine.HasDependency(file.Canon.ToTexN('2'), out var tex2Data)) {
                using var data = file.Stream();
                TexToSwitch(engine, data, tex2Data);
                tex2Data.Dispose();
                return true;
            }

            engine.StoreDependency(file.Canon.ToTexN('1'), file.Stream());
            return false;
        }
        
        using var stream = file.Stream();
        var res = new ResFile(stream);
        
        res.ChangePlatform(
            isSwitch: true,
            alignment: 4096,
            versionA: 0, 5, 0, 3,
            ConverterHandle.BOTW
        );

        res.Alignment = 0x0C;

        using var output = file.OpenWrite(out var compress);
        res.Save(output);
        
        compress?.Invoke();
        
        return true;
    }
    
    private static void TexToSwitch(ConverterEngine engine, Stream tex1Data, Stream tex2Data)
    {
        var tex1 = new ResFile(tex1Data);
        var tex2 = new ResFile(tex2Data);

        foreach (var tex in tex2.Textures.Values) {
            ((Texture)tex1.Textures[tex.Name]).MipSwizzle = ((Texture)tex).Swizzle;
            ((Texture)tex1.Textures[tex.Name]).MipData = ((Texture)tex).MipData;
        }
        
        tex1.Name = tex1.Name.Replace("Tex1", "Tex");
        
        tex1.ChangePlatform(
            isSwitch: true,
            alignment: 4096,
            versionA: 0, 5, 0, 3,
            ConverterHandle.BOTW
        );
        
        tex1.Alignment = 0x0C;

        using var output = engine.OpenWrite(
            Path.Combine("Model", $"{tex1.Name}.sbfres"),
            isCompressed: true, out var compress, ModOutputFolder.Content);
        
        tex1.Save(output);
        
        compress?.Invoke();
    }

    public bool ToWiiu(ConverterEngine engine, ref ModFile file)
    {
        if (file.Canon.EndsWith(".Tex.bfres")) {
            using var data = file.Stream();
            TexToWiiu(engine, data);
            return true;
        }
        
        throw new NotSupportedException("BFRES files can only be converted to Switch");
    }
    
    private static void TexToWiiu(ConverterEngine engine, Stream texData)
    {
        throw new NotSupportedException("BFRES texture files can only be converted to Switch");
    }
}