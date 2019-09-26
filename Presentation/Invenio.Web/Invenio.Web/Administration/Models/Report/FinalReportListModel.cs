using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class FinalReportListModel : BaseNopEntityModel
    {
        public FinalReportListModel()
        {
            AvailableSuppliers = new List<SelectListItem>();
            AvailableOrders = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Reports.SupplierId")]
        public int SupplierId { get; set; }

        [NopResourceDisplayName("Admin.Reports.OrderId")]
        public int OrderId { get; set; }

        public IList<SelectListItem> AvailableSuppliers { get; set; }
        public IList<SelectListItem> AvailableOrders { get; set; }
    }
}