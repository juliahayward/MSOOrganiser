using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSOCore.ApiLogic;

namespace MSOWeb.Controllers
{
    [Authorize(Roles="Superadmin, Admin")]
    public class OlympiadController : Controller
    {
        public ActionResult Index()
        {
            var logic = new OlympiadsLogic();
            var model = logic.GetOlympiads();

            return View(model);
        }

        [HttpGet]
        public ActionResult Olympiad(int id)
        {
            var logic = new OlympiadsLogic();
            var model = logic.GetOlympiad(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Olympiad(OlympiadForm form)
        {
            // post to update
            TempData["SuccessMessage"] = "Saved";

            return new RedirectResult("/Olympiad?id=" + form.Id);
        }

        [HttpGet]
        public ActionResult Event(int id, bool editable = false)
        {
            var logic = new OlympiadsLogic();
            var model = logic.GetEvent(id);
            model.Editable = editable;

            return View(model);
        }

        [HttpPost]
        public ActionResult Event(EventForm form)
        {
            // post to update
            TempData["SuccessMessage"] = "Saved";

            return new RedirectResult("/Olympiad/Event/" + form.Id + "?editable=true");
        }
    }

    public class OlympiadForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EventForm
    {
        public int Id { get; set; }
    }
}
