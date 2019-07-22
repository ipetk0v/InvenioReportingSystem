using Invenio.Core.Data;
using Invenio.Core.Domain.Parts;
using Invenio.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Invenio.Services.Parts
{
    public class PartService : IPartService
    {
        private readonly IRepository<Part> _partRepository;
        private readonly IEventPublisher _eventPublisher;

        public PartService(IRepository<Part> partRepository, IEventPublisher eventPublisher)
        {
            _partRepository = partRepository;
            _eventPublisher = eventPublisher;
        }

        public ICollection<Part> GetAllOrderParts(int orderId)
        {
            if (orderId == 0)
                return null;

            var query = _partRepository.Table;

            query = query.Where(c => c.OrderId == orderId);
            query = query.OrderByDescending(c => c.Id).ThenBy(c => c.SerNumber);

            return query.ToList();
        }

        public Part GetPartById(int partId)
        {
            if (partId == 0)
                return null;

            return _partRepository.GetById(partId);
        }

        //public Part GetPartByReportId(int reportId)
        //{
        //    if (reportId == 0)
        //        return null;

        //    return _partRepository.Table.Where(x => x.ReportId == reportId).SingleOrDefault();
        //}

        public void InsertPart(Part part)
        {
            if (part == null)
                throw new ArgumentNullException("part");

            _partRepository.Insert(part);

            ////cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(part);
        }

        public void DeletePart(Part part)
        {
            if (part == null)
                throw new ArgumentNullException("part");

            _partRepository.Delete(part);

            _eventPublisher.EntityDeleted(part);
        }

        public void UpdatePart(Part part)
        {
            if (part == null)
                throw new ArgumentNullException("part");

            _partRepository.Update(part);

            //cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(part);
        }
    }
}
