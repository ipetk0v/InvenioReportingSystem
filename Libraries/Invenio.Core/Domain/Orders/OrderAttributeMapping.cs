using System.Collections.Generic;

namespace Invenio.Core.Domain.Orders
{
    public class OrderAttributeMapping : BaseEntity
    {
        private ICollection<OrderAttributeValue> _orderAttributeValues;

        public int OrderId { get; set; }

        public int OrderAttributeId { get; set; }

        public string TextPrompt { get; set; }

        public virtual OrderAttribute OrderAttribute { get; set; }

        public virtual Order Order { get; set; }

        public virtual ICollection<OrderAttributeValue> OrderAttributeValues
        {
            get { return _orderAttributeValues ?? (_orderAttributeValues = new List<OrderAttributeValue>()); }
            protected set { _orderAttributeValues = value; }
        }
    }
}
