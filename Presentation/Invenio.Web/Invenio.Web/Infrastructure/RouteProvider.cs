using Invenio.Web.Framework.Localization;
using Invenio.Web.Framework.Mvc.Routes;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Invenio.Web.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(RouteCollection routes)
        {
            //home page
            routes.MapLocalizedRoute("HomePage",
                "",
                new { controller = "Home", action = "Index" },
                new[] { "Invenio.Web.Controllers" });

            //routes.MapLocalizedRoute("UserInfo",
            //    "User/info",
            //    new { controller = "User", action = "Info" },
            //    new[] { "Invenio.Web.Controllers" });


            routes.MapLocalizedRoute("ReportHistory",
                "reporthistory",
                new { controller = "Home", action = "ReportHistory" },
                new[] { "Invenio.Web.Controllers" });

            //login
            routes.MapLocalizedRoute("Login",
                "login/",
                new { controller = "User", action = "Login" },
                new[] { "Invenio.Web.Controllers" });
            ////register
            //routes.MapLocalizedRoute("Register",
            //    "register/",
            //    new { controller = "User", action = "Register" },
            //    new[] { "Invenio.Web.Controllers" });
            //logout
            routes.MapLocalizedRoute("Logout",
                "logout/",
                new { controller = "User", action = "Logout" },
                new[] { "Invenio.Web.Controllers" });

            ////store closed
            //routes.MapLocalizedRoute("StoreClosed",
            //    "storeclosed",
            //    new { controller = "Common", action = "StoreClosed" },
            //    new[] { "Invenio.Web.Controllers" });

            //install
            routes.MapRoute("Installation",
                "install",
                new { controller = "Install", action = "Index" },
                new[] { "Invenio.Web.Controllers" });

            //page not found
            routes.MapLocalizedRoute("PageNotFound",
                "page-not-found",
                new { controller = "Common", action = "PageNotFound" },
                new[] { "Invenio.Web.Controllers" });

            ////passwordrecovery
            //routes.MapLocalizedRoute("PasswordRecovery",
            //    "passwordrecovery",
            //    new { controller = "User", action = "PasswordRecovery" },
            //    new[] { "Invenio.Web.Controllers" });

            ////password recovery confirmation
            //routes.MapLocalizedRoute("PasswordRecoveryConfirm",
            //    "passwordrecovery/confirm",
            //    new { controller = "User", action = "PasswordRecoveryConfirm" },
            //    new[] { "Invenio.Web.Controllers" });
        }
    }
}