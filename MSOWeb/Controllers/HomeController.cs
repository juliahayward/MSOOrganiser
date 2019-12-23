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

        [AllowAnonymous]
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

        public ActionResult OrganiserReleaseNotes()
        {
            ViewBag.Layout = "~/Views/Shared/_NewLayoutNoHeader.cshtml";
            return View("ReleaseNotes");
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
            {
                TempData.Add("error", "User not recognised");
                return new RedirectResult("/Home/Login");
            }

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
    }
}
