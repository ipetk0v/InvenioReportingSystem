using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Home
{
    public partial class DashboardModel : BaseNopModel
    {
        public bool IsLoggedInAsVendor { get; set; }
    }
}