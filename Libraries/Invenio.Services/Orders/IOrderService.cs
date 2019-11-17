using System;
using Invenio.Core;
using Invenio.Core.Domain.Orders;

namespace Invenio.Services.Orders
{
    public interface IOrderService
    {
        void DeleteOrder(Order order);

        IPagedList<Order> GetAllOrders(
            string orderNumber = "",
            int? orderStatus = null,
            DateTime? created = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? published = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false);

        IPagedList<Order> GetAllSupplierOrders(int supplierId, int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false);

        Order GetOrderById(int orderId);

        void InsertOrder(Order order);

        void UpdateOrder(Order order);

        IPagedList<Order> GetOrdersByOrderAtributeId(int orderAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
