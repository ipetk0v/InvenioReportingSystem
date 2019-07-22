using System;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Reports;
using Invenio.Core.Domain.Users;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ReportModel : BaseNopEntityModel
    {
        public long OkPartsQuantity { get; set; }
        public long NokPartsQuantity { get; set; }
        public long ReworkPartsQuantity { get; set; }

        public bool Approved { get; set; }
        public bool Deleted { get; set; }
        public string UserName { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }

        public string WorkShiftName { get; set; }
        //public WorkShift WorkShift { get; set; }
        public int WorkShift { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime ApprovedOn { get; set; }
        public DateTime DateOfInspection { get; set; }

        public int UserId { get; set; }
        //public User User { get; set; }

        public int OrderId { get; set; }
        //public Order Order { get; set; }
    }
}