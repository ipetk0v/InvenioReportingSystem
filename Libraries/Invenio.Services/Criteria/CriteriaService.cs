using Invenio.Core.Data;
using Invenio.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Invenio.Core.Domain.Criterias;

namespace Invenio.Services.Criteria
{
    public class CriteriaService : ICriteriaService
    {
        private IRepository<Core.Domain.Criterias.Criteria> _criteriaRepository;
        private readonly IEventPublisher _eventPublisher;

        public CriteriaService(
            IEventPublisher eventPublisher,
            IRepository<Core.Domain.Criterias.Criteria> criteriaRepository
            )
        {
            _criteriaRepository = criteriaRepository;
            _eventPublisher = eventPublisher;
        }

        public void DeleteCriteria(Core.Domain.Criterias.Criteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            _criteriaRepository.Delete(criteria);

            //cache
            //_cacheManager.RemoveByPattern(criteriaS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(criteria);
        }

        public IList<Core.Domain.Criterias.Criteria> GetAllCriteriaValues(int orderId)
        {
            if (orderId == 0)
                return null;

            var query = _criteriaRepository.Table;

            query = query.Where(c => c.OrderId == orderId);
            query = query.OrderByDescending(c => c.Id).ThenBy(c => c.Description);

            return query.ToList();
        }

        public IList<int> GetAllCriteriaValues(int orderId, CriteriaType ct)
        {
            if (orderId == 0)
                return null;

            var query = _criteriaRepository.Table;

            query = query.Where(c => c.OrderId == orderId);
            query = query.Where(x => x.CriteriaType == ct);
            query = query.OrderByDescending(c => c.Id).ThenBy(c => c.Description);

            return query.Select(x => x.Id).ToList();
        }

        public IList<Core.Domain.Criterias.Criteria> GetCriteriaByDescription(string description, int orderId, bool v)
        {
            if (orderId == 0)
                return null;

            if (string.IsNullOrEmpty(description))
                return null;

            var query = _criteriaRepository.Table;

            query = query.Where(c => c.OrderId == orderId);
            query = query.Where(c => c.Description.ToLower().Trim().Contains(description.ToLower().Trim()));
            query = query.OrderByDescending(c => c.Id).ThenBy(c => c.Description);

            return query.ToList();
        }

        public Core.Domain.Criterias.Criteria GetCriteriaById(int id)
        {
            if (id == 0)
                return null;

            return _criteriaRepository.GetById(id);
        }


        public void InsertCriteria(Core.Domain.Criterias.Criteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            _criteriaRepository.Insert(criteria);

            //cache
            //_cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(criteria);
        }

        public void UpdateCriteria(Core.Domain.Criterias.Criteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");

            //update language
            _criteriaRepository.Update(criteria);

            //cache
            //_cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(criteria);
        }
    }
}
