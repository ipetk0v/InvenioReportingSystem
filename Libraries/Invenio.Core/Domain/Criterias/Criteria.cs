using Invenio.Core.Domain.Orders;

namespace Invenio.Core.Domain.Criterias
{
    public class Criteria : BaseEntity
    {
        public string Description { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public CriteriaType CriteriaType { get; set; }
    }
}
