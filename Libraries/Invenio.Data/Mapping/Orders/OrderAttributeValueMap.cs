using Invenio.Core.Domain.Orders;

namespace Invenio.Data.Mapping.Orders
{
    public class OrderAttributeValueMap : NopEntityTypeConfiguration<OrderAttributeValue>
    {
        public OrderAttributeValueMap()
        {
            this.ToTable("OrderAttributeValue");
            this.HasKey(oav => oav.Id);
            this.Property(oav => oav.Name).IsRequired().HasMaxLength(400);

            this.HasRequired(oav => oav.OrderAttributeMapping)
                .WithMany(oav => oav.OrderAttributeValues)
                .HasForeignKey(oav => oav.OrderAttributeMappingId);

            this.HasOptional(oav => oav.ParentAttributeValue)
                .WithMany()
                .HasForeignKey(oav => oav.ParentAttributeValueId);
        }
    }
}
