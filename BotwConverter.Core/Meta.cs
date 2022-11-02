namespace BotwConverter.Core
{
    public static class Meta
    {
        public static List<string> UnSupported { get; } = new() { ".bcamanim", ".sbcamanim", ".sesetlist", ".sbfarc", ".sbreviewtex", ".sstats", ".sbstftex", ".bfsar", ".sbmapopen", ".sbmaptex" };
        public static List<string> Supported { get; } = new() { ".sbfres", ".sbitemico", ".hkcl", ".hkrg", ".shknm2", ".shktmrb", ".bars", ".bfstm", ".bflim", ".sblarc", ".bcamanim" };
        public static List<string> BfresExt { get; } = new() { ".sbfres", ".sbitemico", ".bcamanim" };
        public static List<string> HavokExt { get; } = new() { ".hkcl", ".hkrg", ".shknm2" };
        public static List<string> LayoutExt { get; } = new() { ".bflan", ".bgsh", ".bnsh", ".bushvt", ".bflyt", ".bflim", ".bntx" };
        public static List<string> SoundExt { get; } = new() { ".bfstm", ".bfstp", ".bfwav", ".bars" };
    }
}
