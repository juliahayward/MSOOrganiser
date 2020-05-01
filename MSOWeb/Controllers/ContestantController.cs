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

    }
}
