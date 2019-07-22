using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Reports;

namespace Invenio.Core.Domain.Parts
{
    public class Part : BaseEntity
    {
        public string Name { get; set; }
        public string SerNumber { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
