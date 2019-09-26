using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ReportListModel : BaseNopEntityModel
    {
        public ReportListModel()
        {
            AvailableApprovedOptions = new List<SelectListItem>();
            AvailableWorkShifts = new List<SelectListItem>();
            AvailableSuppliers = new List<SelectListItem>();
            AvailableOrders = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Reports.DateOfInspectionFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.Reports.DateOfInspectionTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [NopResourceDisplayName("Admin.Reports.SearchText")]
        [AllowHtml]
        public string SearchText { get; set; }

        [NopResourceDisplayName("Admin.Reports.UserName")]
        [AllowHtml]
        public string UserName { get; set; }

        [NopResourceDisplayName("Admin.Reports.SearchApproved")]
        public int SearchApprovedId { get; set; }

        [NopResourceDisplayName("Admin.Reports.AvailableWorkShiftId")]
        public int WorkShiftId { get; set; }

        [NopResourceDisplayName("Admin.Reports.SupplierId")]
        public int SupplierId { get; set; }

        [NopResourceDisplayName("Admin.Reports.OrderId")]
        public int OrderId { get; set; }

        public IList<SelectListItem> AvailableApprovedOptions { get; set; }
        public IList<SelectListItem> AvailableWorkShifts { get; set; }
        public IList<SelectListItem> AvailableSuppliers { get; set; }
        public IList<SelectListItem> AvailableOrders { get; set; }
    }
}