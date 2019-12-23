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

        [AllowAnonymous]
        public ActionResult NewPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult NewPasswordRequest(string userName)
        {
            _userLogic.SendUserPasswordResetLink(userName, 
                HttpContext.Request.Url.ToString().Replace("NewPasswordRequest", "PasswordReset"));

            return RedirectToAction("NewPasswordSent");
        }

        [AllowAnonymous]
        public ActionResult NewPasswordSent()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult PasswordReset(int userId, string token)
        {
            return View(new PasswordResetVM {
                UserId = userId,
                Token = token.Replace(" ", "+") // because + gets filtered out in the GET URL
            });
        }

        public class PasswordResetVM {
            public int UserId { get; set; }
            public string Token { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult PasswordReset(int userId, string token, string password)
        {
            try
            { 
                _userLogic.UpdateUserPassword(userId, token, password);
                TempData.Add("success", "Your password has been updated.");
            }
            catch (Exception ex)
            {
                TempData.Add("error", ex.Message);
            }

            return RedirectToAction("Login", "Home");
        }
    }
}
