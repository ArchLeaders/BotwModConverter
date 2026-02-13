using System;
using BotwModConverter.Core;
using BotwModConverter.Core.Attributes;
using CommunityToolkit.HighPerformance.Buffers;

namespace CoreSourceGenerator.Sample;

[MatchesName("Test")]
[MatchesExtension(".test", ".test.sub")]
[MatchesMagic("BY", "ABC")]
public class Test : IConverter
{
    public SpanOwner<byte> ToSwitch(ArraySegment<byte> data, ReadOnlySpan<char> canon, ModContext context) => default;

    public SpanOwner<byte> ToWiiu(ArraySegment<byte> data, ReadOnlySpan<char> canon, ModContext context) => default;
}

[MatchesName("Test2")]
[MatchesExtension(".test2", ".test2.sub")]
[MatchesMagic("BY2", "ABC2")]
public class Test2 : IConverter
{
    public SpanOwner<byte> ToSwitch(ArraySegment<byte> data, ReadOnlySpan<char> canon, ModContext context) => default;

    public SpanOwner<byte> ToWiiu(ArraySegment<byte> data, ReadOnlySpan<char> canon, ModContext context) => default;
}