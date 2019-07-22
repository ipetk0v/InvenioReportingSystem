using Invenio.Core.Domain.DeliveryNumbers;

namespace Invenio.Data.Mapping.DeliveryNumbers
{
    public class DeliveryNumberMap : NopEntityTypeConfiguration<DeliveryNumber>
    {
        public DeliveryNumberMap()
        {
            ToTable("DeliveryNumber");
            HasKey(x => x.Id);
            this.Property(x => x.Number).HasMaxLength(100);

            HasRequired(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartId);
        }
    }
}
