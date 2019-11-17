using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Data;
using Invenio.Core.Domain.Orders;
using Invenio.Services.Events;
using System;
using System.Linq;

namespace Invenio.Services.Orders
{
    public class OrderService : IOrderService
    {
        private const string ORDERS_BY_ID_KEY = "Invenio.order.id-{0}";
        private const string ORDERS_PATTERN_KEY = "Invenio.order.";


        private readonly IRepository<Order> _orderRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        public OrderService(
            IRepository<Order> orderRepository,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager
            )
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
            _cacheManager = cacheManager;
        }

        public void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            order.Deleted = true;
            UpdateOrder(order);

            //event notification
            _eventPublisher.EntityDeleted(order);
        }

        public IPagedList<Order> GetAllOrders(
            string orderNumber = "",
            int? orderStatus = null,
            DateTime? created = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? published = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = _orderRepository.Table;
            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!string.IsNullOrWhiteSpace(orderNumber))
                query = query.Where(m => m.Number.ToLower().Contains(orderNumber.Trim().ToLower()));

            if (orderStatus.HasValue)
            {
                if (orderStatus.Value == 0)
                    query = query.Where(x => !x.StartDate.HasValue && !x.EndDate.HasValue);

                if (orderStatus.Value == 5)
                    query = query.Where(x => x.StartDate.HasValue && !x.EndDate.HasValue);

                if (orderStatus.Value == 10)
                    query = query.Where(x => x.StartDate.HasValue && x.EndDate.HasValue);
            }

            if (created.HasValue)
                query = query.Where(x => x.CreatedOnUtc.HasValue && x.CreatedOnUtc.Value == created.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.StartDate.HasValue && x.StartDate.Value == startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.EndDate.HasValue && x.EndDate.Value == endDate.Value);

            if (published.HasValue)
                query = query.Where(x => x.Published == published.Value);

            query = query.Where(m => !m.Deleted);
            query = query.OrderByDescending(m => m.Id).ThenBy(x => x.StartDate);

            return new PagedList<Order>(query, pageIndex, pageSize);
        }

        public IPagedList<Order> GetAllSupplierOrders(int supplierId, int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            if (supplierId == 0)
                return null;

            var query = _orderRepository.Table;

            if (!showHidden)
                query = query.Where(x => x.Published);

            query = query.Where(x => x.Deleted == false);
            query = query.Where(x => x.SupplierId == supplierId);
            query = query.OrderBy(x => x.Id);

            return new PagedList<Order>(query, pageIndex, pageSize);
        }

        public Order GetOrderById(int orderId)
        {
            if (orderId == 0)
                return null;

            string key = string.Format(ORDERS_BY_ID_KEY, orderId);
            return _cacheManager.Get(key, () => _orderRepository.GetById(orderId));
        }

        public void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Insert(order);

            //cache
            _cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(order);
        }

        public void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Update(order);

            //cache
            _cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(order);
        }

        public virtual IPagedList<Order> GetOrdersByOrderAtributeId(int orderAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _orderRepository.Table;
            query = query.Where(x => x.OrderAttributeMappings.Any(y => y.OrderAttributeId == orderAttributeId));
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.Name);

            var order = new PagedList<Order>(query, pageIndex, pageSize);
            return order;
        }
    }
}
