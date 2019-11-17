using Invenio.Core.Domain.Localization;
using Invenio.Core.Domain.Orders;

namespace Invenio.Core.Domain.Catalog
{
    public class PredefinedOrderAttributeValue : BaseEntity, ILocalizedEntity
    {
        public int OrderAttributeId { get; set; }

        public string Name { get; set; }

        public decimal PriceAdjustment { get; set; }

        public decimal WeightAdjustment { get; set; }

        public decimal Cost { get; set; }

        public bool IsPreSelected { get; set; }

        public int DisplayOrder { get; set; }

        public virtual OrderAttribute OrderAttribute { get; set; }
    }
}
