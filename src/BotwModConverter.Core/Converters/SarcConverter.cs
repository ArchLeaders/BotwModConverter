using BotwModConverter.Core.Attributes;
using BotwModConverter.Core.Utils;
using Revrs;
using SarcLibrary;

namespace BotwModConverter.Core.Converters;

[MatchesMagic("SARC")]
public class SarcConverter : IConverter
{
    public ConvertResult ToSwitch<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
    {
        return ConvertFiles(engine, ref file);
    }

    public ConvertResult ToWiiu<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
    {
        return ConvertFiles(engine, ref file);
    }

    public ConvertResult ConvertFiles<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
    {
        using var buffer = file.Rent();
        var sarc = Sarc.FromBinary(buffer.Segment);
        
        // TODO: Pull vanilla for re-constructed output

        bool hasChanges = false;
        bool hasDependencies = false;
        
        foreach (var (name, entryData) in sarc) {
            if (HashUtils.IsVanilla(name, entryData, engine.Platform)) {
                continue;
            }
            
            var modFile = SarcModFile.FromEntry(sarc, name, entryData);
            var result = engine.Convert(ref modFile);

            if (result.WasDependency) {
                hasDependencies = true;
            }
            
            if (result.WasConverted) {
                Console.WriteLine(name);
                hasChanges = true;
            }

            if (name.Contains(".Tex1") || name.Contains(".Tex2")) {
                sarc.Remove(name);
                hasChanges = true;
            }
        }

        if (hasDependencies) {
            buffer.DelayReleaseTo(engine);
        }

        if (!hasChanges) {
            file.WriteCopy();
            return ConvertResult.Copied;
        }

        using var output = file.OpenWrite<T>(out var compress);
        sarc.Write(output, engine.Platform switch {
            Platform.Switch => Endianness.Big,
            Platform.WiiU => Endianness.Little,
            _ => throw new NotSupportedException($"Unsupported platform: {engine.Platform}")
        });

        compress?.Invoke(file);
        return ConvertResult.Converted;
    }
}