using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Invenio.Core.Domain.Manufacturers;

namespace Invenio.Data.Mapping.Manufacturers
{
    public class ManufacturerMap : NopEntityTypeConfiguration<Manufacturer>
    {
        public ManufacturerMap()
        {
            ToTable("Manufacturer");
            this.HasKey(m => m.Id);
            this.Property(x => x.Name).HasMaxLength(100);
            this.Property(x => x.Comment).HasMaxLength(250);
        }
    }
}
