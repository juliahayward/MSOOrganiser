using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSOCore;
using MSOCore.ApiLogic;
using MSOCore.Calculators;

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

        [HttpGet]
        public ActionResult CurrentOlympiad()
        {
            var logic = new OlympiadsLogic();
            try
            {
                var id = logic.GetCurrentOlympiadId();
                return RedirectToAction("Olympiad", new { id = id });
            }
            catch (NoCurrentOlympiadException e)
            {
                TempData["FailureMessage"] = "There is no current Olympiad to edit";
                return RedirectToAction("Index");
            }
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
            var model = ParseModelFromForm(form);

            try
            {
                model.Validate();
                var logic = new OlympiadsLogic();
                logic.UpdateEvent(model);
                TempData["SuccessMessage"] = "Event updated";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return new RedirectResult("/Olympiad/Event/" + form.Id + "?editable=true");
        }

        [HttpPost]
        public ActionResult FreezeEvent(int eventId)
        {
            var logic = new OlympiadsLogic();
            try
            {
                logic.FreezeEvent(eventId);
                TempData["SuccessMessage"] = "Standings generated";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return Json(new { eventId = eventId });
        }

        [HttpPost]
        public ActionResult DeleteEntrant(int entrantId)
        {
            var logic = new OlympiadsLogic();
            try
            {
                logic.DeleteEntrant(entrantId);
                TempData["SuccessMessage"] = "Deleted";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return Json(new { entrantId = entrantId });
        }


        /// <summary>
        /// Turn the Form back into a state that's easy to update the database
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private OlympiadsLogic.UpdateEventModel ParseModelFromForm(EventForm form)
        {
            var model = new OlympiadsLogic.UpdateEventModel();
            model.EventId = form.Id;
            model.Entrants = new List<OlympiadsLogic.UpdateEventModel.EntrantVm>();
            var ie = form.EntryNumber.GetEnumerator();
            var ae = form.Absent.GetEnumerator();
            var se = form.Score.GetEnumerator();
            var te = form.Tiebreak.GetEnumerator();
            var me = form.Medal.GetEnumerator();
            var jme = form.JuniorMedal.GetEnumerator();

            while (ie.MoveNext())
            {
                ae.MoveNext();
                se.MoveNext();
                te.MoveNext();
                me.MoveNext();
                jme.MoveNext();

                model.Entrants.Add(new OlympiadsLogic.UpdateEventModel.EntrantVm()
                {
                    EntryNumber = ie.Current,
                    Absent = ae.Current,
                    Score = se.Current,
                    Tiebreak = te.Current,
                    Medal = me.Current,
                    JuniorMedal = jme.Current
                });
            };

            return model;
        }
    }

    public class OlympiadForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents what's posted from the Event
    /// </summary>
    public class EventForm
    {
        public int Id { get; set; }

        public IEnumerable<string> EntryNumberString { get; set; }

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

        public IEnumerable<int> EntryNumber
        {
            get
            {
                return EntryNumberString.Select(x => int.Parse(x));
            }
        }

    }
}
