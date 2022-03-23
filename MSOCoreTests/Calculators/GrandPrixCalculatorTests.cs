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
    public class GrandPrixCalculatorTests
    {
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

            var calc = new GrandPrixCalculator();
            calc.Calculate(1, data);

            Assert.AreEqual(40f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(20f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(0, data[2].PentaScore, 0.00001);        // lower than 50%
            Assert.AreEqual(0, data[3].PentaScore, 0.00001);
            Assert.AreEqual(12f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(28f, data[5].PentaScore, 0.00001);
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

            var calc = new GrandPrixCalculator();
            calc.Calculate(1, data);

            Assert.AreEqual(40f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(28f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(20f, data[2].PentaScore, 0.00001);
            Assert.AreEqual(12f, data[3].PentaScore, 0.00001);
            Assert.AreEqual(2f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(2f, data[5].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[6].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[7].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[8].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[9].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[10].PentaScore, 0.00001);
        }

        [TestMethod]
        public void CalculatorGetsRightAnswerWhenTie()
        {
            var data = new[] {
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 1 },
                new TestCalc { Rank = 2, PentaScore = 0f, Absent = false, Id = 2 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 3 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 4 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 5 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 6 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 7 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 8 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 9 },
                new TestCalc { Rank = 10, PentaScore = 0f, Absent = false, Id = 10 },
                new TestCalc { Rank = 11, PentaScore = 0f, Absent = false, Id = 11 },
                new TestCalc { Rank = 12, PentaScore = 0f, Absent = false, Id = 12 },
                new TestCalc { Rank = 13, PentaScore = 0f, Absent = false, Id = 13 },
                new TestCalc { Rank = 14, PentaScore = 0f, Absent = false, Id = 14 },
                new TestCalc { Rank = 15, PentaScore = 0f, Absent = false, Id = 15 },
                new TestCalc { Rank = 16, PentaScore = 0f, Absent = false, Id = 16 },
                new TestCalc { Rank = 17, PentaScore = 0f, Absent = false, Id = 17 },
                new TestCalc { Rank = 18, PentaScore = 0f, Absent = false, Id = 18 },
                new TestCalc { Rank = 19, PentaScore = 0f, Absent = false, Id = 19 },
            };

            var calc = new GrandPrixCalculator();
            calc.Calculate(1, data);

            Assert.AreEqual(40f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(28f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(20f, data[2].PentaScore, 0.00001);
            Assert.AreEqual(5f, data[3].PentaScore, 0.00001);
            Assert.AreEqual(5f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(5f, data[5].PentaScore, 0.00001);
            Assert.AreEqual(5f, data[6].PentaScore, 0.00001);
            Assert.AreEqual(5f, data[7].PentaScore, 0.00001);
            Assert.AreEqual(5f, data[8].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[9].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[10].PentaScore, 0.00001);
            Assert.AreEqual(0f, data[11].PentaScore, 0.00001);

        }

        [TestMethod]
        // https://mindsportsolympiad.com/mso-grand-prix/?msclkid=3915208aaaef11ec82ffd46aaa61f0dd#1642103865073-8a2a8ac9-f8a6
        public void CalculatorGetsRightAnswerWhenComplexTie() 
        {
            var data = new[] {
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = 0f, Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = 0f, Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = 0f, Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = 0f, Absent = false, Id = 4 },
                new TestCalc { Rank = 1, PentaScore = 0f, Absent = false, Id = 5 },
            };

            var calc = new GrandPrixCalculator();
            calc.Calculate(1, data);

            Assert.AreEqual(34f, data[0].PentaScore, 0.00001);
            Assert.AreEqual(20f, data[1].PentaScore, 0.00001);
            Assert.AreEqual(0, data[2].PentaScore);
            Assert.AreEqual(0, data[3].PentaScore);
            Assert.AreEqual(12f, data[4].PentaScore, 0.00001);
            Assert.AreEqual(34f, data[5].PentaScore, 0.00001);
        }
    }
}
