using BotwModConverter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotwModConverter.UnitTests
{
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
}
