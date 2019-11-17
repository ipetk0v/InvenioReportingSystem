using Invenio.Core.Domain.Reports;

namespace Invenio.Data.Mapping.Reports
{
    public class ReportOrderAttributeMap : NopEntityTypeConfiguration<ReportOrderAttribute>
    {
        public ReportOrderAttributeMap()
        {
            ToTable("ReportOrderAttributeMap");
            this.HasKey(r => r.Id);

            this.HasRequired(r => r.Report)
                .WithMany(p => p.ReportOrderAttributes)
                .HasForeignKey(r => r.ReportId);
        }
    }
}
