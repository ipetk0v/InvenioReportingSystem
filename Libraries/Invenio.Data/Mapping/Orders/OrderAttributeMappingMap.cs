using Invenio.Core.Domain.Orders;

namespace Invenio.Data.Mapping.Orders
{
    public class OrderAttributeMappingMap : NopEntityTypeConfiguration<OrderAttributeMapping>
    {
        public OrderAttributeMappingMap()
        {
            this.ToTable("Order_OrderAttribute_Mapping");
            this.HasKey(oam => oam.Id);

            this.HasRequired(oam => oam.Order)
                .WithMany(p => p.OrderAttributeMappings)
                .HasForeignKey(oam => oam.OrderId);

            this.HasRequired(oam => oam.OrderAttribute)
                .WithMany()
                .HasForeignKey(oam => oam.OrderAttributeId);
        }
    }
}
