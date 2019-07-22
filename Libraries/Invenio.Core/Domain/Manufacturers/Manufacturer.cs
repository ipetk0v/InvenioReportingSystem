using Invenio.Core.Domain.Directory;
using System;

namespace Invenio.Core.Domain.Manufacturers
{
    public class Manufacturer : BaseEntity
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool Deleted { get; set; }
        public bool Published { get; set; }

        public int? CountryId { get; set; }
        public virtual Country Country { get; set; }

        public int? StateProvinceId { get; set; }
        public virtual StateProvince StateProvince { get; set; }
    }
}
