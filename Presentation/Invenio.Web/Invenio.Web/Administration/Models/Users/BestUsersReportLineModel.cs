using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public partial class BestUsersReportLineModel : BaseNopModel
    {
        public int UserId { get; set; }
        [NopResourceDisplayName("Admin.Users.Reports.BestBy.Fields.User")]
        public string UserName { get; set; }

        [NopResourceDisplayName("Admin.Users.Reports.BestBy.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [NopResourceDisplayName("Admin.Users.Reports.BestBy.Fields.OrderCount")]
        public decimal OrderCount { get; set; }
    }
}