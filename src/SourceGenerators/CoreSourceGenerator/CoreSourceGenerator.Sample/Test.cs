using BotwModConverter.Core;
using BotwModConverter.Core.Attributes;

namespace CoreSourceGenerator.Sample;

[MatchesName("Test")]
[MatchesExtension(".test", ".test.sub")]
[MatchesMagic("BY", "ABC")]
public class Test : IConverter
{
    public bool ToSwitch(ConverterEngine engine, ref ModFile file) => false;

    public bool ToWiiu(ConverterEngine engine, ref ModFile file) => false;
}

[MatchesName("Test2")]
[MatchesExtension(".test2", ".test2.sub")]
[MatchesMagic("BY2", "ABC2")]
public class Test2 : IConverter
{
    public bool ToSwitch(ConverterEngine engine, ref ModFile file) => false;

    public bool ToWiiu(ConverterEngine engine, ref ModFile file) => false;
}