using BotwModConverter.Core;
using BotwModConverter.Core.Attributes;

namespace CoreSourceGenerator.Sample;

[MatchesName("Test")]
[MatchesExtension(".test", ".test.sub")]
[MatchesMagic("BY", "ABC")]
public class Test : IConverter
{
    public ConvertResult ToSwitch<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
        => ConvertResult.Converted;

    public ConvertResult ToWiiu<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
        => ConvertResult.Converted;
}

[MatchesName("Test2")]
[MatchesExtension(".test2", ".test2.sub")]
[MatchesMagic("BY2", "ABC2")]
public class Test2 : IConverter
{
    public ConvertResult ToSwitch<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
        => ConvertResult.Converted;

    public ConvertResult ToWiiu<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct
        => ConvertResult.Converted;
}