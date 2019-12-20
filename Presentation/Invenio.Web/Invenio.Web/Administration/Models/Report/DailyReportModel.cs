using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;

namespace Invenio.Admin.Models.Report
{
    public class DailyReportModel : BaseNopEntityModel
    {
        public DailyReportModel()
        {
            AttributeValueIds = new List<int>();
        }

        public DateTime? DateOfInspection { get; set; }
        public string AttrsKey { get; set; }
        public Dictionary<int, string> Attributes { get; set; }
        public ICollection<int> AttributeValueIds { get; set; }

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
        public int Dod9 { get; set; }
        public int Dod10 { get; set; }
        public int Dod11 { get; set; }
        public int Dod12 { get; set; }
        public int Dod13 { get; set; }
        public int Dod14 { get; set; }
        public int Dod15 { get; set; }
        public int Dod16 { get; set; }
        public int Dod17 { get; set; }
        public int Dod18 { get; set; }
        public int Dod19 { get; set; }
        public int Dod20 { get; set; }

        public int Dor1 { get; set; }
        public int Dor2 { get; set; }
        public int Dor3 { get; set; }
        public int Dor4 { get; set; }
        public int Dor5 { get; set; }
        public int Dor6 { get; set; }
        public int Dor7 { get; set; }
        public int Dor8 { get; set; }
        public int Dor9 { get; set; }
        public int Dor { get; set; }
        public int Dor10 { get; set; }
        public int Dor11 { get; set; }
        public int Dor12 { get; set; }
        public int Dor13 { get; set; }
        public int Dor14 { get; set; }
        public int Dor15 { get; set; }
        public int Dor16 { get; set; }
        public int Dor17 { get; set; }
        public int Dor18 { get; set; }
        public int Dor19 { get; set; }
        public int Dor20 { get; set; }
    }
}