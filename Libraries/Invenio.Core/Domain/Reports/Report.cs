using System;
using Invenio.Core.Domain.ChargeNumbers;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.DeliveryNumbers;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Parts;
using Invenio.Core.Domain.Users;

namespace Invenio.Core.Domain.Reports
{
    public class Report : BaseEntity
    {
        public long OkPartsQuantity { get; set; }
        public long NokPartsQuantity { get; set; }
        public long ReworkPartsQuantity { get; set; }

        public bool Approved { get; set; }

        public WorkShift WorkShift { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? DateOfInspection { get; set; }
        public DateTime? ApprovedOn { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int? PartId { get; set; }
        public Part Part { get; set; }

        public int? ChargeNumberId { get; set; }
        public ChargeNumber ChargeNumber { get; set; }

        public int? DeliveryNumberId { get; set; }
        public DeliveryNumber DeliveryNumber { get; set; }
    }
}
