using System;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Data;
using Invenio.Core.Infrastructure;
using Invenio.Services.Users;

namespace Invenio.Web.Framework
{
    /// <summary>
    /// Represents filter attribute to validate User password expiration
    /// </summary>
    public class ValidatePasswordAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            var actionName = filterContext.ActionDescriptor.ActionName;
            if (string.IsNullOrEmpty(actionName) || actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase))
                return;

            var controllerName = filterContext.Controller.ToString();
            if (string.IsNullOrEmpty(controllerName) || controllerName.Equals("User", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //get current User
            var User = EngineContext.Current.Resolve<IWorkContext>().CurrentUser;

            //check password expiration
            if (User.PasswordIsExpired())
            {
                var changePasswordUrl = new UrlHelper(filterContext.RequestContext).RouteUrl("UserChangePassword");
                filterContext.Result = new RedirectResult(changePasswordUrl);
            }
        }
    }
}
