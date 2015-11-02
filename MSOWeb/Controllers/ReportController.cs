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

        public ActionResult IndividualMedals()
        {
            var generator = new IndividualMedalTableGenerator();

            var model = generator.GetItems();

            return View(model);
        }

        public ActionResult ContestantMedals(int contestantId)
        {
            var generator = new ContestantMedalsGenerator();

            var model = generator.GetModel(contestantId);

            return View(model);
        }
    }
}
