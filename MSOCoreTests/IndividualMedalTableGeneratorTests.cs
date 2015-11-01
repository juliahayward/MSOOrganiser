using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Reports;

namespace MSOCoreTests
{
    [TestClass]
    public class IndividualMedalTableGeneratorTests
    {
        [TestMethod]
        public void IntegrationTest()
        {
            var generator = new IndividualMedalTableGenerator();

            var data = generator.GetItems();

            Assert.IsTrue(data.Any());
        }
    }
}
