using System;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Data;
using Invenio.Core.Infrastructure;
using Invenio.Services.Users;

namespace Invenio.Web.Framework
{
    public class StoreIpAddressAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();

            //update IP address
            string currentIpAddress = webHelper.GetCurrentIpAddress();
            if (!String.IsNullOrEmpty(currentIpAddress))
            {
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                var User = workContext.CurrentUser;
                if (!currentIpAddress.Equals(User.LastIpAddress, StringComparison.InvariantCultureIgnoreCase))
                {
                    var UserService = EngineContext.Current.Resolve<IUserService>();
                    User.LastIpAddress = currentIpAddress;
                    UserService.UpdateUser(User);
                }
            }
        }
    }
}
