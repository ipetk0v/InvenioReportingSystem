using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ReportTotalModel : BaseNopModel
    {
        public long TotalChecked { get; set; }
        public long TotalOk { get; set; }
        public long TotalBlocked { get; set; }
        public long TotalReworked { get; set; }
        public long TotalNok { get; set; }
        public decimal NokPercentage { get; set; }
    }
}