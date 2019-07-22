using System;
using System.Collections.Generic;
using Invenio.Core;
using Invenio.Core.Domain.Reports;

namespace Invenio.Services.Reports
{
    public interface IReportService
    {
        void DeleteReport(Report report);

        IPagedList<Report> GetAllReports(
            int manufacturerId = 0,
            int regionId = 0,
            int orderId = 0,
            int isAprroved = 0,
            int userId = 0,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false);

        //IPagedList<Report> GetAllCustomerReports(int customerId, int pageIndex = 0, int pageSize = int.MaxValue,
        //    bool showHidden = false);

        Report GetReportById(int reportId);

        IList<Report> GetReportsByIds(int[] reportsIds);

        void InsertReport(Report report);

        void UpdateReport(Report report);

        //long GetCheckedOrderQuantity(int orderId);
    }
}
