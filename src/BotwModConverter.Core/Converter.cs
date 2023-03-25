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

    public static Converter Get(string path, bool isYaz0)
    {
        // Custom converter for the actorinfo
        if (Path.GetFileName(path) == "ActorInfo.product.sbyml") {
            return Init<ActorInfoConverter>(path);
        }

        // Search for the rest using the file extension
        string ext = Path.GetExtension(path).Remove(0, isYaz0 ? 2 : 1);
        return ext switch {
            "bars" => Init<BarsConverter>(path),

            "bcamanim" or "bfres" or "bitemico" or
            "bmapopen" or "bmaptex" or "breviewtex" or
            "bstftex" => Init<BfresConverter>(path),

            // Binary Ecosystem ("beco")
            // Binary Loop Asset List ("blal")
            // BFEV, BFSTM, Sound, etc ("bfstm" or "bsftp")

            "baischedule" or "baniminfo" or "bgdata" or
            "bgsvdata" or "bquestpack" or "byml" or
            "mubin" => Init<BymlConverter>(path),

            // AnimationDrivenSpeed/AnimalUnitSpeed (".bin") ???
            // Emitter Set List (".esetlist")
            // Grass Colour Layout ("grass.extm")

            "hkcl" or "hknm2" or "hkrb" or
            "hkrg" or "hksc" or
            "hktm" => Init<HavokConverter>(path),

            // MATE (".mate") ???
            "msbt" => Init<MsbtConverter>(path),
            // ResourceSizeTable (".rstb")

            "bactorpack" or "beventpack" or "bgenv" or
            "blarc" or "bmodelsh" or "genvb" or
            "sarc" or "stats" or "stera" or
            "baatarc" or "pack" => Init<SarcConverter>(path),

            // Terrain Scene Binary (".tscb")
            // Water Layout ("water.extm")

            _ => throw new NotSupportedException($"Could not find a converter for the file '{path}'"),
        };
    }
}
