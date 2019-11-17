namespace Invenio.Core.Domain.Reports
{
    public class ReportDetail : BaseEntity
    {
        public int CriteriaId { get; set; }
        public int Quantity { get; set; }

        public int ReportId { get; set; }
        public Report Report { get; set; }
    }
}
