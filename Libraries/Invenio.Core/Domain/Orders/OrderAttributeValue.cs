using Invenio.Core.Domain.Localization;

namespace Invenio.Core.Domain.Orders
{
    public class OrderAttributeValue : BaseEntity, ILocalizedEntity
    {
        public int OrderAttributeMappingId { get; set; }
        public int? ParentAttributeValueId { get; set; }

        public string Name { get; set; }

        public virtual OrderAttributeMapping OrderAttributeMapping { get; set; }
        public virtual OrderAttributeValue ParentAttributeValue { get; set; }
    }
}
