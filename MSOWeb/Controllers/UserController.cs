using MSOCore;
using MSOCore.ApiLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Controllers
{
    public class UserController : Controller
    {
        private UserLogic _userLogic;

        public UserController()
        {
            _userLogic = new UserLogic();
        }

        public ActionResult NewPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewPasswordRequest(string userName)
        {
            _userLogic.SendUserPasswordResetLink(userName, 
                HttpContext.Request.Url.ToString().Replace("NewPasswordRequest", "PasswordReset"));

            return RedirectToAction("NewPasswordSent");
        }

        public ActionResult NewPasswordSent()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PasswordReset(int userId, string token)
        {
            return View(new PasswordResetVM { UserId = userId, Token = token});
        }

        public class PasswordResetVM {
            public int UserId { get; set; }
            public string Token { get; set; }
        }

        [HttpPost]
        public ActionResult PasswordReset(int userId, string token, string password)
        {
            _userLogic.UpdateUserPassword(userId, token, password);

            TempData.Add("success", "Your password has been updated.");

            return RedirectToAction("Login", "Home");
        }
    }
}
