using Invenio.Core.Domain.Orders;

namespace Invenio.Data.Mapping.Orders
{
    public class OrderMap : NopEntityTypeConfiguration<Order>
    {
        public OrderMap()
        {
            ToTable("Order");
            HasKey(o => o.Id);
            this.Property(x => x.Name).HasMaxLength(100);
            this.Property(x => x.Number).HasMaxLength(200);

            this.HasRequired(o => o.Supplier)
                .WithMany()
                .HasForeignKey(o => o.SupplierId);
        }
    }
}
