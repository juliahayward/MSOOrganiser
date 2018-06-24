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
    public class PentaPre2010CalculatorTests
    {
        [TestMethod]
        public void CalculatorThrowsOnMissingRank()
        {
            var data = new[] { new TestCalc { Rank = 0, PentaScore = 0, Absent = false, Id = 1 } };

            var calc = new PentaPre2010Calculator();
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

            var calc = new PentaPre2010Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual(100, data[0].PentaScore);
            Assert.AreEqual(50, data[1].PentaScore);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(25, data[4].PentaScore);
            Assert.AreEqual(75, data[5].PentaScore);
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerInSimpleCaseLargeEvent()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 1 },
                new TestCalc { Rank = 2, PentaScore = 0f, Absent = false, Id = 2 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 3 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 4 },
                new TestCalc { Rank = 5, PentaScore = 0f, Absent = false, Id = 5 },
                new TestCalc { Rank = 6, PentaScore = 0f, Absent = false, Id = 6 },
                new TestCalc { Rank = 7, PentaScore = 0f, Absent = false, Id = 7 },
                new TestCalc { Rank = 8, PentaScore = 0f, Absent = false, Id = 8 },
                new TestCalc { Rank = 9, PentaScore = 0f, Absent = false, Id = 9 },
                new TestCalc { Rank = 10, PentaScore = 0f, Absent = false, Id = 10 },
                new TestCalc { Rank = 11, PentaScore = 0f, Absent = false, Id = 11 },
                new TestCalc { Rank = 12, PentaScore = 0f, Absent = false, Id = 12 },
            };

            var calc = new PentaPre2010Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual(100f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(90.90909f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(81.81818f, data[2].PentaScore, 0.00001);
            Assert.AreEqual(18.18181f, data[9].PentaScore, 0.00001);
            Assert.AreEqual(9.09090f, data[10].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[11].PentaScore, 0.00001);
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

            var calc = new PentaPre2010Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual(87.5, data[0].PentaScore);
            Assert.AreEqual(50, data[1].PentaScore);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(25, data[4].PentaScore);
            Assert.AreEqual(87.5, data[5].PentaScore);
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

            var calc = new PentaPre2010Calculator();
            calc.Calculate(2, data);

            Assert.AreEqual(100, data[0].PentaScore);
            Assert.AreEqual(50, data[1].PentaScore);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(25, data[4].PentaScore);
            Assert.AreEqual(75, data[5].PentaScore);
        }
    }
}
