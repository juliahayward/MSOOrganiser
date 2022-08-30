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

            var data = generator.GetItems(1, 100);

            Assert.AreEqual(data.Page, 1);
            Assert.IsTrue(data.Entries.Any());
        }

        [TestMethod]
        public void IntegrationTest_YearMedals()
        {
            var generator = new YearMedalsGenerator();

            var data = generator.GetModel(2014, null);

            Assert.IsTrue(data.Medals.Any());
        }
    }
}
