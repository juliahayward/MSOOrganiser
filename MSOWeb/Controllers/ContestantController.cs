using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSOCore.ApiLogic;

namespace MSOWeb.Controllers
{
    [Authorize(Roles = "Superadmin, Admin")]
    public class ContestantController : Controller
    {
        [HttpGet]
        public ActionResult Contestant(int id, bool editable = false)
        {
            var logic = new ContestantsLogic();
            var model = logic.GetContestant(id);
            model.IsEditable = editable;

            return View(model);
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult Contestant(ContestantForm form)
        {
            var model = ParseModelFromForm(form);

            try
            {
                var logic = new ContestantsLogic();
                logic.UpdateContestant(model);
                TempData["SuccessMessage"] = "Contestant updated";
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return new RedirectResult("/Contestant/Contestant/" + form.Id + "?editable=true");
        }

        private ContestantsLogic.ContestantVm ParseModelFromForm(ContestantForm form)
        {
            var model = new ContestantsLogic.ContestantVm()
            {
                ContestantId = form.Id,
                Title = form.Title,
                Firstname = form.Firstname,
                Initials = form.Initials,
                Lastname = form.Lastname,
                IsMale = (form.Gender == "Male"),
                Nationality = form.Nationality,
                OnlineNicknames = form.OnlineNicknames
            };
            return model;
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpGet]
        public ActionResult ContestantForName(string name)
        {
            var logic = new ContestantsLogic();
            var model = logic.GetContestantsForName(name);

            if (model.Count() > 20) return new HttpStatusCodeResult(400, "Too many results: please be more specific");

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Superadmin, Admin")]
        [HttpPost]
        public ActionResult AddContestantToEvent(int contestantId, int eventId)
        {
            var logic = new ContestantsLogic();

            // TODO - verify event is in current olympiad;
            logic.AddContestantToEvent(contestantId, eventId);
            // todo - refresh page
            
            return Json("done");
        }
    }

    public class ContestantForm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Firstname { get; set; }
        public string Initials { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string OnlineNicknames { get; set; }
    }

}
