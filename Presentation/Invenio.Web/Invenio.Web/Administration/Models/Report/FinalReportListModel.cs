using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class FinalReportListModel : BaseNopEntityModel
    {
        public FinalReportListModel()
        {
        }

        [NopResourceDisplayName("Admin.Reports.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.Reports.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        //[NopResourceDisplayName("Admin.Reports.SearchText")]
        //[AllowHtml]
        //public string SearchText { get; set; }
    }
}