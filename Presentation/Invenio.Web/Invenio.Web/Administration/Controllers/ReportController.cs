using System;
using System.Collections.Generic;
using System.IO;
using Invenio.Admin.Models.Report;
using Invenio.Services.Security;
using Invenio.Web.Framework.Kendoui;
using System.Web.Mvc;
using Invenio.Services.Helpers;
using Invenio.Services.Reports;
using System.Linq;
using System.Reflection;
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

            var result = dfrs.OrderBy(x => x.DateOfInspection?.Date);

            using (var ms = new MemoryStream())
            {
                ExcelPackage pck = new ExcelPackage();
                var ws = pck.Workbook.Worksheets.Add("ExportPage");

                ws.Cells["A1"].Value = "Sample 1";
                ws.Cells["A1"].Style.Font.Bold = true;


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

            var reports = _reportService.GetAllReports(orderId: model.Id, isAprroved: 1);

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
            var repDetails = _reportDetailService.GetReportDetailsByOrderId(model.OrderId, true);

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

            var criteriaBlocked = _criteriaService.GetAllCriteriaValues(model.OrderId).Where(x => x.CriteriaType == CriteriaType.BlockedParts).OrderBy(x => x.Id).ToList();

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

            var criteriaReworked = _criteriaService.GetAllCriteriaValues(model.OrderId).Where(x => x.CriteriaType == CriteriaType.ReworkParts).OrderBy(x => x.Id).ToList();

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
        #endregion
    }
}