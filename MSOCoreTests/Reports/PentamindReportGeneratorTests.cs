using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Reports;
using MSOCore.Calculators;

namespace MSOCoreTests.Reports
{
    [TestClass]
    public class PentamindReportGeneratorTests
    {
        [TestMethod]
        public void PRG_HandlesCaseOfDominatedLongEvent1_Correctly()
        {
            var prg = new PentamindMetaScoreCalculator();

            // Will overlook the best long session (AAAA) in order to pick up big AAA1
            // - because we can get long events BBBB and CCCC
            var scores = new List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore>()
            {
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "AA", IsLongSession = true, Code = "AAAA" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 9, GameCode = "BB", IsLongSession = true, Code = "BBBB" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 8, GameCode = "CC", IsLongSession = true, Code = "CCCC" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 100, GameCode = "AA", IsLongSession = false, Code = "AAA1" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "DD", IsLongSession = false, Code = "DDDD" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "EE", IsLongSession = false, Code = "EEEE" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "FF", IsLongSession = false, Code = "FFFF" }
            };

            var score = prg.SelectBestScores(scores, 2, 5, 2015);
            Assert.AreEqual(5, score.Count());
            Assert.AreEqual(137, score.Sum(x => x.Score));

        }


        [TestMethod]
        public void PRG_HandlesCaseOfDominatedLongEvent2_Correctly()
        {
            var prg = new PentamindMetaScoreCalculator();

            var scores = new List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore>()
            {
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "AA", IsLongSession = true, Code = "AAAA" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 9, GameCode = "BB", IsLongSession = true, Code = "BBBB" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 8, GameCode = "CC", IsLongSession = true, Code = "CCCC" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 100, GameCode = "BB", IsLongSession = false, Code = "BBB1" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "DD", IsLongSession = false, Code = "DDDD" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "EE", IsLongSession = false, Code = "EEEE" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "FF", IsLongSession = false, Code = "FFFF" }

            };

            var score = prg.SelectBestScores(scores, 2, 5, 2015);
            Assert.AreEqual(5, score.Count());
            Assert.AreEqual(138, score.Sum(x => x.Score));

        }

        [TestMethod]
        public void PRG_Prefers4WithLowerScoreOver4WithHigher()
        {
            var prg = new PentamindMetaScoreCalculator();

            // If you take either AA or BB for 100, you can't get five events with two big ones
            // so prefer the 
            var scores = new List<PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore>()
            {
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "AA", IsLongSession = true, Code = "AAAA" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 9, GameCode = "BB", IsLongSession = true, Code = "BBBB" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 8, GameCode = "CC", IsLongSession = false, Code = "CCCC" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 100, GameCode = "AA", IsLongSession = false, Code = "AAA1" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 100, GameCode = "BB", IsLongSession = false, Code = "BBB1" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "DD", IsLongSession = false, Code = "DDDD" },
                new PentamindStandingsGenerator.PentamindStandingsReportVm.EventScore()
                { Score = 10, GameCode = "EE", IsLongSession = false, Code = "EEEE" }

            };

            var score = prg.SelectBestScores(scores, 2, 5, 2015);
            Assert.AreEqual(5, score.Count());
            Assert.AreEqual(47, score.Sum(x => x.Score));

        }
    }
}
