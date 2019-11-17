using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Data;
using Invenio.Core.Domain.Orders;
using System;
using System.Linq;
using Invenio.Services.Events;
using System.Collections.Generic;
using Invenio.Core.Domain.Catalog;

namespace Invenio.Services.Orders
{
    public class OrderAttributeService : IOrderAttributeService
    {
        private const string ORDERATTRIBUTES_ALL_KEY = "Invenio.orderattribute.all-{0}-{1}";
        private const string ORDERATTRIBUTES_BY_ID_KEY = "Invenio.orderattribute.id-{0}";
        private const string ORDERATTRIBUTES_PATTERN_KEY = "Invenio.orderattribute.";
        private const string ORDERATTRIBUTEMAPPINGS_PATTERN_KEY = "Invenio.orderattributemapping.";
        private const string ORDERATTRIBUTEVALUES_PATTERN_KEY = "Invenio.orderattributevalue.";
        private const string ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY = "Invenio.orderattributecombination.";
        private const string ORDERATTRIBUTEMAPPINGS_BY_ID_KEY = "Invenio.orderattributemapping.id-{0}";
        private const string ORDERATTRIBUTEMAPPINGS_ALL_KEY = "Invenio.orderattributemapping.all-{0}";
        private const string ORDERATTRIBUTEVALUES_BY_ID_KEY = "Invenio.orderattributevalue.id-{0}";
        private const string ORDERATTRIBUTEVALUES_ALL_KEY = "Invenio.orderattributevalue.all-{0}";


        private readonly IRepository<OrderAttribute> _orderAttributeRepository;
        private readonly IRepository<PredefinedOrderAttributeValue> _predefinedOrderAttributeValueRepository;
        private readonly IRepository<OrderAttributeMapping> _orderAttributeMappingRepository;
        private readonly IRepository<OrderAttributeValue> _orderAttributeValueRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        public OrderAttributeService(
            IRepository<OrderAttribute> orderAttributeRepository,
            IRepository<PredefinedOrderAttributeValue> predefinedOrderAttributeValueRepository,
            ICacheManager cacheManager,
            IEventPublisher eventPublisher,
            IRepository<OrderAttributeMapping> orderAttributeMappingRepository,
            IRepository<OrderAttributeValue> orderAttributeValueRepository
            )
        {
            _predefinedOrderAttributeValueRepository = predefinedOrderAttributeValueRepository;
            _orderAttributeRepository = orderAttributeRepository;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
            _orderAttributeMappingRepository = orderAttributeMappingRepository;
            _orderAttributeValueRepository = orderAttributeValueRepository;
        }

        public virtual IPagedList<OrderAttribute> GetAllOrderAttributes(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(ORDERATTRIBUTES_ALL_KEY, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from pa in _orderAttributeRepository.Table
                            orderby pa.Name
                            select pa;
                var orderAttribute = new PagedList<OrderAttribute>(query, pageIndex, pageSize);
                return orderAttribute;
            });
        }

        public virtual void DeleteOrderAttribute(OrderAttribute orderAttribute)
        {
            if (orderAttribute == null)
                throw new ArgumentNullException(nameof(orderAttribute));

            _orderAttributeRepository.Delete(orderAttribute);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(orderAttribute);
        }

        public virtual OrderAttribute GetOrderAttributeById(int orderAttributeId)
        {
            if (orderAttributeId == 0)
                return null;

            string key = string.Format(ORDERATTRIBUTES_BY_ID_KEY, orderAttributeId);
            return _cacheManager.Get(key, () => _orderAttributeRepository.GetById(orderAttributeId));
        }

        public virtual void UpdateOrderAttribute(OrderAttribute orderAttribute)
        {
            if (orderAttribute == null)
                throw new ArgumentNullException(nameof(orderAttribute));

            _orderAttributeRepository.Update(orderAttribute);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(orderAttribute);
        }

        public virtual void InsertOrderAttribute(OrderAttribute orderAttributeId)
        {
            if (orderAttributeId == null)
                throw new ArgumentNullException(nameof(orderAttributeId));

            _orderAttributeRepository.Insert(orderAttributeId);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(orderAttributeId);
        }

        public virtual void DeletePredefinedOrderAttributeValue(PredefinedOrderAttributeValue ppav)
        {
            if (ppav == null)
                throw new ArgumentNullException("ppav");

            _predefinedOrderAttributeValueRepository.Delete(ppav);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(ppav);
        }

        public virtual IList<PredefinedOrderAttributeValue> GetPredefinedOrderAttributeValues(int orderAttributeId)
        {
            var query = from ppav in _predefinedOrderAttributeValueRepository.Table
                        orderby ppav.DisplayOrder, ppav.Id
                        where ppav.OrderAttributeId == orderAttributeId
                        select ppav;
            var values = query.ToList();
            return values;
        }

        public virtual PredefinedOrderAttributeValue GetPredefinedOrderAttributeValueById(int id)
        {
            if (id == 0)
                return null;

            return _predefinedOrderAttributeValueRepository.GetById(id);
        }

