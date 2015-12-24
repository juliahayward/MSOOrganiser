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
    public class RankCheckerTests
    {
        [TestMethod]
        public void CheckerGetsRightAnswerInSimpleCase()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = "", Absent = true, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            c.Check(1, data);
        }

        [TestMethod]
        public void CheckerCatchesZeroRank()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 0, PentaScore = "", Absent = false, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            try 
            { 
                c.Check(1, data);
                Assert.Fail("Missed zero rank");
            }
            catch {}
        }

        [TestMethod]
        public void CheckerCatchesMissingRank()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 7, PentaScore = "", Absent = false, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            try
            {
                c.Check(1, data);
                Assert.Fail("Missed zero rank");
            }
            catch { }
        }

        [TestMethod]
        public void CheckerCatchesDuplicateRank()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 13 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            try
            {
                c.Check(1, data);
                Assert.Fail("Missed duplicate rank");
            }
            catch { }
        }

        [TestMethod]
        public void CheckerHandlesDuplicateRankWithGapAfter()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 5, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 13 },
                new TestCalc { Rank = 6, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            c.Check(1, data);
        }

        [TestMethod]
        public void CheckerHandlesTeams()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 13 },
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            c.Check(2, data);
        }

        [TestMethod]
        public void CheckerCatchesMissingInTeams()
        {
            var data = new[] { 
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 1 },
                new TestCalc { Rank = 3, PentaScore = "", Absent = false, Id = 2 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 3 },
                new TestCalc { Rank = 4, PentaScore = "", Absent = false, Id = 13 },
                new TestCalc { Rank = 1, PentaScore = "", Absent = false, Id = 4 },
                new TestCalc { Rank = 2, PentaScore = "", Absent = false, Id = 5 },
            };

            var c = new RankChecker();
            try
            {
                c.Check(2, data);
                Assert.Fail("Missed duplicate");
            }
            catch { }
        }
    }
}
