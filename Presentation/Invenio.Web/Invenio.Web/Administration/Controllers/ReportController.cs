using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Invenio.Admin.Models.Report;
using Invenio.Services.Security;
using Invenio.Web.Framework.Kendoui;
using System.Web.Mvc;
using Invenio.Services.Helpers;
using Invenio.Services.Reports;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Invenio.Admin.Extensions;
using Invenio.Core;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Reports;
using Invenio.Services.Customers;
using Invenio.Services.Events;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Orders;
using Invenio.Services.Users;
using Invenio.Web.Framework.Mvc;
using Invenio.Core.Domain.Orders;
using Invenio.Services.ChargeNumber;
using Invenio.Services.Criteria;
using Invenio.Services.Parts;
using Invenio.Services.DeliveryNumber;
using Invenio.Web.Framework.Controllers;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.Style;

namespace Invenio.Admin.Controllers
{
    public class ReportController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IReportService _reportService;
        private readonly IWorkContext _workContext;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserActivityService _userActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;
        private readonly IPartService _partService;
        private readonly IReportDetailService _reportDetailService;
        private readonly IChargeNumberService _chargeNumberService;
        private readonly IDeliveryNumberService _deliveryNumberService;
        private readonly ICriteriaService _criteriaService;

        public ReportController(
            IPermissionService permissionService,
            IDateTimeHelper dateTimeHelper,
            IReportService reportService,
            IWorkContext workContext,
            IUserService userService,
            IOrderService orderService,
            ILocalizationService localizationService,
            IUserActivityService userActivityService,
            IEventPublisher eventPublisher,
            ICustomerService customerService,
            IPartService partService,
            IReportDetailService reportDetailService,
            IChargeNumberService chargeNumberService,
            IDeliveryNumberService deliveryNumberService,
            ICriteriaService criteriaService
            )
        {
            _dateTimeHelper = dateTimeHelper;
            _permissionService = permissionService;
            _reportService = reportService;
            _workContext = workContext;
            _userService = userService;
            _orderService = orderService;
            _localizationService = localizationService;
            _userActivityService = userActivityService;
            _eventPublisher = eventPublisher;
            _customerService = customerService;
            _partService = partService;
            _reportDetailService = reportDetailService;
            _chargeNumberService = chargeNumberService;
            _deliveryNumberService = deliveryNumberService;
            _criteriaService = criteriaService;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction(nameof(List));
        }

        #region Report List for Approve
        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            var model = new ReportListModel();

            model.AvailableApprovedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.All"), Value = "0" });
            model.AvailableApprovedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.ApprovedOnly"), Value = "1" });
            model.AvailableApprovedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.DisapprovedOnly"), Value = "2" });

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, ReportListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var rm = new List<ReportModel>();

