using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;

namespace MSOWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SecurityFilter());
        }
    }

    public class SecurityFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            HttpCookie authCookie =
              filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket =
                       FormsAuthentication.Decrypt(authCookie.Value);
                var identity = new GenericIdentity(authTicket.Name, "Forms");
                var principal = new GenericPrincipal(identity, new string[] { authTicket.UserData });
                filterContext.HttpContext.User = principal;
            }

            var isAccessAllowed = IsAccessAllowed(filterContext);
            if (!isAccessAllowed)
            {
                FormsAuthentication.RedirectToLoginPage();
            }
        }

        public static bool IsAccessAllowed(AuthorizationContext filterContext)
        {
            var controller = filterContext.ActionDescriptor.ControllerDescriptor;
            var action = filterContext.ActionDescriptor;
            var user = filterContext.HttpContext.User;
            var ip = filterContext.HttpContext.Request.UserHostAddress;

            // AllowAnonymous overrides everything else
            if (action.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute))
                return true;
            if (controller.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute))
                return true;

            return false;
        }
    }
}