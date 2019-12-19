using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Reports;
using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;

namespace Invenio.Admin.Models.Report
{
    public class ReportDetailsModel : BaseNopModel
    {
        public ReportDetailsModel()
        {
            Attributes = new List<ReportOrderAttribute>();
            AttrsKey = new List<int>();
        }

        public int CriteriaId { get; set; }

        public int Quantity { get; set; }

        public CriteriaType CriteriaType { get; set; }

        public DateTime? DateOfInspection { get; set; }

        public ICollection<ReportOrderAttribute> Attributes { get; set; }

        public IEnumerable<int> AttrsKey { get; set; }

        //public string ChargeNumber { get; set; }

        //public string DeliveryNumber { get; set; }
    }
}