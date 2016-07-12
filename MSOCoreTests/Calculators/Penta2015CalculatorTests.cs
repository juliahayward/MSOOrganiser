using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Calculators;

namespace MSOCoreTests
{
    [TestClass]
    public class Penta2015CalculatorTests
    {
        [TestMethod]
        public void CalculatorThrowsOnMissingRank()
        {
            var data = new[] { new TestCalc { Rank = 0, PentaScore = 0f, Absent = false, Id = 1 } };

            var calc = new Penta2015Calculator();
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

            var calc = new Penta2015Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual(83.33333f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(41.66666f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(20.83333f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(62.5, data[5].PentaScore, 0.00001);
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

            var calc = new Penta2015Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual(92.30769f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(83.91608f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(75.52447f, data[2].PentaScore, 0.00001);
            Assert.AreEqual(16.78321f, data[9].PentaScore, 0.00001);
            Assert.AreEqual(8.39160f, data[10].PentaScore, 0.00001);
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

            var calc = new Penta2015Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual(72.91667f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(41.66666f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(20.83333f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(72.91667f, data[5].PentaScore, 0.00001);
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

            var calc = new Penta2015Calculator();
            calc.Calculate(2, data);

            Assert.AreEqual(83.33333f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(41.66667f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(20.83333f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(62.5f, data[5].PentaScore, 0.00001);
        }
    }

    public class TestCalc : IPentaCalculable
    {
        public string Score { get; set; }
        public int Rank { get; set; }
        public float PentaScore { get; set; }
        public bool Absent { get; set; }
        public int Id { get; set; }
    }
}
