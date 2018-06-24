using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/

        public ActionResult Entrants(int year, string eventCode)
        {
            var generator = new EventEntrantsGenerator();

            var model = generator.GetModel(year, eventCode);

            return View(model);
        }

    }
}
