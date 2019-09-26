using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Invenio.Admin.Models.Report
{
    public class ЕfficiencyListModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Reports.DateFrom")]
        [UIHint("DateNullable")]
        public DateTime? DateFrom { get; set; }

        [NopResourceDisplayName("Admin.Reports.DateTo")]
        [UIHint("DateNullable")]
        public DateTime? DateTo { get; set; }
    }
}