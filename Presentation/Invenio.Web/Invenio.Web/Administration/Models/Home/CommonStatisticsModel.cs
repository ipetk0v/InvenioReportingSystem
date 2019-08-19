using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Home
{
    public partial class CommonStatisticsModel : BaseNopModel
    {
        public int NumberOfOrders { get; set; }

        public int NumberOfUsers { get; set; }

        public int NumberOfSuppliers { get; set; }

        public int NumberNotApprovedReports { get; set; }
    }
}