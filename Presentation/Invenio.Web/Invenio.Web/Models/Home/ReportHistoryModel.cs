using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Invenio.Web.Models.Home
{
    public class ReportHistoryModel : BaseNopModel
    {
        public ReportHistoryModel()
        {
            ReportHistoryList = new List<HistoryModel>();
        }

        public IList<HistoryModel> ReportHistoryList { get; set; }
    }

    public class HistoryModel : BaseNopModel
    {
        public string WorkShift { get; set; }
        public string DateOfInspection { get; set; }
        public string Customer { get; set; }
        public string PartName { get; set; }
        public string Order { get; set; }
        public string DeliveryNumber { get; set; }
        public string ChargeNumber { get; set; }
        public long OkQuantity { get; set; }
        public long NokQuantity { get; set; }
        public long ReworkedQuantity { get; set; }
        public bool IsApproved { get; set; }
    }
}