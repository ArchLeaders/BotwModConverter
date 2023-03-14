using BotwModConverter.Core;

namespace BotwModConverter.UnitTests;

[TestClass]
public class BotwConverterTests
{
    [TestMethod]
    [DataRow("../../../test-data/Bunker")]
    public async Task Convert(string mod)
    {
        await BotwConverter.Convert(mod);
    }
}
