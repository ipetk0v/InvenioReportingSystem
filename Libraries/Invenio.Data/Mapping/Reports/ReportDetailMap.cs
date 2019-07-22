using Invenio.Core.Domain.Reports;

namespace Invenio.Data.Mapping.Reports
{
    public class ReportDetailMap : NopEntityTypeConfiguration<ReportDetail>
    {
        public ReportDetailMap()
        {
            ToTable("ReportDetail");
            this.HasKey(r => r.Id);

            HasRequired(r => r.Report)
                .WithMany()
                .HasForeignKey(r => r.ReportId);
        }
    }
}
