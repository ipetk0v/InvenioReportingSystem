using Invenio.Core.Domain.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Data;
using Invenio.Core.Domain.Orders;
using Invenio.Services.Events;

namespace Invenio.Services.Reports
{
    public class ReportDetailService : IReportDetailService
    {
        private readonly IRepository<ReportDetail> _reportRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        public ReportDetailService(
            IRepository<ReportDetail> reportRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager
        )
        {
            _reportRepository = reportRepository;
            _workContext = workContext;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;
            _cacheManager = cacheManager;
        }

        public void DeleteReport(ReportDetail report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            //report.Deleted = true;
            UpdateReport(report);

            //event notification
            _eventPublisher.EntityDeleted(report);
        }

        public ReportDetail GetReportById(int reportId)
        {
            if (reportId == 0)
                return null;

            return _reportRepository.GetById(reportId);
        }

        public IList<ReportDetail> GetReportDetailsByReportId(int reportId)
        {
            if (reportId == 0)
                return null;

            return _reportRepository.Table.Where(x => x.ReportId == reportId).ToList();
        }

        public IList<ReportDetail> GetReportDetailsByOrderId(int orderId,bool isApprove = true)
        {
            if (orderId == 0)
                return null;
            var query = _reportRepository.Table;

            //query = query.Join(_orderRepository.Table, x => x.Id, j=> j.EntityId, )
            if(isApprove)
            query = query.Where(x => x.Report.Approved == isApprove);

            query = query.Where(x => x.Report.OrderId == orderId);
            

            return query.ToList();
        }

        public void InsertReport(ReportDetail report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            _reportRepository.Insert(report);

            //cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(report);
        }

        public void UpdateReport(ReportDetail report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            _reportRepository.Update(report);

            //cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(report);
        }
    }
}
