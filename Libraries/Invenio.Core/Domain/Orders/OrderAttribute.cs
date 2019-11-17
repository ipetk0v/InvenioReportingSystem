using Invenio.Core.Domain.Localization;

namespace Invenio.Core.Domain.Orders
{
    public class OrderAttribute : BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int? ParentOrderAttributeId { get; set; }
        public virtual OrderAttribute ParentOrderAttribute { get; set; }
    }
}
