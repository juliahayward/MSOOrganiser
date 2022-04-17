using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore.Calculators;
using MSOCore.Reports;
using System.Collections.Generic;

namespace MSOCoreTests.Reports
{
    [TestClass]
    public class GrandPrixMetaScoreCalculatorTests
    {
        [TestMethod]
        public void GPCalculator_HandlesSimpleEvent_Correctly()
        {
            var prg = new GrandPrixMetaScoreCalculator();

            var scores = new List<GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore>()
            {
                new GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore()
                { Score = 10, GameCode = "AA", GameVariantCode=1, EventCode="AVariantOne", GPCategory="vowels" },
                new GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore()
                { Score = 30, GameCode = "AA", GameVariantCode=1, EventCode="AVariantOneAgain", GPCategory="vowels" },
                new GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore()
                { Score = 8, GameCode = "AA", GameVariantCode=2, EventCode="AVariantTwo", GPCategory="vowels" },
                new GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore()
                { Score = 14, GameCode = "EE", GameVariantCode=51, EventCode="EVariantOne", GPCategory="vowels" },
                new GrandPrixStandingsGenerator.GrandPrixStandingsReportVm.EventScore()
                { Score = 9, GameCode = "FF", GameVariantCode=61, EventCode="FVariantOne", GPCategory="consonants" },
            };

            var eligibleScores = prg.SelectEligibleScores(scores);
            Assert.AreEqual(4, eligibleScores.Count);
            // Best in category "vowels" is doubled
            Assert.AreEqual(60, eligibleScores[0].Score);
            Assert.AreEqual("AVariantOneAgain", eligibleScores[0].EventCode);
            // Best in category "consonants" is doubled
            Assert.AreEqual(18, eligibleScores[1].Score);
            Assert.AreEqual("FVariantOne", eligibleScores[1].EventCode);
            // Other "vowels"
            Assert.AreEqual(14, eligibleScores[2].Score);
            Assert.AreEqual("EVariantOne", eligibleScores[2].EventCode);
            // This is halved as it's a variant of A
            Assert.AreEqual(4, eligibleScores[3].Score);
            Assert.AreEqual("AVariantTwo", eligibleScores[3].EventCode);
        }
    }
}
