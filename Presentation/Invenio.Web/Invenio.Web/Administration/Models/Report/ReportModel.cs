using Invenio.Web.Framework.Mvc;
using System;

namespace Invenio.Admin.Models.Report
{
    public class ReportModel : BaseNopEntityModel
    {
        public long CheckedPartsQuantity { get; set; }
        public long OkPartsQuantity { get; set; }
        public long NokPartsQuantity { get; set; }
        public long ReworkPartsQuantity { get; set; }

        public bool Approved { get; set; }
        public bool Deleted { get; set; }
        public string UserName { get; set; }
        public string OrderNumber { get; set; }
        public string SupplierName { get; set; }

        public string WorkShiftName { get; set; }
        public int WorkShift { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime ApprovedOn { get; set; }
        public DateTime DateOfInspection { get; set; }

        public int UserId { get; set; }
        public int OrderId { get; set; }
    }
}