        public virtual void InsertPredefinedOrderAttributeValue(PredefinedOrderAttributeValue ppav)
        {
            if (ppav == null)
                throw new ArgumentNullException("ppav");

            _predefinedOrderAttributeValueRepository.Insert(ppav);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(ppav);
        }

        public virtual void UpdatePredefinedOrderAttributeValue(PredefinedOrderAttributeValue ppav)
        {
            if (ppav == null)
                throw new ArgumentNullException("ppav");

            _predefinedOrderAttributeValueRepository.Update(ppav);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(ppav);
        }

        public virtual void DeleteOrderAttributeMapping(OrderAttributeMapping orderAttributeMapping)
        {
            if (orderAttributeMapping == null)
                throw new ArgumentNullException(nameof(orderAttributeMapping));

            _orderAttributeMappingRepository.Delete(orderAttributeMapping);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(orderAttributeMapping);
        }

        public virtual IList<OrderAttributeMapping> GetOrderAttributeMappingsByOrderAttributeId(int orderAttributeId)
        {
            string key = string.Format(ORDERATTRIBUTEMAPPINGS_ALL_KEY, orderAttributeId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pam in _orderAttributeMappingRepository.Table
                                //orderby pam.DisplayOrder, pam.Id
                            where pam.OrderAttributeId == orderAttributeId
                            select pam;
                var orderAttributeMappings = query.ToList();
                return orderAttributeMappings;
            });
        }

        public virtual IList<OrderAttributeMapping> GetOrderAttributeMappingsByOrderId(int orderId)
        {
            string key = string.Format(ORDERATTRIBUTEMAPPINGS_ALL_KEY, orderId);

            return _cacheManager.Get(key, () =>
            {
                var query = from pam in _orderAttributeMappingRepository.Table
                                //orderby pam.DisplayOrder, pam.Id
                            where pam.OrderId == orderId
                            select pam;
                var orderAttributeMappings = query.ToList();
                return orderAttributeMappings;
            });
        }

        public virtual OrderAttributeMapping GetOrderAttributeMappingById(int orderAttributeMappingId)
        {
            if (orderAttributeMappingId == 0)
                return null;

            string key = string.Format(ORDERATTRIBUTEMAPPINGS_BY_ID_KEY, orderAttributeMappingId);
            return _cacheManager.Get(key, () => _orderAttributeMappingRepository.GetById(orderAttributeMappingId));
        }

        public virtual void InsertOrderAttributeMapping(OrderAttributeMapping orderAttributeMapping)
        {
            if (orderAttributeMapping == null)
                throw new ArgumentNullException(nameof(orderAttributeMapping));

            _orderAttributeMappingRepository.Insert(orderAttributeMapping);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(orderAttributeMapping);
        }

        public virtual void UpdateOrderAttributeMapping(OrderAttributeMapping orderAttributeMapping)
        {
            if (orderAttributeMapping == null)
                throw new ArgumentNullException(nameof(orderAttributeMapping));

            _orderAttributeMappingRepository.Update(orderAttributeMapping);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(orderAttributeMapping);
        }

        public virtual void InsertOrderAttributeValue(OrderAttributeValue orderAttributeValue)
        {
            if (orderAttributeValue == null)
                throw new ArgumentNullException(nameof(orderAttributeValue));

            _orderAttributeValueRepository.Insert(orderAttributeValue);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(orderAttributeValue);
        }

        public virtual void DeleteOrderAttributeValue(OrderAttributeValue orderAttributeValue)
        {
            if (orderAttributeValue == null)
                throw new ArgumentNullException("orderAttributeValue");

            _orderAttributeValueRepository.Delete(orderAttributeValue);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(orderAttributeValue);
        }

        public virtual IList<OrderAttributeValue> GetOrderAttributeValues(int orderAttributeMappingId)
        {
            string key = string.Format(ORDERATTRIBUTEVALUES_ALL_KEY, orderAttributeMappingId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pav in _orderAttributeValueRepository.Table
                            orderby pav.Id
                            where pav.OrderAttributeMappingId == orderAttributeMappingId
                            select pav;
                var orderAttributeValues = query.ToList();
                return orderAttributeValues;
            });
        }

        public virtual OrderAttributeValue GetOrderAttributeValueById(int orderAttributeValueId)
        {
            if (orderAttributeValueId == 0)
                return null;

            string key = string.Format(ORDERATTRIBUTEVALUES_BY_ID_KEY, orderAttributeValueId);
            return _cacheManager.Get(key, () => _orderAttributeValueRepository.GetById(orderAttributeValueId));
        }

        public virtual void UpdateOrderAttributeValue(OrderAttributeValue orderAttributeValue)
        {
            if (orderAttributeValue == null)
                throw new ArgumentNullException("orderAttributeValue");

            _orderAttributeValueRepository.Update(orderAttributeValue);

            //cache
            _cacheManager.RemoveByPattern(ORDERATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEMAPPINGS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTEVALUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(ORDERATTRIBUTECOMBINATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(orderAttributeValue);
        }
    }
}
