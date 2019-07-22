using Invenio.Core.Domain.ChargeNumbers;

namespace Invenio.Data.Mapping.ChargeNumbers
{
    public class ChargeNumberMap : NopEntityTypeConfiguration<ChargeNumber>
    {
        public ChargeNumberMap()
        {
            ToTable("ChargeNumber");
            this.HasKey(cn => cn.Id);
            this.Property(x => x.Number).HasMaxLength(100);

            HasRequired(x => x.DeliveryNumber)
                .WithMany()
                .HasForeignKey(x => x.DeliveryNumberId);
        }
    }
}
