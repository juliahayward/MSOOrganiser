using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Calculators;

namespace MSOCoreTests.Calculators
{
    [TestClass]
    public class Penta2010CalculatorTests
    {
        [TestMethod]
        public void CalculatorThrowsOnMissingRank()
        {
            var data = new[] { new TestCalc { Rank = 0, PentaScore = 0, Absent = false, Id = 1 } };

            var calc = new Penta2010Calculator();
            try
            {
                calc.Calculate(1, data);
                Assert.Fail("Should throw");
            }
            catch (ArgumentException) { }
            catch (Exception) { Assert.Fail("Threw wrong exception"); }
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerInSimpleCase()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = 0f, Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = 0f, Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = 0f, Absent = false, Id = 5 },
            };

            var calc = new Penta2010Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual("83.3333333333333", data[0].PentaScore);
            Assert.AreEqual("41.6666666666667", data[1].PentaScore);
            Assert.AreEqual("0", data[2].PentaScore);
            Assert.AreEqual("", data[3].PentaScore);
            Assert.AreEqual("20.8333333333333", data[4].PentaScore);
            Assert.AreEqual("62.5", data[5].PentaScore);
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerWhenTie()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = 0f, Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = 0f, Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 4 },
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 5 },
            };

            var calc = new Penta2010Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual("72.9166666666667", data[0].PentaScore);
            Assert.AreEqual("41.6666666666667", data[1].PentaScore);
            Assert.AreEqual("0", data[2].PentaScore);
            Assert.AreEqual("", data[3].PentaScore);
            Assert.AreEqual("20.8333333333333", data[4].PentaScore);
            Assert.AreEqual("72.9166666666667", data[5].PentaScore);
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerWhenTwoPerTeam()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = 0f, Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = 0f, Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = 0f, Absent = false, Id = 5 },
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 101 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 102 },
                new TestCalc { Rank = 5, PentaScore = 0f, Absent = false, Id = 103 },
                new TestCalc { Rank = 0, PentaScore = 0f, Absent = true, Id = 113 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 104 },
                new TestCalc { Rank = 2, PentaScore = 0f, Absent = false, Id = 105 },
            };

            var calc = new Penta2010Calculator();
            calc.Calculate(2, data);

            Assert.AreEqual("83.3333333333333", data[0].PentaScore);
            Assert.AreEqual("41.6666666666667", data[1].PentaScore);
            Assert.AreEqual("0", data[2].PentaScore);
            Assert.AreEqual("", data[3].PentaScore);
            Assert.AreEqual("20.8333333333333", data[4].PentaScore);
            Assert.AreEqual("62.5", data[5].PentaScore);
        }
    }
}
