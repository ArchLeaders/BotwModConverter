using FluentAssertions;
using BotwModConverter.Core.Utils;

namespace UnitTests;

public class GetCanonNameTests
{
    [Theory]
    [InlineData(".sr", ".r")]
    [InlineData("Test.sbarc", "Test.barc")]
    [InlineData("Test.sarc", "Test.sarc")]
    [InlineData("F:\\Some\\Path\\And\\File.ssarc", "File.sarc")]
    public static void ShouldStripSPrefix(string source, string result)
    {
        source.ToCanon().ToString().Should().Be(result);
    }
}