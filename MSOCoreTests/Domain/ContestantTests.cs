using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOCore;

namespace MSOCoreTests.Domain
{
    [TestClass]
    public class ContestantTests
    {
        [TestMethod]
        public void IsJunior_CalculatesCorrectly()
        {
            var olympiad = new Olympiad_Info() { AgeDate = new DateTime(2016, 8, 21), JnrAge = 18 };
            // if JnrAge is 18, then your nineteenth birthday must be after the start of the olympiad
            var contestant = new Contestant() { DateofBirth = new DateTime(1997, 8, 21) };
            var isJunior = contestant.IsJuniorForOlympiad(olympiad);
            Assert.IsFalse(isJunior);
            contestant = new Contestant() { DateofBirth = new DateTime(1997, 8, 22) };
            isJunior = contestant.IsJuniorForOlympiad(olympiad);
            Assert.IsTrue(isJunior);
            // Null date of birth disqualifies you from being Junior
            contestant = new Contestant() { DateofBirth = null };
            isJunior = contestant.IsJuniorForOlympiad(olympiad);
            Assert.IsFalse(isJunior);
        }

        [TestMethod]
        public void IsSenior_CalculatesCorrectly()
        {
            var olympiad = new Olympiad_Info() { AgeDate = new DateTime(2016, 8, 21), SnrAge = 60 };
            // if SnrAge is 60, then your sixtieth birthday must be at the start of the olympiad or earlier
            var contestant = new Contestant() { DateofBirth = new DateTime(1956, 8, 21) };
            var isSenior = contestant.IsSeniorForOlympiad(olympiad);
            Assert.IsTrue(isSenior);
            contestant = new Contestant() { DateofBirth = new DateTime(1956, 8, 22) };
            isSenior = contestant.IsSeniorForOlympiad(olympiad);
            Assert.IsFalse(isSenior);
            // Null date of birth disqualifies you from being Senior
            contestant = new Contestant() { DateofBirth = null };
            isSenior = contestant.IsSeniorForOlympiad(olympiad);
            Assert.IsFalse(isSenior);
        }
    }

}
