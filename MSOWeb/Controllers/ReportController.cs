using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/

        public ActionResult Nationality()
        {
            var generator = new NationalityReportGenerator();

            var model = generator.GetItemsForLatest();

            return View(model);
        }

        public ActionResult IndividualMedals(int page = 1)
        {
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

        public ActionResult GameMedals(string gameCode)
        {
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

        public ActionResult PentamindStandings(int? year)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetStandings(year);

            return View(model);
        }

        public ActionResult EurogamesStandings(int? year)
        {
            var generator = new PentamindStandingsGenerator();

            var model = generator.GetEuroStandings(year);

            return View(model);
        }

        public ActionResult EventResults(int? year, string eventCode)
        {
            var generator = new EventResultsGenerator();

            if (!year.HasValue) year = DateTime.Now.Year;
            var model = generator.GetModel(year.Value, eventCode);

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
