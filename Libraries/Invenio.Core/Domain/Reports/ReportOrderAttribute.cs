using Invenio.Core.Domain.Orders;

namespace Invenio.Core.Domain.Reports
{
    public class ReportOrderAttribute : BaseEntity
    {
        public int ReportId { get; set; }
        public Report Report { get; set; }

        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }
    }
}
