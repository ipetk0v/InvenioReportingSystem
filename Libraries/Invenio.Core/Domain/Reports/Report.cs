using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Users;
using System;
using System.Collections.Generic;

namespace Invenio.Core.Domain.Reports
{
    public class Report : BaseEntity
    {
        private ICollection<ReportOrderAttribute> _reportOrderAttributes;

        public long CheckedPartsQuantity { get; set; }
        public long OkPartsQuantity { get; set; }
        public long NokPartsQuantity { get; set; }
        public long ReworkPartsQuantity { get; set; }

        public bool Approved { get; set; }

        public WorkShift WorkShift { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? DateOfInspection { get; set; }
        public DateTime? ApprovedOn { get; set; }

        public int? Time { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public virtual ICollection<ReportOrderAttribute> ReportOrderAttributes
        {
            get { return _reportOrderAttributes ?? (_reportOrderAttributes = new List<ReportOrderAttribute>()); }
            protected set { _reportOrderAttributes = value; }
        }
    }
}
