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
            var data = new[] { new TestCalc { Rank = 0, PentaScore = "", Absent = false, Id = 1 } };

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
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = "", Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var calc = new Penta2015Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual("80", data[0].PentaScore);
            Assert.AreEqual("40", data[1].PentaScore);
            Assert.AreEqual("0", data[2].PentaScore);
            Assert.AreEqual("", data[3].PentaScore);
            Assert.AreEqual("20", data[4].PentaScore);
            Assert.AreEqual("60", data[5].PentaScore);
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerWhenTie()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = "", Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 5 },
            };

            var calc = new Penta2015Calculator();
            calc.Calculate(1, data);

            Assert.AreEqual("70", data[0].PentaScore);
            Assert.AreEqual("40", data[1].PentaScore);
            Assert.AreEqual("0", data[2].PentaScore);
            Assert.AreEqual("", data[3].PentaScore);
            Assert.AreEqual("20", data[4].PentaScore);
            Assert.AreEqual("70", data[5].PentaScore);
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerWhenTwoPerTeam()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = "", Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 101 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 102 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 103 },
                new TestCalc { Rank = 0, PentaScore = "", Absent = true, Id = 113 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 104 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 105 },
            };

            var calc = new Penta2015Calculator();
            calc.Calculate(2, data);

            Assert.AreEqual("80", data[0].PentaScore);
            Assert.AreEqual("40", data[1].PentaScore);
            Assert.AreEqual("0", data[2].PentaScore);
            Assert.AreEqual("", data[3].PentaScore);
            Assert.AreEqual("20", data[4].PentaScore);
            Assert.AreEqual("60", data[5].PentaScore);
        }
    }

    public class TestCalc : IPentaCalculable
    {
        public int Rank { get; set; }
        public string PentaScore { get; set; }
        public bool Absent { get; set; }
        public int Id { get; set; }
    }
}
