using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Manufacturers;
using System;

namespace Invenio.Core.Domain.Customers
{
    public class Customer : BaseEntity
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

        public int? ManufacturerId { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
