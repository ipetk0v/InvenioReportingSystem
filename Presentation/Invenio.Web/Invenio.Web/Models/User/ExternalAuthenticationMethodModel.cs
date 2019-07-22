using System.Web.Routing;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Web.Models.User
{
    public partial class ExternalAuthenticationMethodModel : BaseNopModel
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }
}