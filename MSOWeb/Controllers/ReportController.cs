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

        public ActionResult GrandPrixStandings(int? year, DateTime? date, bool header = false, int count = 100)
        {
            var model = GetGrandPrixStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = (x => true);
            model.HeaderRequired = header;

            return View("GrandPrixStandings", model);
        }

        public ActionResult ClearPentamindStandingsCache()
        {
            System.Web.HttpContext.Current.Application["Pentamind"] = null;
            System.Web.HttpContext.Current.Application["Eurogames"] = null;
            System.Web.HttpContext.Current.Application["ModernAbstract"] = null;
            System.Web.HttpContext.Current.Application["Poker"] = null;
            System.Web.HttpContext.Current.Application["Chess"] = null;
            System.Web.HttpContext.Current.Application["Backgammon"] = null;
            System.Web.HttpContext.Current.Application["GrandPrix"] = null;
            // GP categories
            System.Web.HttpContext.Current.Application["imperfectinfo"] = null;
            System.Web.HttpContext.Current.Application["abstract"] = null;
            System.Web.HttpContext.Current.Application["backgammon"] = null;
            System.Web.HttpContext.Current.Application["chess"] = null;
            System.Web.HttpContext.Current.Application["poker"] = null;
            System.Web.HttpContext.Current.Application["draughts"] = null;
            System.Web.HttpContext.Current.Application["multiplayer"] = null;
            return new RedirectResult("/");
        }

        public ActionResult RecalculateSeedings()
        {
            var calculator = new SeedingScoreCalculator();
            calculator.CalculateSeedings();

            return new RedirectResult("/");
        }

        public ActionResult RecalculateElos()
        {
            var calculator = new SeedingScoreCalculator();
            calculator.CalculateRatings();

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


        public ActionResult WomensGrandPrixStandings(int? year, DateTime? date, bool header = false, int count = 40)
        {
            var model = GetGrandPrixStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = (x => x.IsInWomensPenta);
            model.HeaderRequired = header;

            return View("GrandPrixStandings", model);
        }

        public ActionResult SeniorGrandPrixStandings(int? year, DateTime? date, bool header = false, int count = 40)
        {
            var model = GetGrandPrixStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = x => x.IsSenior;
            model.HeaderRequired = header;

            return View("GrandPrixStandings", model);
        }

        public ActionResult JuniorGrandPrixStandings(int? year, DateTime? date, bool header = false, int count = 40)
        {
            var model = GetGrandPrixStandings(year, date);
            model.TopNRequired = count;
            model.StandingsFilter = x => x.IsJunior;
            model.HeaderRequired = header;

            return View("GrandPrixStandings", model);
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

        public GrandPrixStandingsGenerator.GrandPrixStandingsReportVm GetGrandPrixStandings(int? year, DateTime? date)
        {
            GrandPrixStandingsGenerator.GrandPrixStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["GrandPrix"] != null)
            {
                model = System.Web.HttpContext.Current.Application["GrandPrix"] as GrandPrixStandingsGenerator.GrandPrixStandingsReportVm;
            }
            else
            {
                var generator = new GrandPrixStandingsGenerator();
                model = generator.GetStandings(year, date);
                System.Web.HttpContext.Current.Application["GrandPrix"] = model;
            }
            return model;
        }

        public ActionResult EurogamesStandings(int? year, bool header = false, int count = 40)
        {
            var model = GetEurogameStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("EurogamesStandings", model);
        }

        public PentamindStandingsGenerator.PentamindStandingsReportVm GetEurogameStandings(int? year)
        {
            PentamindStandingsGenerator.PentamindStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["Eurogames"] != null)
            {
                model = System.Web.HttpContext.Current.Application["Eurogames"] as PentamindStandingsGenerator.PentamindStandingsReportVm;
            }
            else
            {
                var generator = new PentamindStandingsGenerator();
                model = generator.GetEuroStandings(year);
                System.Web.HttpContext.Current.Application["Eurogames"] = model;
            }
            return model;
        }

        public ActionResult ModernAbstractStandings(int? year, bool header = false, int count = 40)
        {
            var model = GetModernAbstractStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("ModernAbstractStandings", model);
        }

        public PentamindStandingsGenerator.PentamindStandingsReportVm GetModernAbstractStandings(int? year)
        {
            PentamindStandingsGenerator.PentamindStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["ModernAbstract"] != null)
            {
                model = System.Web.HttpContext.Current.Application["ModernAbstract"] as PentamindStandingsGenerator.PentamindStandingsReportVm;
            }
            else
            {
                var generator = new PentamindStandingsGenerator();
                model = generator.GetModernAbstractStandings(year);
                System.Web.HttpContext.Current.Application["ModernAbstract"] = model;
            }
            return model;
        }

        public ActionResult PokerStandings(int? year, bool header = false, int count = 40)
        {
            var model = GetPokerStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("PokerStandings", model);
        }

        public PentamindStandingsGenerator.PentamindStandingsReportVm GetPokerStandings(int? year)
        {
            PentamindStandingsGenerator.PentamindStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["Poker"] != null)
            {
                model = System.Web.HttpContext.Current.Application["Poker"] as PentamindStandingsGenerator.PentamindStandingsReportVm;
            }
            else
            {
                var generator = new PentamindStandingsGenerator();
                model = generator.GetPokerStandings(year);
                System.Web.HttpContext.Current.Application["Poker"] = model;
            }
            return model;
        }

        public ActionResult ChessStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetChessStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("ChessStandings", model);
        }

        public PentamindStandingsGenerator.PentamindStandingsReportVm GetChessStandings(int? year)
        {
            PentamindStandingsGenerator.PentamindStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["Chess"] != null)
            {
                model = System.Web.HttpContext.Current.Application["Chess"] as PentamindStandingsGenerator.PentamindStandingsReportVm;
            }
            else
            {
                var generator = new PentamindStandingsGenerator();
                model = generator.GetChessStandings(year);
                System.Web.HttpContext.Current.Application["Chess"] = model;
            }
            return model;
        }

        public ActionResult BackgammonStandings(int? year, bool header = false, int count = 40)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetBackgammonStandings(year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("BackgammonStandings", model);
        }

        public PentamindStandingsGenerator.PentamindStandingsReportVm GetBackgammonStandings(int? year)
        {
            PentamindStandingsGenerator.PentamindStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application["Backgammon"] != null)
            {
                model = System.Web.HttpContext.Current.Application["Backgammon"] as PentamindStandingsGenerator.PentamindStandingsReportVm;
            }
            else
            {
                var generator = new PentamindStandingsGenerator();
                model = generator.GetBackgammonStandings(year);
                System.Web.HttpContext.Current.Application["Backgammon"] = model;
            }
            return model;
        }

        public ActionResult GPCategoryStandings(string category, int? year, bool header = false, int count = 40)
        {
            var model = GetGPCategoryStandings(category, year);
            model.TopNRequired = count;
            model.HeaderRequired = header;

            return View("GrandPrixStandings", model);
        }

        public GrandPrixStandingsGenerator.GrandPrixStandingsReportVm GetGPCategoryStandings(string category, int? year)
        {
            GrandPrixStandingsGenerator.GrandPrixStandingsReportVm model;
            if (System.Web.HttpContext.Current.Application[category] != null)
            {
                model = System.Web.HttpContext.Current.Application[category] as GrandPrixStandingsGenerator.GrandPrixStandingsReportVm;
            }
            else
            {
                var generator = new GrandPrixStandingsGenerator();
                model = generator.GetGPCategoryStandings(category, year);
                System.Web.HttpContext.Current.Application[category] = model;
            }
            return model;
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

        
        public ActionResult FreezeMetaEvents()
        {
            var freezer = new MetaEventFreezer();

            freezer.FreezeMetaEvents();

            return new RedirectResult("/");
        }
    }
}
