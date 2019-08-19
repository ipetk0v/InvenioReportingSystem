using System.Collections.Generic;
using Invenio.Core.Domain.Reports;

namespace Invenio.Services.Reports
{
    public interface IReportDetailService
    {
        void DeleteReport(ReportDetail report);

        //IPagedList<Report> GetAllReports(string reportName = "",
        //    int pageIndex = 0,
        //    int pageSize = int.MaxValue,
        //    bool showHidden = false);

        //IPagedList<Report> GetAllSupplierReports(int supplierId, int pageIndex = 0, int pageSize = int.MaxValue,
        //    bool showHidden = false);

        IList<ReportDetail> GetReportDetailsByReportId(int reportId);

        ReportDetail GetReportById(int reportId);

        void InsertReport(ReportDetail report);

        void UpdateReport(ReportDetail report);

        IList<ReportDetail> GetReportDetailsByOrderId(int orderId, bool isApprove = true);
    }
}
