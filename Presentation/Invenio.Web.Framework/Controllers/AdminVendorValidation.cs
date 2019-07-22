using System;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Data;
using Invenio.Core.Domain.Users;
using Invenio.Core.Infrastructure;

namespace Invenio.Web.Framework.Controllers
{
    /// <summary>
    /// Attribute to ensure that users with "Vendor" User role has appropriate vendor account associated (and active)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class AdminVendorValidation : FilterAttribute, IAuthorizationFilter
    {
        private readonly bool _ignore;

        public AdminVendorValidation(bool ignore = false)
        {
            this._ignore = ignore;
        }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (_ignore)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //var workContext = EngineContext.Current.Resolve<IWorkContext>();
            //if (!workContext.CurrentUser.IsVendor())
            //    return;

            //ensure that this user has active vendor record associated
            //if (workContext.CurrentVendor == null)
            //    filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}
