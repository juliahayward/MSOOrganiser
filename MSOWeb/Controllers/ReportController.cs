using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            // If someone puts in a 4 letter code, reduce it to a 2
            gameCode = gameCode.Substring(0, 2);

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

        public ActionResult PentamindStandings(int? year, DateTime? date, string view="", int count=100)
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
            model.TopNRequired = count;

            return View("PentamindStandings"+view, model);
        }

        public ActionResult ClearPentamindStandingsCache()
        {
            System.Web.HttpContext.Current.Application["Pentamind"] = null;

            return new RedirectResult("/");
        }

        public ActionResult WomensPentamindStandings(int? year, DateTime? date, string view = "", int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetStandings(year, date, true);
            model.TopNRequired = count;

            return View("PentamindStandings" + view, model);
        }

        public ActionResult EurogamesStandings(int? year, string view="", int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetEuroStandings(year);
            model.TopNRequired = count;

            return View("EurogamesStandings"+view, model);
        }

        public ActionResult ModernAbstractStandings(int? year, string view="", int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetModernAbstractStandings(year);
            model.TopNRequired = count;

            return View("ModernAbstractStandings"+view, model);
        }

        public ActionResult PokerStandings(int? year, string view="", int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetPokerStandings(year);
            model.TopNRequired = count;

            return View("PokerStandings"+view, model);
        }

        public ActionResult ChessStandings(int? year, string view = "", int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetChessStandings(year);
            model.TopNRequired = count;

            return View("ChessStandings" + view, model);
        }

        public ActionResult BackgammonStandings(int? year, string view = "", int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetBackgammonStandings(year);
            model.TopNRequired = count;

            return View("BackgammonStandings" + view, model);
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

        // For evaluation only
        public ActionResult PentamindStandings4Cats(int? year)
        {
            var generator = new PentamindStandings4CatsGenerator();

            var model = generator.GetStandings(year);

            return View(model);
        }
    }
}
