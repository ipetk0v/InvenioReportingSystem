using Invenio.Core.Data;
using Invenio.Core.Domain.DeliveryNumbers;
using Invenio.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Invenio.Services.DeliveryNumber
{
    public class DeliveryNumberService : IDeliveryNumberService
    {
        private readonly IRepository<Core.Domain.DeliveryNumbers.DeliveryNumber> _deliveryRepository;
        private readonly IEventPublisher _eventPublisher;

        public DeliveryNumberService(
            IRepository<Core.Domain.DeliveryNumbers.DeliveryNumber> deliveryRepository, 
            IEventPublisher eventPublisher)
        {
            _deliveryRepository = deliveryRepository;
            _eventPublisher = eventPublisher;
        }

        public Core.Domain.DeliveryNumbers.DeliveryNumber GetDeliveryNumberById(int delNumberId)
        {
            if (delNumberId == 0)
                return null;

            return _deliveryRepository.GetById(delNumberId);
        }

        public void InsertDeliveryNumber(Core.Domain.DeliveryNumbers.DeliveryNumber delNumber)
        {
            if (delNumber == null)
                throw new ArgumentNullException(nameof(delNumber));

            _deliveryRepository.Insert(delNumber);

            ////cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(delNumber);
        }

        public void UpdateDeliveryNumber(Core.Domain.DeliveryNumbers.DeliveryNumber delNumber)
        {
            if (delNumber == null)
                throw new ArgumentNullException(nameof(delNumber));

            _deliveryRepository.Update(delNumber);

            //cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(delNumber);
        }

        public void DeleteDeliveryNumber(Core.Domain.DeliveryNumbers.DeliveryNumber delNumber)
        {
            if (delNumber == null)
                throw new ArgumentNullException(nameof(delNumber));

            _deliveryRepository.Delete(delNumber);

            _eventPublisher.EntityDeleted(delNumber);
        }

        public ICollection<Core.Domain.DeliveryNumbers.DeliveryNumber> GetAllPartDeliveryNumbers(int partId)
        {
            if (partId == 0)
                return null;

            var query = _deliveryRepository.Table;

            //query = query.Where(c => c.OrderId == orderId);
            query = query.Where(x => x.PartId == partId);
            query = query.OrderByDescending(c => c.Id).ThenBy(c => c.Number);

            return query.ToList();
        }
    }
}
