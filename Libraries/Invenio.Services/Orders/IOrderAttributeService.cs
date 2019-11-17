using Invenio.Core;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Orders;
using System.Collections.Generic;

namespace Invenio.Services.Orders
{
    public interface IOrderAttributeService
    {
        IPagedList<OrderAttribute> GetAllOrderAttributes(int pageIndex = 0, int pageSize = int.MaxValue);
        void DeleteOrderAttribute(OrderAttribute orderAttribute);
        OrderAttribute GetOrderAttributeById(int orderAttributeId);
        void InsertOrderAttribute(OrderAttribute orderAttributeId);
        void UpdateOrderAttribute(OrderAttribute orderAttribute);
        IList<PredefinedOrderAttributeValue> GetPredefinedOrderAttributeValues(int orderAttributeId);
        PredefinedOrderAttributeValue GetPredefinedOrderAttributeValueById(int id);
        void InsertPredefinedOrderAttributeValue(PredefinedOrderAttributeValue ppav);
        void UpdatePredefinedOrderAttributeValue(PredefinedOrderAttributeValue ppav);
        void DeletePredefinedOrderAttributeValue(PredefinedOrderAttributeValue ppav);
        void DeleteOrderAttributeMapping(OrderAttributeMapping orderAttributeMapping);
        IList<OrderAttributeMapping> GetOrderAttributeMappingsByOrderId(int orderId);
        OrderAttributeMapping GetOrderAttributeMappingById(int orderAttributeMappingId);
        void UpdateOrderAttributeMapping(OrderAttributeMapping orderAttributeMapping);
        void InsertOrderAttributeMapping(OrderAttributeMapping orderAttributeMapping);
        void InsertOrderAttributeValue(OrderAttributeValue productAttributeValue);
        IList<OrderAttributeMapping> GetOrderAttributeMappingsByOrderAttributeId(int orderAttributeId);
        void UpdateOrderAttributeValue(OrderAttributeValue orderAttributeValue);
        IList<OrderAttributeValue> GetOrderAttributeValues(int orderAttributeMappingId);
        OrderAttributeValue GetOrderAttributeValueById(int orderAttributeValueId);
        void DeleteOrderAttributeValue(OrderAttributeValue orderAttributeValue);
    }
}
