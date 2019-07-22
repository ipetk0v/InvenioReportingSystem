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

        [NopResourceDisplayName("Admin.Reports.SearchApproved")]
        public int SearchApprovedId { get; set; }

        public IList<SelectListItem> AvailableApprovedOptions { get; set; }
    }
}