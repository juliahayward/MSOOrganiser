using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Controllers
{
    public class OlympiadController : Controller
    {
        //
        // GET: /Olympiad/

        public ActionResult Index()
        {
            // returns a list of them
            return View();
        }

        [HttpGet]
        public ActionResult Olympiad(int id)
        {
            // get the page
            return View();
        }

        [HttpPost]
        public ActionResult Olympiad(/* form*/)
        {
            // post to update
            return new EmptyResult();
        }

    }
}
