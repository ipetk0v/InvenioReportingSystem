using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Data;
using Invenio.Core.Domain.Reports;
using Invenio.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Invenio.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Report> _reportRepository;
        private readonly IEventPublisher _eventPublisher;

        public ReportService(
            IRepository<Report> reportRepository,
            IEventPublisher eventPublisher
        )
        {
            _reportRepository = reportRepository;
            _eventPublisher = eventPublisher;
        }

        public void DeleteReport(Report report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            _reportRepository.Delete(report);

            //event notification
            _eventPublisher.EntityDeleted(report);
        }

        public IPagedList<Report> GetAllReports(
            int CustomerId = 0,
            int regionId = 0,
            int orderId = 0,
            int isAprroved = 0,
            int userId = 0,
            DateTime? date = null,
            DateTime? DateFrom = null,
            DateTime? DateTo = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = _reportRepository.Table;

            if (CustomerId != 0)
                query = query.Where(x => x.Order.Supplier.CustomerId == CustomerId);

            if (regionId != 0)
                query = query.Where(x => x.Order.Supplier.Customer.StateProvinceId == regionId);

            if (orderId != 0)
                query = query.Where(x => x.OrderId == orderId);

            if (isAprroved == 1)
                query = query.Where(x => x.Approved == true);

            if (isAprroved == 2)
                query = query.Where(x => x.Approved == false);

            if (userId != 0)
                query = query.Where(x => x.UserId == userId);

            if (date.HasValue)
                query = query.Where(x => x.DateOfInspection.Value.Year == date.Value.Year 
                                         && x.DateOfInspection.Value.Month == date.Value.Month);

            if(DateTo.HasValue)
                query = query.Where(x => x.DateOfInspection.Value <= DateTo.Value);

            if (DateFrom.HasValue)
                query = query.Where(x => x.DateOfInspection.Value >= DateFrom.Value);

            query = query.OrderByDescending(r => r.Id);

            var reports = new PagedList<Report>(query, pageIndex, pageSize);
            return reports;
        }

        //public long GetCheckedOrderQuantity(int orderId)
        //{
        //    var query = _reportRepository.Table;

        //    if (orderId == 0)
        //        return 0;

        //    query = query.Where(x => x.Approved);

        //    var nok = query.Select(x => x.NokPartsQuantity).Sum();
        //    var ok = query.Select(x => x.OkPartsQuantity).Sum();
        //    var reworked = query.Select(x => x.ReworkPartsQuantity).Sum();

        //    return nok + ok + reworked;
        //}

        public Report GetReportById(int reportId)
        {
            if (reportId == 0)
                return null;

            return _reportRepository.GetById(reportId);
        }

        public IList<Report> GetReportsByIds(int[] reportsIds)
        {
            if (reportsIds == null || reportsIds.Length == 0)
                return new List<Report>();

            var query = from bc in _reportRepository.Table
                        where reportsIds.Contains(bc.Id)
                        select bc;
            var reports = query.ToList();
            //sort by passed identifiers
            var sortedReports = new List<Report>();
            foreach (int id in reportsIds)
            {
                var report = reports.Find(x => x.Id == id);
                if (report != null)
                    sortedReports.Add(report);
            }
            return sortedReports;
        }

        public void InsertReport(Report report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            _reportRepository.Insert(report);

            //cache
            //_cacheManager.RemoveByPattern(ORDERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(report);
        }

        public void UpdateReport(Report report)
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
