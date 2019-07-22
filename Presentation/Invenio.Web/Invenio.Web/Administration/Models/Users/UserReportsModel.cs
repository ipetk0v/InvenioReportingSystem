using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public partial class UserReportsModel : BaseNopModel
    {
        public BestUsersReportModel BestUsersByOrderTotal { get; set; }
        public BestUsersReportModel BestUsersByNumberOfOrders { get; set; }
    }
}