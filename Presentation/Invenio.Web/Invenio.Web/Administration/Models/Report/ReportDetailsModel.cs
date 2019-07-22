using System;
using Invenio.Core.Domain.Criterias;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ReportDetailsModel : BaseNopModel
    {
        public int CriteriaId { get; set; }

        public int Quantity { get; set; }

        public CriteriaType CriteriaType { get; set; }

        public DateTime? DateOfInspection { get; set; }

        public string ChargeNumber { get; set; }

        public string DeliveryNumber { get; set; }
    }
}