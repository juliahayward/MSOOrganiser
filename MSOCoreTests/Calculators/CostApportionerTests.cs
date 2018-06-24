using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore;
using MSOCore.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCoreTests.Calculators
{
    [TestClass]
    public class CostApportionerTests
    {
        [TestMethod]
        public void ApportionCostsSimpleCase_RoundsToPennies()
        {
            var data = new[] {
                new TestApportionable { Cost = 15, IsApportionable = true },
                new TestApportionable { Cost = 15, IsApportionable = true },
                new TestApportionable { Cost = 15, IsApportionable = true }
            };
            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x,y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 40.0m);
            Assert.AreEqual(13.33m, data[0].Cost);
            Assert.AreEqual(13.34m, data[1].Cost);
            Assert.AreEqual(13.33m, data[2].Cost);
        }

        [TestMethod]
        public void ApportionCostsSimpleCase_DoingItTwiceGivesSameResult()
        {
            var data = new[] {
                new TestApportionable { Cost = 15, IsApportionable = true },
                new TestApportionable { Cost = 15, IsApportionable = true },
                new TestApportionable { Cost = 15, IsApportionable = true }
            };
            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x, y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 40.0m);
            apportioner.ApportionCost(data, 40.0m);
            Assert.AreEqual(13.33m, data[0].Cost);
            Assert.AreEqual(13.34m, data[1].Cost);
            Assert.AreEqual(13.33m, data[2].Cost);
        }

        [TestMethod]
        public void ApportionCostsComplexCase()
        {
            var data = new[] {
                new TestApportionable { Cost = 10, IsApportionable = true },
                new TestApportionable { Cost = 15, IsApportionable = true },
                new TestApportionable { Cost = 10, IsApportionable = true }
            };
            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x, y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 28.0m);
            Assert.AreEqual(8.0m, data[0].Cost);
            Assert.AreEqual(12.0m, data[1].Cost);
            Assert.AreEqual(8.0m, data[2].Cost);
        }

        [TestMethod]
        public void ApportionCostsWithOneToIgnore()
        {
            var data = new[] {
                new TestApportionable { Cost = 15, IsApportionable = true },
                new TestApportionable { Cost = 15, IsApportionable = false },
                new TestApportionable { Cost = 15, IsApportionable = true }
            };
            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x, y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 20.0m);
            Assert.AreEqual(10.0m, data[0].Cost);
            Assert.AreEqual(15.0m, data[1].Cost);
            Assert.AreEqual(10.0m, data[2].Cost);
        }

        [TestMethod]
        public void ApportionCostsWithAllToIgnore()
        {
            var data = new[] {
                new TestApportionable { Cost = 10, IsApportionable = false },
                new TestApportionable { Cost = 20, IsApportionable = false },
                new TestApportionable { Cost = 30, IsApportionable = false }
            };
            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x, y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 40.0m);
            Assert.AreEqual(10m, data[0].Cost);
            Assert.AreEqual(20m, data[1].Cost);
            Assert.AreEqual(30m, data[2].Cost);
        }

        [TestMethod]
        public void ApportionCostsDoesNothingWhenTotalTooLarge()
        {
            var data = new[] {
                new TestApportionable { Cost = 10, IsApportionable = true },
                new TestApportionable { Cost = 20, IsApportionable = true },
                new TestApportionable { Cost = 30, IsApportionable = true }
            };
            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x, y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 80.0m);
            Assert.AreEqual(10m, data[0].Cost);
            Assert.AreEqual(20m, data[1].Cost);
            Assert.AreEqual(30m, data[2].Cost);
        }

        [TestMethod]
        public void ApportionCostsSafeOnEmptyList()
        {
            var data = new TestApportionable[0];

            var apportioner = new CostApportioner<TestApportionable>(x => x.Cost, (x, y) => x.Cost = y, x => x.IsApportionable);
            apportioner.ApportionCost(data, 40.0m);
        }

        class TestApportionable
        {
            public decimal Cost { get; set; }
            public bool IsApportionable { get; set; }
        }
    }
}