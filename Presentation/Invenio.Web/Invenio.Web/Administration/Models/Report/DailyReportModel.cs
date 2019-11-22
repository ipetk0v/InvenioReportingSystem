using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;

namespace Invenio.Admin.Models.Report
{
    public class DailyReportModel : BaseNopEntityModel
    {
        public DateTime? DateOfInspection { get; set; }
        public string AttrsKey { get; set; }
        public Dictionary<int, string> Attributes { get; set; }

        public long Quantity { get; set; }

        public long FirstRunOkParts { get; set; }

        public long BlockedParts { get; set; }

        public long NokParts { get; set; }

        public decimal NokPercentage { get; set; }

        public long ReworkedParts { get; set; }

        public decimal ReworkedPercentage { get; set; }

        public int Dod1 { get; set; }
        public int Dod2 { get; set; }
        public int Dod3 { get; set; }
        public int Dod4 { get; set; }
        public int Dod5 { get; set; }
        public int Dod6 { get; set; }
        public int Dod7 { get; set; }
        public int Dod8 { get; set; }

        public int Dor1 { get; set; }
        public int Dor2 { get; set; }
        public int Dor3 { get; set; }
        public int Dor4 { get; set; }
    }
}