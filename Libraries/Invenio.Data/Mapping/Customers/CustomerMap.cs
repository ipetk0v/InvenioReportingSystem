using Invenio.Core.Domain.Customers;

namespace Invenio.Data.Mapping.Customers
{
    public class CustomerMap : NopEntityTypeConfiguration<Customer>
    {
        public CustomerMap()
        {
            ToTable("Customer");
            this.HasKey(m => m.Id);
            this.Property(x => x.Name).HasMaxLength(100);
            this.Property(x => x.Comment).HasMaxLength(250);
        }
    }
}
