using Invenio.Core.Data;
using Invenio.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Invenio.Services.ChargeNumber
{
    public class ChargeNumberService : IChargeNumberService
    {
        private readonly IRepository<Core.Domain.ChargeNumbers.ChargeNumber> _chargeRepository;
        private readonly IEventPublisher _eventPublisher;

        public ChargeNumberService(
            IRepository<Core.Domain.ChargeNumbers.ChargeNumber> chargeRepository,
            IEventPublisher eventPublisher)
        {
            _chargeRepository = chargeRepository;
            _eventPublisher = eventPublisher;
        }

        public Core.Domain.ChargeNumbers.ChargeNumber GetChargeNumberById(int delNumberId)
        {
            if (delNumberId == 0)
                return null;

            return _chargeRepository.GetById(delNumberId);
        }

        public void InsertChargeNumber(Core.Domain.ChargeNumbers.ChargeNumber delNumber)
        {
            if (delNumber == null)
                throw new ArgumentNullException(nameof(delNumber));

            _chargeRepository.Insert(delNumber);

            ////cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(delNumber);
        }

        public void UpdateChargeNumber(Core.Domain.ChargeNumbers.ChargeNumber delNumber)
        {
            if (delNumber == null)
                throw new ArgumentNullException(nameof(delNumber));

            _chargeRepository.Update(delNumber);

            //cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(delNumber);
        }

        public void DeleteChargeNumber(Core.Domain.ChargeNumbers.ChargeNumber delNumber)
        {
            if (delNumber == null)
                throw new ArgumentNullException(nameof(delNumber));

            _chargeRepository.Delete(delNumber);

            _eventPublisher.EntityDeleted(delNumber);
        }

        public ICollection<Core.Domain.ChargeNumbers.ChargeNumber> GetAllDeliveryChargeNumbers(int delNumberId)
        {
            if (delNumberId == 0)
                return null;

            var query = _chargeRepository.Table;

            query = query.Where(c => c.DeliveryNumberId == delNumberId);
            query = query.OrderByDescending(c => c.Id).ThenBy(c => c.Number);

            return query.ToList();
        }
    }
}
