using Invenio.Core.Domain.Reports;

namespace Invenio.Data.Mapping.Reports
{
    public class ReportMap : NopEntityTypeConfiguration<Report>
    {
        public ReportMap()
        {
            ToTable("Report");
            this.HasKey(r => r.Id);

            HasRequired(o => o.Order)
                .WithMany()
                .HasForeignKey(o => o.OrderId);

            //HasRequired(p => p.Part)
            //    .WithMany()
            //    .HasForeignKey(p => p.PartId);
        }
    }
}
