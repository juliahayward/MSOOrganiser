using MSOCore.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MSOCore.ApiLogic;
using MSOCore;

namespace MSOWeb.Controllers
{
    // https://www.codeproject.com/Articles/288631/Secure-ASP-NET-MVC-applications
    //[Authorize(Roles="Admin")]
    public class HomeController : Controller
    {
        private UserLogic _userLogic;

        public HomeController()
        {
            _userLogic = new UserLogic();
        }


        public ActionResult Index()
        {
            var generator = new GameListGenerator();

            var model = generator.GetItems();

            return View(model);
        }

        public ActionResult ReleaseNotes()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string username, string password, string ReturnUrl)
        {
            var user = _userLogic.GetUserForLogin(username, password);
            if (user == null)
                return new RedirectResult("/Home/Login");

            string Role = user.Role;

            var authTicket = new FormsAuthenticationTicket(1, username,
                DateTime.Now, DateTime.Now.AddMinutes(30), true,
                Role);
            string cookieContents = FormsAuthentication.Encrypt(authTicket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieContents)
            {
                Expires = authTicket.Expiration,
                Path = FormsAuthentication.FormsCookiePath
            };
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(ReturnUrl))
                Response.Redirect(ReturnUrl);

            return new RedirectResult("/");
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return new RedirectResult("/");
        }

        public ActionResult Downloads()
        {
            var generator = new EventListGenerator();

            var model = generator.GetItems();

            return View(model);
        }

        public class EventEntriesModel
        {
            public Dictionary<string, string> Events;
            public Dictionary<string, int> Entrants;
            public Dictionary<string, string> Games;
        }

        public ActionResult EventEntries()
        {
            // Copied from PrintEventEntriesSummeryReportPrinter, this should really be in Core

            var context = DataEntitiesProvider.Provide();
            var currentOlympiad = context.Olympiad_Infoes.Where(x => x.Current).First();

            var events = context.Events.Where(x => x.OlympiadId == currentOlympiad.Id)
                .ToDictionary(e => e.Code, e => e.Mind_Sport);

            var entrants = context.Entrants.Where(x => x.OlympiadId == currentOlympiad.Id)
                .GroupBy(x => x.Game_Code)
                .ToDictionary(gp => gp.Key, gp => gp.Count());

            var games = context.Games.Where(x => !x.Code.StartsWith("ZZ"))
                .ToDictionary(g => g.Code, g => g.Mind_Sport);

            return View(new EventEntriesModel { Events = events, Entrants = entrants, Games = games });
        }
    }
}
