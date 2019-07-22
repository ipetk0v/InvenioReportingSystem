using System.Web.Mvc;
using System.Web.Routing;
using Invenio.Core.Infrastructure;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Security;
using Invenio.Web.Framework.Seo;
//using Invenio.Web.Framework.Seo;

namespace Invenio.Web.Controllers
{
    //[CheckAffiliate]
    [StoreClosed]
    [PublicStoreAllowNavigation]
    [LanguageSeoCode]
    [NopHttpsRequirement(SslRequirement.NoMatter)]
    [WwwRequirement]
    public abstract partial class BasePublicController : BaseController
    {
        protected virtual ActionResult InvokeHttp404()
        {
            // Call target Controller and pass the routeData.
            IController errorController = EngineContext.Current.Resolve<CommonController>();

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Common");
            routeData.Values.Add("action", "PageNotFound");

            errorController.Execute(new RequestContext(this.HttpContext, routeData));

            return new EmptyResult();
        }

    }
}
