using MSOCore.ApiLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Controllers
{
    public class ApiV1Controller : Controller
    {
        public ActionResult GetOlympiadEvents()
        {
            var l = new OlympiadEventsApiLogic();

            return Json(l.GetOlympiadEvents(), JsonRequestBehavior.AllowGet);
        }

    }
}
