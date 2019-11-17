using Invenio.Core.Domain.Catalog;

namespace Invenio.Data.Mapping.Orders
{
    public class PredefinedOrderAttributeValueMap : NopEntityTypeConfiguration<PredefinedOrderAttributeValue>
    {
        public PredefinedOrderAttributeValueMap()
        {
            this.ToTable("PredefinedOrderAttributeValue");
            this.HasKey(pav => pav.Id);
            this.Property(pav => pav.Name).IsRequired().HasMaxLength(400);

            this.Property(pav => pav.PriceAdjustment).HasPrecision(18, 4);
            this.Property(pav => pav.WeightAdjustment).HasPrecision(18, 4);
            this.Property(pav => pav.Cost).HasPrecision(18, 4);

            this.HasRequired(pav => pav.OrderAttribute)
                .WithMany()
                .HasForeignKey(pav => pav.OrderAttributeId);
        }
    }
}
