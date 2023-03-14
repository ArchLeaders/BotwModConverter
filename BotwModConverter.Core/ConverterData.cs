namespace BotwModConverter.Core;

public static class ConverterData
{
    public static string[] UnSupported { get; } = new string[] {
        ".bcamanim", ".sbcamanim", ".sesetlist", ".sbfarc",
        ".sbreviewtex", ".sstats", ".sbstftex", ".bfsar",
        ".sbmapopen", ".sbmaptex"
    };

    public static string[] Supported { get; } = new string[] {
        ".sbfres", ".sbitemico", ".hkcl", ".hkrg",
        ".shknm2", ".shktmrb", ".bars", ".bfstm",
        ".bflim", ".sblarc", ".bcamanim"
    };

    public static string[] BfresExt { get; } = new string[] {
        ".sbfres", ".sbitemico", ".bcamanim"
    };

    public static string[] HavokExt { get; } = new string[] {
        ".hkcl", ".hkrg", ".shknm2"
    };

    public static string[] LayoutExt { get; } = new string[] {
        ".bflan", ".bgsh", ".bnsh", ".bushvt",
        ".bflyt", ".bflim", ".bntx"
    };

    public static string[] SoundExt { get; } = new string[] {
        ".bfstm", ".bfstp", ".bfwav", ".bars"
    };
}
