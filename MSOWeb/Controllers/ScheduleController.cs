using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSOCore.Reports;

namespace MSOWeb.Controllers
{
    [AllowAnonymous]
    public class ScheduleController : Controller
    {
        public ActionResult Complete()
        {
            var generator = new ScheduleGenerator();

            var model = generator.GetCompleteSchedule();

            return View(model);
        }

        public ActionResult Today()
        {
            var generator = new ScheduleGenerator();

            var model = generator.GetDaySchedule(DateTime.Now.Date);

            return View(model);
        }
    }
}
