using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Customers;
using System;

namespace Invenio.Core.Domain.Suppliers
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool Deleted { get; set; }
        public bool Published { get; set; }

        public int? AddressId { get; set; }
        public virtual Address Address { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
