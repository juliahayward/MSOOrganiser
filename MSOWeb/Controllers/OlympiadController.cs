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
            // TODO - valiudate
            // TODO - save
            // TODO - caluclate ranks and pentamind points
            // TODO - events with partners
            TempData["SuccessMessage"] = "Received";

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

        public IEnumerable<string> EntrantIdString { get; set; }

        public IEnumerable<string> Medal { get; set; }

        public IEnumerable<string> JuniorMedal { get; set; }

        public IEnumerable<string> Score { get; set; }

        public IEnumerable<string> Tiebreak { get; set; }

        public IEnumerable<string> AbsentString { get; set; }

        public EventForm()
        {
            Medal = new List<string>();
            JuniorMedal = new List<string>();
            Score = new List<string>();
            Tiebreak = new List<string>();
            AbsentString = new List<string>();
        }

        public IEnumerable<bool> Absent
        {
            get
            {
                // A workround for checkboxes only submitting "on", never "off" - there's a hidden field in 
                // front that submits "off" which we delete if we pick up the "on"
                List<bool> result = new List<bool>();
                foreach (var s in AbsentString)
                {
                    if (s == "off")
                        result.Add(false);
                    else if (s == "on")
                    {
                        result.RemoveAt(result.LastIndexOf(false));
                        result.Add(true);
                    }
                    else
                        throw new ArgumentException("Invalid SELECT value: " + s);
                }
                return result;
            }
        }

        public IEnumerable<int> EntrantId
        {
            get
            {
                return EntrantIdString.Select(x => int.Parse(x));
            }
        }

    }
}
