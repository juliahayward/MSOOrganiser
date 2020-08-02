using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using System;
using MSOWeb.Filters;

namespace MSOWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SecurityFilter());
            FilterProviders.Providers.Add(new PerformanceTestFilterProvider());
        }
    }

    public class SecurityFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            HttpCookie authCookie =
              filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = null;

            bool isAccessAllowed;
            try
            {
                if (authCookie != null)
                {

                    authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var identity = new GenericIdentity(authTicket.Name, "Forms");
                    var principal = new GenericPrincipal(identity, new string[] { authTicket.UserData });
                    filterContext.HttpContext.User = principal;
                }
                isAccessAllowed = IsAccessAllowed(filterContext, authTicket);
            }
            catch (ArgumentException)
            {
                https://stackoverflow.com/questions/18895746/invalid-value-for-encryptedticket-parameter
                isAccessAllowed = false;
            }

            if (!isAccessAllowed)
            {
                FormsAuthentication.RedirectToLoginPage();
            }
        }

        public static bool IsAccessAllowed(AuthorizationContext filterContext, FormsAuthenticationTicket ticket)
        {
            var controller = filterContext.ActionDescriptor.ControllerDescriptor;
            var action = filterContext.ActionDescriptor;
            var user = filterContext.HttpContext.User;
            var ip = filterContext.HttpContext.Request.UserHostAddress;
            var role = ticket?.UserData ?? "";

            // AllowAnonymous overrides everything else
            if (action.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute))
                return true;
            if (controller.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute))
                return true;

            // The presence of an Authorize attribute lets you in if you're in a specific role...
            var methodAuth = action.GetCustomAttributes(true).FirstOrDefault(x => x is AuthorizeAttribute);
            if (methodAuth != null)
                return (methodAuth as AuthorizeAttribute).Roles.Contains(role);
            var controllerAuth = controller.GetCustomAttributes(true).FirstOrDefault(x => x is AuthorizeAttribute);
            if (controllerAuth != null)
                return (controllerAuth as AuthorizeAttribute).Roles.Contains(role);

            // otherwise, just "are you logged in".
            return (ticket != null);
        }
    }
}