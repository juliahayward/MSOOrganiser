using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSOCore.Calculators;

namespace MSOWeb.Controllers
{
    [AllowAnonymous]
    public class ReportController : Controller
    {
        public ActionResult Nationality()
        {
            var generator = new NationalityReportGenerator();

            var model = generator.GetItemsForLatest();

            return View(model);
        }

        public ActionResult IndividualMedals(int page = 1, bool header = true)
        {
            ViewBag.Title = "Mind Sports Olympiad Live Results";
            if (header)
            {
                ViewBag.Layout = "~/Views/Shared/_NewLayout.cshtml";
                ViewBag.TitleWanted = true;
            }
            else
            {
                ViewBag.Layout = "~/Views/Shared/_NewLayoutNoHeader.cshtml";
                ViewBag.TitleWanted = false;
            }

            var generator = new IndividualMedalTableGenerator();

            var model = generator.GetItems(page, 100);

            return View(model);
        }

        public ActionResult ContestantMedals(int contestantId)
        {
            var generator = new ContestantMedalsGenerator();

            var model = generator.GetModel(contestantId);

            return View(model);
        }

        public ActionResult YearMedals(int year)
        {
            var generator = new YearMedalsGenerator();

            var model = generator.GetModel(year);

            return View(model);
        }

        public ActionResult CountryMedals()
        {
            var rg = new MedalTableReportGenerator();

            var results = rg.GetItemsForLatest();

            return View(results);
        }

        public ActionResult GameMedals(string gameCode)
        {
            // If someone puts in a 4 letter code, reduce it to a 2 (note - some games now 3-letters)
            if (gameCode.Length >= 4) gameCode.Substring(0, gameCode.Length - 2);

            try
            {
                var generator = new GameMedalsGenerator();

                var model = generator.GetModel(gameCode);

                return View(model);
            }
            catch (ArgumentException ex)
            {
                return View("Error", (object)ex.Message);
            }
        }

        public ActionResult PentamindStandings(int? year, DateTime? date, bool header=false, int count=100)
        {
            var model = GetPentamindStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = (x => true);
            model.HeaderRequired = header;

            return View("PentamindStandings", model);
        }

        public ActionResult ClearPentamindStandingsCache()
        {
            System.Web.HttpContext.Current.Application["Pentamind"] = null;

            return new RedirectResult("/");
        }

        public ActionResult RecalculateSeedings()
        {
            var calculator = new SeedingScoreCalculator();
            calculator.CalculateSeedings();

            return new RedirectResult("/");
        }


        public ActionResult WomensPentamindStandings(int? year, DateTime? date, bool header = false, int count = 40)
        {
            var model = GetPentamindStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = (x => x.IsInWomensPenta);
            model.HeaderRequired = header;

            return View("PentamindStandings", model);
        }

        public ActionResult SeniorPentamindStandings(int? year, DateTime? date, bool header = false, int count = 40)
        {
            var model = GetPentamindStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = x => x.IsSenior;
            model.HeaderRequired = header;

            return View("PentamindStandings", model);
        }

        public ActionResult JuniorPentamindStandings(int? year, DateTime? date, bool header = false, int count = 40)
        {
            var model = GetPentamindStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = x => x.IsJunior;
            model.HeaderRequired = header;

            return View("PentamindStandings", model);
        }

        public PentamindStandingsGenerator.PentamindStandingsReportVm GetPentamindStandings(int? year, DateTime? date)
        {
            PentamindStandingsGenerator.PentamindStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["Pentamind"] != null)
            {
                model = System.Web.HttpContext.Current.Application["Pentamind"] as PentamindStandingsGenerator.PentamindStandingsReportVm;
            }
            else
            {
                var generator = new PentamindStandingsGenerator();
                model = generator.GetStandings(year, date);
                System.Web.HttpContext.Current.Application["Pentamind"] = model;
            }
            return model;
        }

        public ActionResult EurogamesStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetEuroStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("EurogamesStandings", model);
        }

        public ActionResult ModernAbstractStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetModernAbstractStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("ModernAbstractStandings", model);
        }

        public ActionResult PokerStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetPokerStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("PokerStandings", model);
        }

        public ActionResult ChessStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetChessStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("ChessStandings", model);
        }

        public ActionResult BackgammonStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetBackgammonStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("BackgammonStandings", model);
        }

        public ActionResult TotalEventEntries(int? year)
        {
            var generator = new TotalEventEntriesGenerator();

            var model = generator.GetModel(year);

            return View(model);
        }

        public ActionResult EventResultsIndex()
        {
            var generator = new EventResultsGenerator();

            var model = generator.GetEventsIndex();

            return View(model);
        }

        public ActionResult EventEntries(string eventCode)
        {
            var generator = new EventResultsGenerator();

            var model = generator.GetEntrantsModel(eventCode);

            return View(model);
        }

        public ActionResult EventResults(int? year, string eventCode)
        {
            var generator = new EventResultsGenerator();

            var model = generator.GetModel(year, eventCode);

            return View(model);
        }

        /* For evaluation only
        public ActionResult PentamindStandings4Cats(int? year)
        {
            var generator = new PentamindStandings4CatsGenerator();

            var model = generator.GetStandings(year);

            return View(model);
        }*/
    }
}
