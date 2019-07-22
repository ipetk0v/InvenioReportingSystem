using Invenio.Core.Domain.Criterias;

namespace Invenio.Data.Mapping.Criterias
{
    public class CriteriaMap : NopEntityTypeConfiguration<Criteria>
    {
        public CriteriaMap()
        {
            ToTable("Criteria");
            HasKey(c => c.Id);
            this.Property(x => x.Description).HasMaxLength(200);

            HasRequired(o => o.Order)
                .WithMany()
                .HasForeignKey(o => o.OrderId);
        }
    }
}
