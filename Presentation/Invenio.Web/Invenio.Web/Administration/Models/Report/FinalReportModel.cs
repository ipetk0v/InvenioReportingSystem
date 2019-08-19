using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class FinalReportModel : BaseNopEntityModel
    {
        public string Supplier { get; set; }

        public string PartNumber { get; set; }

        public string PartDescription { get; set; }

        public string OrderNumber { get; set; }

        public int QuantityToCheck { get; set; }

        public string TypeOfReport { get; set; }

        public long TotalChecked { get; set; }

        public long TotalBlocked { get; set; }

        public long TotalReworked { get; set; }

        public long TotalNok { get; set; }

        public long TotalOk { get; set; }

        public decimal NokPercentage { get; set; }

        public long OrderQuantity { get; set; }
    }
}