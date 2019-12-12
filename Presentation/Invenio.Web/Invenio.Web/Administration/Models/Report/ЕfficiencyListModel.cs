using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ЕfficiencyListModel : BaseNopModel
    {
        public ЕfficiencyListModel()
        {
            OrderBy = new List<SelectListItem>();
        }

        //[NopResourceDisplayName("Admin.Reports.DateFrom")]
        //[UIHint("DateNullable")]
        //public DateTime? DateFrom { get; set; }

        //[NopResourceDisplayName("Admin.Reports.DateTo")]
        //[UIHint("DateNullable")]
        //public DateTime? DateTo { get; set; }

        [NopResourceDisplayName("Account.Fields.EfficiencySearch")]
        public int? EfficiencySearchMonth { get; set; }
        [NopResourceDisplayName("Account.Fields.EfficiencySearch")]
        public int? EfficiencySearchYear { get; set; }

        public DateTime? ParseEfficiencySearch()
        {
            if (!EfficiencySearchMonth.HasValue || !EfficiencySearchYear.HasValue)
                return null;

            DateTime? efficiancyMonth = null;
            try
            {
                efficiancyMonth = new DateTime(EfficiencySearchYear.Value, EfficiencySearchMonth.Value, 1);
            }
            catch { }
            return efficiancyMonth;
        }

        public string Name { get; set; }

        public IList<SelectListItem> OrderBy { get; set; }
        public int OrderById { get; set; }
    }
}