            foreach (var stateProvince in _workContext.CurrentUser.ManufacturerRegions)
            {
                var reports = _reportService
                    .GetAllReports(
                    regionId: stateProvince.Id,
                    isAprroved: model.SearchApprovedId,
                    fromDate: model.CreatedOnFrom,
                    toDate: model.CreatedOnTo);
                if (reports.Any())
                {
                    rm.AddRange(reports.Select(x =>
                    {
                        var r = x.ToModel();

                        r.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOn);
                        if (x.DateOfInspection.HasValue)
                            r.DateOfInspection = _dateTimeHelper.ConvertToUserTime(x.DateOfInspection.Value);
                        if (x.ApprovedOn.HasValue)
                            r.ApprovedOn = _dateTimeHelper.ConvertToUserTime(x.ApprovedOn.Value);
                        if (x.UserId != 0)
                            r.UserName = _userService.GetUserById(x.UserId)?.GetFullName();
                        if (x.OrderId == 0) return r;
                        var order = _orderService.GetOrderById(x.OrderId);
                        r.OrderNumber = order.Number;
                        r.CustomerName = order.Customer.Name;
                        r.WorkShiftName = ((WorkShift)r.WorkShift).GetLocalizedEnum(_localizationService, _workContext);

                        return r;
                    }));
                }
            }

            foreach (var man in _workContext.CurrentUser.Manufacturers)
            {
                var reports = _reportService
                    .GetAllReports(manufacturerId: man.Id,
                    isAprroved: model.SearchApprovedId,
                    fromDate: model.CreatedOnFrom,
                    toDate: model.CreatedOnTo);
                if (reports.Any())
                {
                    rm.AddRange(reports.Select(x =>
                    {
                        var r = x.ToModel();

                        r.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOn, DateTimeKind.Utc);
                        if (x.DateOfInspection.HasValue)
                            r.DateOfInspection = _dateTimeHelper.ConvertToUserTime(x.DateOfInspection.Value);
                        if (x.ApprovedOn.HasValue)
                            r.ApprovedOn = _dateTimeHelper.ConvertToUserTime(x.ApprovedOn.Value, DateTimeKind.Utc);
                        if (x.UserId != 0)
                            r.UserName = _userService.GetUserById(x.UserId)?.GetFullName();
                        if (x.OrderId == 0) return r;
                        var order = _orderService.GetOrderById(x.OrderId);
                        r.OrderNumber = order.Number;
                        r.CustomerName = order.Customer.Name;
                        r.WorkShiftName = ((WorkShift)r.WorkShift).GetLocalizedEnum(_localizationService, _workContext);

                        return r;
                    }));
                }
            }

            var result = new PagedList<ReportModel>(rm, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = result.OrderByDescending(x => x.Id),
                Total = result.TotalCount
            };


            return Json(gridModel);
        }

        [HttpPost]
        public virtual ActionResult ApproveSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var reports = _reportService.GetReportsByIds(selectedIds.ToArray()).Where(report => !report.Approved);

                foreach (var report in reports)
                {
                    report.Approved = true;
                    report.ApprovedOn = DateTime.Now;
                    _reportService.UpdateReport(report);

                    PlusTotalCheckedPartsQuantity(report);

                    //activity log
                    _userActivityService.InsertActivity("EditReport", _localizationService.GetResource("ActivityLog.EditReport"), report.Id);
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual ActionResult DisapproveSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                //filter approved comments
                var reports = _reportService.GetReportsByIds(selectedIds.ToArray()).Where(report => report.Approved);

                foreach (var report in reports)
                {
                    report.Approved = false;
                    _reportService.UpdateReport(report);
                    MinusTotalCheckedPartsQuantity(report);

                    //activity log
                    _userActivityService.InsertActivity("EditReport", _localizationService.GetResource("ActivityLog.EditReport"), report.Id);

                }
            }

            return Json(new { Result = true });
        }

        private void MinusTotalCheckedPartsQuantity(Report report)
        {
            var order = _orderService.GetOrderById(report.OrderId);
            order.CheckedPartsQuantity -= (int)(report.ReworkPartsQuantity + report.NokPartsQuantity + report.OkPartsQuantity);
            _orderService.UpdateOrder(order);
        }

        private void PlusTotalCheckedPartsQuantity(Report report)
        {
            var order = _orderService.GetOrderById(report.OrderId);
            order.CheckedPartsQuantity += (int)(report.ReworkPartsQuantity + report.NokPartsQuantity + report.OkPartsQuantity);
            _orderService.UpdateOrder(order);
        }

        [HttpPost]
        public virtual ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var reports = _reportService.GetReportsByIds(selectedIds.ToArray());

                foreach (var report in reports)
                {
                    _reportService.DeleteReport(report);
                    if (report.Approved)
                    {
                        MinusTotalCheckedPartsQuantity(report);
                    }
                }

                //activity log
                foreach (var report in reports)
                {
                    _userActivityService.InsertActivity("DeleteReports", _localizationService.GetResource("ActivityLog.DeleteReports"), report.Id);
                }
            }

            return Json(new { Result = true });
        }

        public virtual ActionResult Update(ReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            var report = _reportService.GetReportById(model.Id);
            if (report == null)
                throw new ArgumentException("No report found with the specified id");

            report.OkPartsQuantity = model.OkPartsQuantity;
            report.NokPartsQuantity = model.NokPartsQuantity;
            report.ReworkPartsQuantity = model.ReworkPartsQuantity;

            report.Approved = model.Approved;
            if (report.Approved)
            {
                report.ApprovedOn = DateTime.Now;
                PlusTotalCheckedPartsQuantity(report);
            }

            _reportService.UpdateReport(report);

            //activity log
            _userActivityService.InsertActivity("EditReport", _localizationService.GetResource("ActivityLog.EditReport"), model.Id);

            return new NullJsonResult();
        }

        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            var report = _reportService.GetReportById(id);
            if (report == null)
                throw new ArgumentException("No report found with the specified id");

            _reportService.DeleteReport(report);

            if (report.Approved)
            {
                MinusTotalCheckedPartsQuantity(report);
            }

            //activity log
            _userActivityService.InsertActivity("DeleteReport", _localizationService.GetResource("ActivityLog.DeleteReport"), report.Id);

            return new NullJsonResult();
        }
        #endregion

        #region Final Report List
        public virtual ActionResult FinalReportList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            var model = new FinalReportListModel();
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult FinalReportList(DataSourceRequest command, FinalReportListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var listApprovedReports = new List<Report>();

            foreach (var stateProvince in _workContext.CurrentUser.ManufacturerRegions)
            {
                var customers =
                    _customerService.GetAllCustomers(countryId: stateProvince.CountryId, stateId: stateProvince.Id);

                foreach (var cus in customers)
                {
                    var orders = _orderService.GetAllCustomerOrders(customerId: cus.Id);

                    foreach (var order in orders)
                    {
                        var reports = _reportService
                            .GetAllReports(
                                isAprroved: 1,
                                orderId: order.Id,
                                fromDate: model.CreatedOnFrom,
                                toDate: model.CreatedOnTo);

                        listApprovedReports.AddRange(reports);
                    }
                }
            }

            foreach (var man in _workContext.CurrentUser.Manufacturers)
            {
                var customers = _customerService.GetCustomersByManufacturer(man.Id);
                foreach (var cus in customers)
                {
                    var orders = _orderService.GetAllCustomerOrders(customerId: cus.Id);

                    foreach (var order in orders)
                    {
                        var reports = _reportService
                            .GetAllReports(
                                isAprroved: 1,
                                orderId: order.Id,
                                fromDate: model.CreatedOnFrom,
                                toDate: model.CreatedOnTo);

                        listApprovedReports.AddRange(reports);
                    }
                }
            }

            var rm = new List<FinalReportModel>();

            if (listApprovedReports.Any())
            {
                var groupReports = listApprovedReports.GroupBy(x => x.Order).Select(x => x.ToList()).ToList();

                foreach (var reports in groupReports)
                {
                    var report = reports.FirstOrDefault();
                    if (report == null) continue;

                    var parts = _partService.GetAllOrderParts(report.OrderId);
                    var frm = new FinalReportModel
                    {
                        Id = report.OrderId,
                        Customer = report.Order.Customer.Name,
                        OrderNumber = report.Order.Number,
                        QuantityToCheck = report.Order.TotalPartsQuantity,
                        TypeOfReport = "Final Report",
                        TotalOk = reports.Select(x => x.OkPartsQuantity).Sum(),
                        TotalNok = reports.Select(x => x.NokPartsQuantity).Sum(),
                        TotalReworked = reports.Select(x => x.ReworkPartsQuantity).Sum(),
                        OrderQuantity = report.Order.TotalPartsQuantity
                    };

                    frm.TotalBlocked = frm.TotalNok + frm.TotalReworked;
                    frm.TotalChecked = frm.TotalOk + frm.TotalBlocked;
                    frm.NokPercentage = Math.Round((decimal)frm.TotalNok / frm.TotalChecked, 5) * 100;

                    foreach (var part in parts)
                    {
                        frm.PartNumber += $"{part.SerNumber} ";
                        frm.PartDescription += $"{part.Name} ";
                    }

                    rm.Add(frm);
                }
            }

            var result = new PagedList<FinalReportModel>(rm, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = result.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult DailyFinalReport(int Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var model = new DailyReportModelList
            {
                OrderId = Id
            };

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("download-pdf")]
        public virtual ActionResult DownloadReportAsPdf(DataSourceRequest command, DailyReportModelList model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            return null;
        }

        [HttpGet, ActionName("ExportToExcel")]
        public virtual ActionResult DownloadReportAsExcel(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();


            var dfrs = GetDailyReportData(orderId).ToList();

            var result = dfrs.OrderBy(x => x.DateOfInspection?.Date);

            var rowDailyCount = result.Count();
            var order = _orderService.GetOrderById(orderId);
            var criteriaBlocked = _criteriaService.GetAllCriteriaValues(orderId).Where(x => x.CriteriaType == CriteriaType.BlockedParts).OrderBy(x => x.Id).ToList();
            var criteriaReworked = _criteriaService.GetAllCriteriaValues(orderId).Where(x => x.CriteriaType == CriteriaType.ReworkParts).OrderBy(x => x.Id).ToList();

            using (var ms = new MemoryStream())
            {
                ExcelPackage pck = new ExcelPackage();
                var ws = pck.Workbook.Worksheets.Add("100% Report");

                using (ExcelRange rng = ws.Cells["F6:Q11"])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                using (ExcelRange rng = ws.Cells["A15:Y" + (rowDailyCount + 16)])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using (ExcelRange rng = ws.Cells["A15:Y16"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (ExcelRange rng = ws.Cells["A17:Y" + (rowDailyCount + 16)])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (ExcelRange rng = ws.Cells["A15:Y" + (rowDailyCount + 16)])
                {

                    rng.Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }


                using (ExcelRange rng = ws.Cells[string.Format("D{0}:G{1}", rowDailyCount + 19, rowDailyCount + 19 + 11)])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                }

                using (ExcelRange rng = ws.Cells[string.Format("D{0}:G{1}", rowDailyCount + 19, rowDailyCount + 19 + 11)])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (ExcelRange rng = ws.Cells[string.Format("D{0}:G{1}", rowDailyCount + 19 + 8, rowDailyCount + 19 + 11)])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }


                using (ExcelRange rng = ws.Cells[string.Format("T{0}:T{1}", rowDailyCount + 19, rowDailyCount + 19 + 1)])
                {
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                using (ExcelRange rng = ws.Cells[string.Format("V{0}:Y{1}", rowDailyCount + 19, rowDailyCount + 19 + 1)])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                }

                ws.Cells[string.Format("T{0}", rowDailyCount + 19)].Value = "Quantities";
                ws.Cells[string.Format("T{0}", rowDailyCount + 20)].Value = "Percentages";

                ws.Cells[string.Format("U{0}", rowDailyCount + 18)].Value = "Total checked";
                ws.Cells[string.Format("V{0}", rowDailyCount + 18)].Value = "Total OK";
                ws.Cells[string.Format("W{0}", rowDailyCount + 18)].Value = "Total Blocked";
                ws.Cells[string.Format("X{0}", rowDailyCount + 18)].Value = "Total NOK";
                ws.Cells[string.Format("Y{0}", rowDailyCount + 18)].Value = "Total Reworked";

                ws.Cells[string.Format("U{0}", rowDailyCount + 19)].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                ws.Cells[string.Format("U{0}", rowDailyCount + 19)].Formula = string.Format("=SUM(F17:F{0})", rowDailyCount + 16);
                ws.Cells[string.Format("V{0}", rowDailyCount + 19)].Formula = string.Format("=SUM(G17:G{0})", rowDailyCount + 16);
                ws.Cells[string.Format("W{0}", rowDailyCount + 19)].Formula = string.Format("=SUM(H17:H{0})", rowDailyCount + 16);
                ws.Cells[string.Format("X{0}", rowDailyCount + 19)].Formula = string.Format("=SUM(I17:I{0})", rowDailyCount + 16);
                ws.Cells[string.Format("Y{0}", rowDailyCount + 19)].Formula = string.Format("=SUM(T17:T{0})", rowDailyCount + 16);


                ws.Cells[string.Format("V{0}", rowDailyCount + 20)].Formula = string.Format("=V{0}/U{0}", rowDailyCount + 19);
                ws.Cells[string.Format("V{0}", rowDailyCount + 20)].Style.Numberformat.Format = "#0.00%";

                ws.Cells[string.Format("W{0}", rowDailyCount + 20)].Formula = string.Format("=W{0}/U{0}", rowDailyCount + 19);
                ws.Cells[string.Format("W{0}", rowDailyCount + 20)].Style.Numberformat.Format = "#0.00%";

                ws.Cells[string.Format("X{0}", rowDailyCount + 20)].Formula = string.Format("=X{0}/U{0}", rowDailyCount + 19);
                ws.Cells[string.Format("X{0}", rowDailyCount + 20)].Style.Numberformat.Format = "#0.00%";

                ws.Cells[string.Format("Y{0}", rowDailyCount + 20)].Formula = string.Format("=Y{0}/U{0}", rowDailyCount + 19);
                ws.Cells[string.Format("Y{0}", rowDailyCount + 20)].Style.Numberformat.Format = "#0.00%";


                using (ExcelRange rng = ws.Cells[string.Format("T{0}:Y{1}", rowDailyCount + 18, rowDailyCount + 18 + 2)])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 10;
                    rng.Style.WrapText = true;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }


                ws.Column(1).Width = 3.8;
                ws.Column(2).Width = 14.5;
                ws.Column(6).Width = 14.5;
                ws.Column(7).Width = 14.5;
                ws.Column(8).Width = 6.5;
                ws.Column(9).Width = 6.5;

                for (int i = 3; i <= 5; i++)
                    ws.Column(i).Width = 9.4;

                for (int i = 10; i <= 17; i++)
                    ws.Column(i).Width = 5.5;

                for (int i = 19; i <= 24; i++)
                    ws.Column(i).Width = 13;

                ws.Column(18).Width = 26;
                ws.Column(25).Width = 14.2;

                ws.Row(15).Height = 30;
                ws.Row(16).Height = 30;

                ws.Cells["A15"].Value = "No";
                ws.Cells["A16"].Value = "Номер";

                ws.Cells["B15"].Value = "Date of inspection";
                ws.Cells["B16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DateOfInspection").ResourceValue;

                ws.Cells["C15"].Value = "Part number";
                ws.Cells["C16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.PartNumber").ResourceValue;

                ws.Cells["D15"].Value = "Delivery №";
                ws.Cells["D16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DeliveryNumber").ResourceValue;


                ws.Cells["E15"].Value = "Charge №";
                ws.Cells["E16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ChargeNumber").ResourceValue;

                ws.Cells["F15"].Value = "Quantity";
                ws.Cells["F16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.Quantity").ResourceValue;

                ws.Cells["G15"].Value = "First run OK parts";
                ws.Cells["G16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.FirstRunOkParts").ResourceValue;


                ws.Cells["H15"].Value = "Blocked parts";
                ws.Cells["H16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.BlockedParts").ResourceValue;

                ws.Cells["I15"].Value = "NOK parts";
                ws.Cells["I16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.NokParts").ResourceValue;


                ws.Cells["J15:Q15"].Merge = true;
                using (ExcelRange rng = ws.Cells["J15:Q15"])
                {
                    rng.Value = "Due to   (Razlog)";
                }

                ws.Cells["J16"].Value = "1";
                ws.Cells["K16"].Value = "2";
                ws.Cells["L16"].Value = "3";
                ws.Cells["M16"].Value = "4";
                ws.Cells["N16"].Value = "5";
                ws.Cells["O16"].Value = "6";
                ws.Cells["P16"].Value = "7";
                ws.Cells["Q16"].Value = "8";

                ws.Cells["R15"].Value = "Description of blocked parts";
                ws.Cells["R16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfBlockedParts").ResourceValue;



                ws.Cells["S15"].Value = "NOK percentage";
                ws.Cells["S16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.NokPercentage").ResourceValue;

                ws.Cells["T15"].Value = "Reworked parts";
                ws.Cells["T16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ReworkedParts").ResourceValue;


                ws.Cells["U15:X15"].Merge = true;
                using (ExcelRange rng = ws.Cells["U15:X15"])
                {
                    rng.Value = "Due to";
                }

                ws.Cells["U16"].Value = "9";
                ws.Cells["V16"].Value = "10";
                ws.Cells["W16"].Value = "11";
                ws.Cells["X16"].Value = "12";


                ws.Cells["Y15"].Value = "Rework percentage";
                ws.Cells["Y16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ReworkedPercentage").ResourceValue;



                var num = 17;
                foreach (var item in result)
                {
                    ws.Cells["A" + num].Value = num;
                    ws.Cells["B" + num].Value = item.DateOfInspection.HasValue ? item.DateOfInspection.Value.Date : (object)null;
                    ws.Cells["B" + num].Style.Numberformat.Format = "[$-409]DD.MMM.YY;@";
                    ws.Cells["C" + num].Value = item.PartNumber;
                    ws.Cells["D" + num].Value = item.DeliveryNumber;
                    ws.Cells["E" + num].Value = item.ChargeNumber;
                    ws.Cells["F" + num].Value = item.Quantity;
                    ws.Cells["G" + num].Value = item.FirstRunOkParts;
                    ws.Cells["H" + num].Value = item.BlockedParts;
                    ws.Cells["I" + num].Value = item.NokParts;
                    ws.Cells["J" + num].Value = item.Dod1;
                    ws.Cells["K" + num].Value = item.Dod2;
                    ws.Cells["L" + num].Value = item.Dod3;
                    ws.Cells["M" + num].Value = item.Dod4;
                    ws.Cells["N" + num].Value = item.Dod5;
                    ws.Cells["O" + num].Value = item.Dod6;
                    ws.Cells["P" + num].Value = item.Dod7;
                    ws.Cells["Q" + num].Value = item.Dod8;

                    ws.Cells["R" + num].Formula = string.Format(@"=I{0}/F{0}", num);
                    ws.Cells["R" + num].Style.Numberformat.Format = "#0.00%";

                    ws.Cells["T" + num].Value = item.ReworkedParts;
                    ws.Cells["U" + num].Value = item.Dor1;
                    ws.Cells["V" + num].Value = item.Dor2;
                    ws.Cells["W" + num].Value = item.Dor3;
                    ws.Cells["X" + num].Value = item.Dor4;
                    ws.Cells["Y" + num].Formula = string.Format(@"=T{0}/F{0}", num);
                    ws.Cells["Y" + num].Style.Numberformat.Format = "#0.00%";


                    num++;
                }

                ws.Cells["R17:R" + (rowDailyCount + 16)].Merge = true;
                using (ExcelRange rng = ws.Cells["R17:R" + (rowDailyCount + 16)])
                {
                    var j = 1;
                    var allCriterias = _criteriaService.GetAllCriteriaValues(orderId).OrderBy(x => x.Id).ToList().Select(s => j++ + ". " + s.Description).ToList();
                    rng.Value = string.Join("\r\n", allCriterias);
                }

                var cellName = string.Format("B{0}:C{1}", rowDailyCount + 19, rowDailyCount + 19);
                ws.Cells[cellName].Merge = true;
                using (ExcelRange rng = ws.Cells[cellName])
                {
                    rng.Value = "Description of defects:";
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                }

                cellName = string.Format("B{0}:C{1}", rowDailyCount + 19 + 1, rowDailyCount + 19 + 1);
                ws.Cells[cellName].Merge = true;
                using (ExcelRange rng = ws.Cells[cellName])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfDefects").ResourceValue;
                }

                cellName = string.Format("B{0}:C{1}", rowDailyCount + 19 + 8, rowDailyCount + 19 + 8);
                ws.Cells[cellName].Merge = true;
                using (ExcelRange rng = ws.Cells[cellName])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Value = "Description of rework:";
                }

                cellName = string.Format("B{0}:C{1}", rowDailyCount + 19 + 8 + 1, rowDailyCount + 19 + 8 + 1);
                ws.Cells[cellName].Merge = true;
                using (ExcelRange rng = ws.Cells[cellName])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfRework").ResourceValue;
                }

                for (int i = 0; i < 12; i++)
                {
                    ws.Row(rowDailyCount + 19 + i).Height = 13;
                }

                for (int i = 0; i < 8; i++)
                {
                    ws.Cells["D" + (rowDailyCount + 19 + i)].Value = i + 1;

                    var cn = string.Format("E{0}:G{0}", rowDailyCount + 19 + i);
                    ws.Cells[cn].Merge = true;
                    using (ExcelRange rng = ws.Cells[cn])
                    {
                        rng.Value = criteriaBlocked.Count > i ? criteriaBlocked[i].Description : "";
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    ws.Cells["D" + (rowDailyCount + 19 + 8 + i)].Value = i + 1 + 8;

                    var cn = string.Format("E{0}:G{0}", rowDailyCount + 19 + 8 + i);
                    ws.Cells[cn].Merge = true;
                    using (ExcelRange rng = ws.Cells[cn])
                    {
                        rng.Value = criteriaReworked.Count > i ? criteriaReworked[i].Description : "";
                    }
                }


                //Header

                //logo
                var image = Image.FromFile(HostingEnvironment.MapPath("~/Administration/Content/images/invenio.jpg"));
                var picture = ws.Drawings.AddPicture("logo", image);
                picture.SetSize(40);
                picture.SetPosition(1, 0, 1, 5);


                ws.Cells["H4"].Value = "SORTING & REWORK REPORT";
                ws.Cells["H4"].Style.Font.Size = 16;
                ws.Cells["H4"].Style.Font.Bold = true;

                ws.Cells["F6:G6"].Merge = true;
                using (ExcelRange rng = ws.Cells["F6:G6"])
                {
                    rng.Value = "SUPPLIER :";
                    rng.Style.Font.Bold = true;
                }
                ws.Cells["F7:G7"].Merge = true;
                using (ExcelRange rng = ws.Cells["F7:G7"])
                {
                    rng.Value = "PART NUMBER :";
                }
                ws.Cells["F8:G8"].Merge = true;
                using (ExcelRange rng = ws.Cells["F8:G8"])
                {
                    rng.Value = "PART DESCRIPTION :";
                }
                ws.Cells["F9:G9"].Merge = true;
                using (ExcelRange rng = ws.Cells["F9:G9"])
                {
                    rng.Value = "ORDER NO :";
                }
                ws.Cells["F10:G10"].Merge = true;
                using (ExcelRange rng = ws.Cells["F10:G10"])
                {
                    rng.Value = "QUANTITY TO CHECK: ";
                }
                ws.Cells["F11:G11"].Merge = true;
                using (ExcelRange rng = ws.Cells["F11:G11"])
                {
                    rng.Value = "TYPE OF REPORT :";
                }
                //------
                ws.Cells["H6:Q6"].Merge = true;
                using (ExcelRange rng = ws.Cells["H6:Q6"])
                {
                    rng.Value = order.Customer.Name;
                }
                ws.Cells["H7:Q7"].Merge = true;
                using (ExcelRange rng = ws.Cells["H7:Q7"])
                {
                    rng.Value = string.Join(", ", dfrs.Select(s => s.PartNumber).ToList());
                }
                ws.Cells["H8:Q8"].Merge = true;
                using (ExcelRange rng = ws.Cells["H8:Q8"])
                {
                    rng.Value = "";
                }
                ws.Cells["H9:Q9"].Merge = true;
                using (ExcelRange rng = ws.Cells["H9:Q9"])
                {
                    rng.Value = order.Number;
                }
                ws.Cells["H10:Q10"].Merge = true;
                using (ExcelRange rng = ws.Cells["H10:Q10"])
                {
                    rng.Value = order.CheckedPartsQuantity;
                }
                ws.Cells["H11:Q11"].Merge = true;
                using (ExcelRange rng = ws.Cells["H11:Q11"])
                {
                    rng.Value = "Final Report";
                }

                using (ExcelRange rng = ws.Cells["F6:Q11"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }
                using (ExcelRange rng = ws.Cells["F6:Q6"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }


                pck.SaveAs(ms);

                ms.Position = 0;
                return File(ms.ToArray(), MimeTypes.ApplicationXlsx, "report.xlsx");
            }
        }

        [HttpPost]
        public virtual ActionResult DailyFinalReport(DataSourceRequest command, DailyReportModelList model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var dfrs = GetDailyReportData(model.OrderId);

            var result = new PagedList<DailyReportModel>(dfrs, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = result.OrderBy(x => x.DateOfInspection?.Date),
                Total = result.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult BlockedReportFinalList(DataSourceRequest command, DailyReportModelList model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var blockedReports = _criteriaService
                .GetAllCriteriaValues(model.OrderId)
                .Where(x => x.CriteriaType == CriteriaType.BlockedParts)
                .OrderBy(x => x.Id)
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = blockedReports,
                Total = blockedReports.Count
            };

            return Json(gridModel);
        }

        public virtual ActionResult ReworkedReportFinalList(DataSourceRequest command, DailyReportModelList model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var reworkedReports = _criteriaService
                .GetAllCriteriaValues(model.OrderId)
                .Where(x => x.CriteriaType == CriteriaType.ReworkParts)
                .OrderBy(x => x.Id)
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = reworkedReports,
                Total = reworkedReports.Count
            };

            return Json(gridModel);
        }
        #endregion

        #region Total Final Report
        [HttpPost]
        public ActionResult TotalOrderReportData(DataSourceRequest command, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var model = new List<ReportTotalModel>();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return new NullJsonResult();

            var reports = _reportService.GetAllReports(orderId: orderId).Where(x => x.Approved).ToList();
            var totalNok = reports.Select(x => x.NokPartsQuantity).Sum();
            var totalChecked =
                reports.Select(x => x.NokPartsQuantity).Sum()
                + reports.Select(x => x.ReworkPartsQuantity).Sum()
                + reports.Select(x => x.OkPartsQuantity).Sum();

            model.Add(new ReportTotalModel
            {
                TotalChecked = totalChecked,
                TotalBlocked = reports.Select(x => x.NokPartsQuantity).Sum() +
                                   reports.Select(x => x.ReworkPartsQuantity).Sum(),
                TotalReworked = reports.Select(x => x.ReworkPartsQuantity).Sum(),
                TotalNok = totalNok,
                NokPercentage = Math.Round(totalNok / (decimal)totalChecked, 5) * 100
            });

            var result = new PagedList<ReportTotalModel>(model, command.Page - 1, command.Page);
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = result.TotalCount
            };

            return Json(gridModel);
        }


        private List<DailyReportModel> GetDailyReportData(int orderId)
        {
            var reports = _reportService.GetAllReports(orderId: orderId, isAprroved: 1);

            var dfrs = reports.GroupBy(c => new
            {
                c.DateOfInspection?.Date,
                Part = _partService.GetPartById(c.PartId ?? 0)?.SerNumber,
                ChargeNumber = c.ChargeNumberId.HasValue
                ? _chargeNumberService.GetChargeNumberById(c.ChargeNumberId.Value)
                : null,
                DeliveryNumber = c.DeliveryNumberId.HasValue
                    ? _deliveryNumberService.GetDeliveryNumberById(c.DeliveryNumberId.Value)
                    : null,
            })
            .Select(x => new DailyReportModel
            {
                DateOfInspection = x.Key.Date?.Date,
                ChargeNumber = x.Key.ChargeNumber?.Number,
                DeliveryNumber = x.Key.DeliveryNumber?.Number,
                PartNumber = x.Key.Part,

            })
            .OrderBy(x => x.Id)
            .ToList();

            foreach (var dfr in dfrs)
            {
                var newTest = reports.Where(x => x.DateOfInspection?.Date == dfr.DateOfInspection?.Date
                                                 && x.ChargeNumber?.Number == dfr.ChargeNumber
                                                 && x.DeliveryNumber?.Number == dfr.DeliveryNumber
                                                 && x.Part?.SerNumber == dfr.PartNumber).ToList();

                dfr.NokParts = newTest.Sum(x => x.NokPartsQuantity);
                dfr.ReworkedParts = newTest.Sum(x => x.ReworkPartsQuantity);
                dfr.FirstRunOkParts = newTest.Sum(x => x.OkPartsQuantity);
                dfr.BlockedParts = newTest.Sum(x => x.ReworkPartsQuantity) + newTest.Sum(x => x.NokPartsQuantity);
                dfr.Quantity = newTest.Sum(x => x.OkPartsQuantity) + newTest.Sum(x => x.NokPartsQuantity) + newTest.Sum(x => x.ReworkPartsQuantity);
            }

            dfrs.ForEach(x => x.NokPercentage = Math.Round((decimal)x.NokParts / x.Quantity, 5) * 100);
            dfrs.ForEach(x => x.ReworkedPercentage = Math.Round((decimal)x.ReworkedParts / x.Quantity, 5) * 100);

            //blocked
            var repDetails = _reportDetailService.GetReportDetailsByOrderId(orderId, true);

            var blokedRepDetails = repDetails
                .GroupBy(x => new
                {
                    DateOfInspection = x.Report.DateOfInspection,
                    Criteria = _criteriaService.GetCriteriaById(x.CriteriaId),
                    Quantity = x.Quantity,
                    ChargeNumber = x.Report.ChargeNumber?.Number,
                    DeliveryNumber = x.Report.DeliveryNumber?.Number
                })
                .Select(x => new ReportDetailsModel
                {
                    CriteriaId = x.Key.Criteria.Id,
                    Quantity = x.Key.Quantity,
                    CriteriaType = x.Key.Criteria.CriteriaType,
                    DateOfInspection = x.Key.DateOfInspection,
                    ChargeNumber = x.Key.ChargeNumber,
                    DeliveryNumber = x.Key.DeliveryNumber
                })
                .Where(x => _criteriaService.GetCriteriaById(x.CriteriaId).CriteriaType == CriteriaType.BlockedParts).ToList();

            var criteriaBlocked = _criteriaService.GetAllCriteriaValues(orderId).Where(x => x.CriteriaType == CriteriaType.BlockedParts).OrderBy(x => x.Id).ToList();

            BindingFlags bindingFlags = BindingFlags.Public |
                                        BindingFlags.NonPublic |
                                        BindingFlags.Instance |
                                        BindingFlags.Static;

            var t = 1;
            foreach (var criteria in criteriaBlocked)
            {
                foreach (var dailyReportModel in dfrs)
                {
                    var quantity = blokedRepDetails
                        .Where(x => x.DateOfInspection == dailyReportModel.DateOfInspection
                                    && x.CriteriaId == criteria.Id
                                    && x.DeliveryNumber == dailyReportModel.DeliveryNumber
                                    && x.ChargeNumber == dailyReportModel.ChargeNumber)
                        .Sum(x => x.Quantity);

                    if (quantity == 0)
                        continue;

                    var entity = dfrs
                        .FirstOrDefault(x => x.DateOfInspection == dailyReportModel.DateOfInspection
                             && x.PartNumber == dailyReportModel.PartNumber
                             && x.DeliveryNumber == dailyReportModel.DeliveryNumber
                             && x.ChargeNumber == dailyReportModel.ChargeNumber);

                    if (entity == null)
                        continue;

                    var fieldIndex = t;
                    var field = entity.GetType().GetFields(bindingFlags).FirstOrDefault(x => x.Name.Contains("Dod" + fieldIndex.ToString()));
                    if (field == null) continue;
                    field.SetValue(entity, quantity);
                }
                t++;
            }

            var reworkedRepDetails = repDetails
                .GroupBy(x => new
                {
                    DateOfInspection = x.Report.DateOfInspection,
                    Criteria = _criteriaService.GetCriteriaById(x.CriteriaId),
                    Quantity = x.Quantity,
                    ChargeNumber = x.Report.ChargeNumber?.Number,
                    DeliveryNumber = x.Report.DeliveryNumber?.Number
                })
                .Select(x => new ReportDetailsModel
                {
                    CriteriaId = x.Key.Criteria.Id,
                    Quantity = x.Key.Quantity,
                    CriteriaType = x.Key.Criteria.CriteriaType,
                    DateOfInspection = x.Key.DateOfInspection,
                    ChargeNumber = x.Key.ChargeNumber,
                    DeliveryNumber = x.Key.DeliveryNumber
                })
                .Where(x => _criteriaService.GetCriteriaById(x.CriteriaId).CriteriaType == CriteriaType.ReworkParts).ToList();

            var criteriaReworked = _criteriaService.GetAllCriteriaValues(orderId).Where(x => x.CriteriaType == CriteriaType.ReworkParts).OrderBy(x => x.Id).ToList();

            var y = 1;
            foreach (var criteria in criteriaReworked)
            {
                foreach (var dailyReportModel in dfrs)
                {
                    var quantity = reworkedRepDetails
                        .Where(x => x.DateOfInspection == dailyReportModel.DateOfInspection
                                    && x.CriteriaId == criteria.Id
                                    && x.DeliveryNumber == dailyReportModel.DeliveryNumber
                                    && x.ChargeNumber == dailyReportModel.ChargeNumber)
                        .Sum(x => x.Quantity);

                    if (quantity == 0)
                        continue;

                    var entity = dfrs
                        .FirstOrDefault(x => x.DateOfInspection == dailyReportModel.DateOfInspection
                                             && x.PartNumber == dailyReportModel.PartNumber
                                             && x.DeliveryNumber == dailyReportModel.DeliveryNumber
                                             && x.ChargeNumber == dailyReportModel.ChargeNumber);

                    if (entity == null)
                        continue;

                    var fieldIndex = y;
                    var field = entity.GetType().GetFields(bindingFlags).FirstOrDefault(x => x.Name.Contains("Dor" + fieldIndex.ToString()));
                    if (field == null) continue;
                    field.SetValue(entity, quantity);
                }
                y++;
            }

            return dfrs;
        }
        #endregion
    }
}