using Invenio.Core.Domain.Suppliers;
using System;

namespace Invenio.Core.Domain.Orders
{
    public class Order : BaseEntity
    {
        public string Name { get; set; }

        public string Number { get; set; }
        
        public int PartsPerHour { get; set; }

        public int TotalPartsQuantity { get; set; }

        public int CheckedPartsQuantity { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
