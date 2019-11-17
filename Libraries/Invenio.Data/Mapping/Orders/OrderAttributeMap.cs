using Invenio.Core.Domain.Orders;

namespace Invenio.Data.Mapping.Orders
{
    public class OrderAttributeMap : NopEntityTypeConfiguration<OrderAttribute>
    {
        public OrderAttributeMap()
        {
            this.ToTable("OrderAttribute");
            this.HasKey(pa => pa.Id);
            this.Property(pa => pa.Name).IsRequired();

            this.HasOptional(oam => oam.ParentOrderAttribute)
                .WithMany()
                .HasForeignKey(oam => oam.ParentOrderAttributeId);
        }
    }
}
