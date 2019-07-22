using Invenio.Core.Domain.Parts;

namespace Invenio.Data.Mapping.Parts
{
    public class PartMap : NopEntityTypeConfiguration<Part>
    {
        public PartMap()
        {
            ToTable("Part");
            this.HasKey(ar => ar.Id);

            this.HasRequired(o => o.Order)
                .WithMany()
                .HasForeignKey(o => o.OrderId);
        }
    }
}
