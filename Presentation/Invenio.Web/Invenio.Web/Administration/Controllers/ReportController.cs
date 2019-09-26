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
using System.Web.Hosting;
using Invenio.Admin.Extensions;
using Invenio.Core;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Reports;
using Invenio.Services.Supplier;
using Invenio.Services.Events;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Orders;
using Invenio.Services.Users;
using Invenio.Web.Framework.Mvc;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Suppliers;
using Invenio.Services.ChargeNumber;
using Invenio.Services.Common;
using Invenio.Services.Criteria;
using Invenio.Services.Parts;
using Invenio.Services.DeliveryNumber;
using Invenio.Web.Framework.Controllers;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.Style;
using Image = System.Drawing.Image;
using Invenio.Core.Domain.Users;
using Invenio.Services.Customers;

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
        private readonly ISupplierService _supplierService;
        private readonly IPartService _partService;
        private readonly IReportDetailService _reportDetailService;
        private readonly IChargeNumberService _chargeNumberService;
        private readonly IDeliveryNumberService _deliveryNumberService;
        private readonly ICriteriaService _criteriaService;
        private readonly IPdfService _pdfService;
        private readonly ICustomerService _customerService;

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
            ISupplierService supplierService,
            IPartService partService,
            IReportDetailService reportDetailService,
            IChargeNumberService chargeNumberService,
            IDeliveryNumberService deliveryNumberService,
            ICriteriaService criteriaService,
            IPdfService pdfService,
            ICustomerService customerService)
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
            _supplierService = supplierService;
            _partService = partService;
            _reportDetailService = reportDetailService;
            _chargeNumberService = chargeNumberService;
            _deliveryNumberService = deliveryNumberService;
            _criteriaService = criteriaService;
            _pdfService = pdfService;
            _customerService = customerService;
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

            model.AvailableWorkShifts.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Work.Shift"), Value = "0" });
            model.AvailableWorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShift.FirstShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShift.FirstShift).ToString()
                });

            model.AvailableWorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShift.SecondShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShift.SecondShift).ToString()
                });

            model.AvailableWorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShift.RegularShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShift.RegularShift).ToString()
                });

            model.AvailableWorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShift.NightShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShift.NightShift).ToString()
                });


            model.AvailableSuppliers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.All"), Value = "0" });
            model.AvailableOrders.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.All"), Value = "0" });

            if (_workContext.CurrentUser.IsAdmin())
            {
                var suppliers = _supplierService.GetAllSuppliers();
                suppliers.ToList().ForEach(x =>
                    model.AvailableSuppliers.Add(new SelectListItem { Text = x.Name, Value = x.Id.ToString() }));
            }
            else
            {
                var suppliers = _supplierService.GetAllSuppliers();

                var filtredSuppliers = new List<Supplier>();
                if (_workContext.CurrentUser.CustomerRegions.Any())
                {
                    foreach (var region in _workContext.CurrentUser.CustomerRegions)
                    {
                        var customers =
                            _customerService.GetAllCustomers(countryId: region.CountryId, stateId: region.Id);

                        foreach (var customer in customers)
                        {
                            filtredSuppliers.AddRange(suppliers.Where(x => x.Customer == customer));
                        }
                    }
                }

                if (_workContext.CurrentUser.Customers.Any())
                {
                    foreach (var customer in _workContext.CurrentUser.Customers)
                    {
                        filtredSuppliers.AddRange(suppliers.Where(x => x.Customer == customer));
                    }
                }

                filtredSuppliers = filtredSuppliers.Distinct().ToList();
                filtredSuppliers.ForEach(x =>
                    model.AvailableSuppliers.Add(new SelectListItem { Text = x.Name, Value = x.Id.ToString() }));
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, ReportListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var rm = new List<ReportModel>();

            if (_workContext.CurrentUser.IsAdmin())
            {
                var reports = _reportService
                                .GetAllReports(
                                isAprroved: model.SearchApprovedId,
                                fromDate: model.CreatedOnFrom,
                                toDate: model.CreatedOnTo)
                                .ToList();

                if (!string.IsNullOrEmpty(model.UserName))
                    reports = reports.Where(x => _userService.GetUserById(x.UserId).GetFullName().ToLower().Contains(model.UserName.ToLower())).ToList();

                if (model.WorkShiftId > 0)
                    reports = reports.Where(x => x.WorkShift == (WorkShift)model.WorkShiftId).ToList();

                if (model.SupplierId > 0)
                    reports = reports.Where(x => _orderService.GetOrderById(x.OrderId).SupplierId == model.SupplierId).ToList();

                if (model.OrderId > 0)
                    reports = reports.Where(x => x.OrderId == model.OrderId).ToList();

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
                        r.SupplierName = order.Supplier.Name;
                        r.WorkShiftName = ((WorkShift)r.WorkShift).GetLocalizedEnum(_localizationService, _workContext);

                        return r;
                    }));
                }
            }
            else
            {
                foreach (var stateProvince in _workContext.CurrentUser.CustomerRegions)
                {
                    var reports = _reportService
                        .GetAllReports(
                        regionId: stateProvince.Id,
                        isAprroved: model.SearchApprovedId,
                        fromDate: model.CreatedOnFrom,
                        toDate: model.CreatedOnTo)
                        .ToList();

                    if (!string.IsNullOrEmpty(model.UserName))
                        reports = reports.Where(x => _userService.GetUserById(x.UserId).GetFullName().ToLower().Contains(model.UserName.ToLower())).ToList();

                    if (model.WorkShiftId > 0)
                        reports = reports.Where(x => x.WorkShift == (WorkShift)model.WorkShiftId).ToList();

                    if (model.SupplierId > 0)
                        reports = reports.Where(x => x.Order.SupplierId == model.SupplierId).ToList();

                    if (model.OrderId > 0)
                        reports = reports.Where(x => x.OrderId == model.OrderId).ToList();

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
                            r.SupplierName = order.Supplier.Name;
                            r.WorkShiftName = ((WorkShift)r.WorkShift).GetLocalizedEnum(_localizationService, _workContext);

                            return r;
                        }));
                    }
                }

                foreach (var man in _workContext.CurrentUser.Customers)
                {
                    var reports = _reportService
                        .GetAllReports(CustomerId: man.Id,
                        isAprroved: model.SearchApprovedId,
                        fromDate: model.CreatedOnFrom,
                        toDate: model.CreatedOnTo)
                        .ToList();

                    if (!string.IsNullOrEmpty(model.UserName))
                        reports = reports.Where(x => _userService.GetUserById(x.UserId).GetFullName().ToLower().Contains(model.UserName.ToLower())).ToList();

                    if (model.WorkShiftId > 0)
                        reports = reports.Where(x => x.WorkShift == (WorkShift)model.WorkShiftId).ToList();

                    if (model.SupplierId > 0)
                        reports = reports.Where(x => x.Order.SupplierId == model.SupplierId).ToList();

                    if (model.OrderId > 0)
                        reports = reports.Where(x => x.OrderId == model.OrderId).ToList();

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
                            r.SupplierName = order.Supplier.Name;
                            r.WorkShiftName = ((WorkShift)r.WorkShift).GetLocalizedEnum(_localizationService, _workContext);

                            return r;
                        }));
                    }
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
        public virtual ActionResult ListDetails(int reportId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            if (reportId == 0)
                return new NullJsonResult();

            var reportDetails = _reportDetailService.GetReportDetailsByReportId(reportId);

            var result = (from rd in reportDetails
                          let criteria = _criteriaService.GetCriteriaById(rd.CriteriaId)
                          select new ListDetailsModel
                          {
                              Criteria = criteria.Description,
                              CriteriaQuantity = rd.Quantity
                          }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = result.Count
            };

            return Json(gridModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult GetOrdersBySupplier(string supplierId, bool? addAsterisk)
        {
            //permission validation is not required here


            // This action method gets called via an ajax request
            if (string.IsNullOrEmpty(supplierId))
                throw new ArgumentNullException(nameof(supplierId));

            var supplier = _supplierService.GetSupplierById(Convert.ToInt32(supplierId));
            var states = supplier != null ? _orderService.GetAllSupplierOrders(supplier.Id).ToList() : new List<Order>();
            var result = (from s in states
                          select new { id = s.Id, name = s.Number }).ToList();
            if (addAsterisk.HasValue && addAsterisk.Value)
            {
                //asterisk
                result.Insert(0, new { id = 0, name = "*" });
            }
            else
            {
                if (supplier == null)
                {
                    //customer is not selected ("choose customer" item)
                    result.Insert(0, new { id = 0, name = _localizationService.GetResource("Admin.Report.Supplier.All") });
                }
                else
                {
                    //some country is selected
                    result.Insert(0,
                        !result.Any()
                            ? new { id = 0, name = _localizationService.GetResource("Admin.Report.Supplier.IsEmpty") }
                            : new { id = 0, name = _localizationService.GetResource("Admin.Report.Supplier.SelectSupplier") });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
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

            if (report.Approved != model.Approved && model.Approved)
            {
                report.ApprovedOn = DateTime.Now;
                PlusTotalCheckedPartsQuantity(report);
            }

            if (report.Approved != model.Approved && !model.Approved)
            {
                MinusTotalCheckedPartsQuantity(report);
            }

            report.Approved = model.Approved;
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

            model.AvailableSuppliers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.All"), Value = "0" });
            model.AvailableOrders.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Reports.Option.All"), Value = "0" });

            if (_workContext.CurrentUser.IsAdmin())
            {
                var suppliers = _supplierService.GetAllSuppliers();
                suppliers.ToList().ForEach(x =>
                    model.AvailableSuppliers.Add(new SelectListItem { Text = x.Name, Value = x.Id.ToString() }));
            }
            else
            {
                var suppliers = _supplierService.GetAllSuppliers();

                var filtredSuppliers = new List<Supplier>();
                if (_workContext.CurrentUser.CustomerRegions.Any())
                {
                    foreach (var region in _workContext.CurrentUser.CustomerRegions)
                    {
                        var customers =
                            _customerService.GetAllCustomers(countryId: region.CountryId, stateId: region.Id);

                        foreach (var customer in customers)
                        {
                            filtredSuppliers.AddRange(suppliers.Where(x => x.Customer == customer));
                        }
                    }
                }

                if (_workContext.CurrentUser.Customers.Any())
                {
                    foreach (var customer in _workContext.CurrentUser.Customers)
                    {
                        filtredSuppliers.AddRange(suppliers.Where(x => x.Customer == customer));
                    }
                }

                filtredSuppliers = filtredSuppliers.Distinct().ToList();
                filtredSuppliers.ForEach(x =>
                    model.AvailableSuppliers.Add(new SelectListItem { Text = x.Name, Value = x.Id.ToString() }));
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult FinalReportList(DataSourceRequest command, FinalReportListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedKendoGridJson();

            var listApprovedReports = new List<Report>();

            if (_workContext.CurrentUser.IsAdmin())
            {
                listApprovedReports.AddRange(_reportService.GetAllReports(isAprroved: 1));
            }
            else
            {
                foreach (var stateProvince in _workContext.CurrentUser.CustomerRegions)
                {
                    var suppliers =
                        _supplierService.GetAllSuppliers(countryId: stateProvince.CountryId, stateId: stateProvince.Id);

                    foreach (var cus in suppliers)
                    {
                        var orders = _orderService.GetAllSupplierOrders(supplierId: cus.Id);

                        foreach (var order in orders)
                        {
                            var reports = _reportService
                                .GetAllReports(
                                    isAprroved: 1,
                                    orderId: order.Id);

                            listApprovedReports.AddRange(reports);
                        }
                    }
                }

                foreach (var man in _workContext.CurrentUser.Customers)
                {
                    var suppliers = _supplierService.GetSuppliersByCustomer(man.Id);
                    foreach (var cus in suppliers)
                    {
                        var orders = _orderService.GetAllSupplierOrders(supplierId: cus.Id);

                        foreach (var order in orders)
                        {
                            var reports = _reportService
                                .GetAllReports(
                                    isAprroved: 1,
                                    orderId: order.Id);

                            listApprovedReports.AddRange(reports);
                        }
                    }
                }
            }

            if (model.SupplierId > 0)
                listApprovedReports = listApprovedReports.Where(x =>
                    _orderService.GetOrderById(x.OrderId).SupplierId == model.SupplierId).ToList();

            if (model.OrderId > 0)
                listApprovedReports = listApprovedReports.Where(x => x.OrderId == model.OrderId).ToList();

            var rm = new List<FinalReportModel>();

            if (listApprovedReports.Any())
            {
                var groupReports = listApprovedReports.GroupBy(x => x.Order).Select(x => x.ToList()).ToList();

                foreach (var reports in groupReports)
                {
                    var report = reports.FirstOrDefault();
                    if (report == null) continue;

                    var parts = _partService.GetAllOrderParts(report.OrderId);
                    var order = _orderService.GetOrderById(report.OrderId);
                    var frm = new FinalReportModel
                    {
                        Id = report.OrderId,
                        Supplier = order.Supplier.Name,
                        OrderNumber = order.Number,
                        QuantityToCheck = order.TotalPartsQuantity,
                        TypeOfReport = "Interim Report",
                        TotalOk = reports.Select(x => x.OkPartsQuantity).Sum(),
                        TotalNok = reports.Select(x => x.NokPartsQuantity).Sum(),
                        TotalReworked = reports.Select(x => x.ReworkPartsQuantity).Sum(),
                        OrderQuantity = order.TotalPartsQuantity
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
                TotalOk = reports.Select(x => x.OkPartsQuantity).Sum(),
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

        private DailyReportExportModel PrepareExportData(int orderId)
        {
            var dfrs = GetDailyReportData(orderId).ToList();
            var result = dfrs.OrderBy(x => x.DateOfInspection?.Date);
            var allCreterias = _criteriaService.GetAllCriteriaValues(orderId);
            var order = _orderService.GetOrderById(orderId);

            return new DailyReportExportModel(allCreterias, result, order);
        }
        #endregion

        #region Export
        [HttpGet, ActionName("ExportToPdf")]
        public virtual ActionResult DownloadReportAsPdf(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            var model = PrepareExportData(orderId);

            var html = RenderPartialViewToString("_PdfExportView", model);

            var css = "";
            using (var sr = new StreamReader(Server.MapPath("~/Administration/Content/dailyreportpdf.css")))
                css = sr.ReadToEnd();

            var bytes = _pdfService.PrintDailyReportToPdf(html, css);

            return File(bytes, MimeTypes.ApplicationPdf, $"Interim-Report-{model.OrderNo}.pdf");
        }

        [HttpGet, ActionName("ExportToExcel")]
        public virtual ActionResult DownloadReportAsExcel(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return AccessDeniedView();

            var model = PrepareExportData(orderId);

            var rowDailyCount = model.Items.Count();

            using (var ms = new MemoryStream())
            {
                var pck = new ExcelPackage();
                var ws = pck.Workbook.Worksheets.Add("100% Report");
                ws.View.ZoomScale = 82;

                using (var rng = ws.Cells["F6:Q11"])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                using (var rng = ws.Cells["A15:Y" + (rowDailyCount + 16)])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using (var rng = ws.Cells["A15:Y16"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (var rng = ws.Cells["A17:Y" + (rowDailyCount + 16)])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (var rng = ws.Cells["A15:Y" + (rowDailyCount + 16)])
                {

                    rng.Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }


                using (var rng = ws.Cells[$"D{rowDailyCount + 19}:G{rowDailyCount + 19 + 11}"])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                }

                using (var rng = ws.Cells[$"D{rowDailyCount + 19}:G{rowDailyCount + 19 + 11}"])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (var rng = ws.Cells[$"D{rowDailyCount + 19 + 8}:G{rowDailyCount + 19 + 11}"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }


                using (var rng = ws.Cells[$"T{rowDailyCount + 19}:T{rowDailyCount + 19 + 1}"])
                {
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                using (var rng = ws.Cells[$"V{rowDailyCount + 19}:Y{rowDailyCount + 19 + 1}"])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                }

                using (var rng = ws.Cells[$"U{rowDailyCount + 18}:Y{rowDailyCount + 18}"])
                {
                    rng.Style.Font.Bold = true;
                }

                ws.Cells[$"T{rowDailyCount + 19}"].Value = "Quantities";
                ws.Cells[$"T{rowDailyCount + 20}"].Value = "Percentages";

                ws.Cells[$"U{rowDailyCount + 18}"].Value = "Total checked";
                ws.Cells[$"V{rowDailyCount + 18}"].Value = "Total OK";
                ws.Cells[$"W{rowDailyCount + 18}"].Value = "Total Blocked";
                ws.Cells[$"X{rowDailyCount + 18}"].Value = "Total NOK";
                ws.Cells[$"Y{rowDailyCount + 18}"].Value = "Total Reworked";

                ws.Cells[$"U{rowDailyCount + 19}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                ws.Cells[$"U{rowDailyCount + 19}"].Formula = $"=SUM(F17:F{rowDailyCount + 16})";
                ws.Cells[$"V{rowDailyCount + 19}"].Formula = $"=SUM(G17:G{rowDailyCount + 16})";
                ws.Cells[$"W{rowDailyCount + 19}"].Formula = $"=SUM(H17:H{rowDailyCount + 16})";
                ws.Cells[$"X{rowDailyCount + 19}"].Formula = $"=SUM(I17:I{rowDailyCount + 16})";
                ws.Cells[$"Y{rowDailyCount + 19}"].Formula = $"=SUM(T17:T{rowDailyCount + 16})";


                ws.Cells[$"V{rowDailyCount + 20}"].Formula = $"=V{rowDailyCount + 19}/U{rowDailyCount + 19}";
                ws.Cells[$"V{rowDailyCount + 20}"].Style.Numberformat.Format = "#0.00%";

                ws.Cells[$"W{rowDailyCount + 20}"].Formula = $"=W{rowDailyCount + 19}/U{rowDailyCount + 19}";
                ws.Cells[$"W{rowDailyCount + 20}"].Style.Numberformat.Format = "#0.00%";

                ws.Cells[$"X{rowDailyCount + 20}"].Formula = $"=X{rowDailyCount + 19}/U{rowDailyCount + 19}";
                ws.Cells[$"X{rowDailyCount + 20}"].Style.Numberformat.Format = "#0.00%";

                ws.Cells[$"Y{rowDailyCount + 20}"].Formula = $"=Y{rowDailyCount + 19}/U{rowDailyCount + 19}";
                ws.Cells[$"Y{rowDailyCount + 20}"].Style.Numberformat.Format = "#0.00%";


                using (var rng = ws.Cells[$"T{rowDailyCount + 18}:Y{rowDailyCount + 18 + 2}"])
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

                for (var i = 3; i <= 5; i++)
                    ws.Column(i).Width = 9.4;

                for (var i = 10; i <= 17; i++)
                    ws.Column(i).Width = 5.5;

                for (var i = 19; i <= 24; i++)
                    ws.Column(i).Width = 13;

                ws.Column(18).Width = 26;
                ws.Column(25).Width = 14.2;

                ws.Row(15).Height = 30;
                ws.Row(16).Height = 30;

                ws.Cells["A15"].Value = "No";
                ws.Cells["A16"].Value = "Номер";

                ws.Cells["B15"].Value = "Date of inspection";
                ws.Cells["B16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DateOfInspection")?.ResourceValue;

                ws.Cells["C15"].Value = "Part number";
                ws.Cells["C16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.PartNumber")?.ResourceValue;

                ws.Cells["D15"].Value = "Delivery №";
                ws.Cells["D16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DeliveryNumber")?.ResourceValue;


                ws.Cells["E15"].Value = "Charge №";
                ws.Cells["E16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ChargeNumber")?.ResourceValue;

                ws.Cells["F15"].Value = "Quantity";
                ws.Cells["F16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.Quantity")?.ResourceValue;

                ws.Cells["G15"].Value = "First run OK parts";
                ws.Cells["G16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.FirstRunOkParts")?.ResourceValue;


                ws.Cells["H15"].Value = "Blocked parts";
                ws.Cells["H16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.BlockedParts")?.ResourceValue;

                ws.Cells["I15"].Value = "NOK parts";
                ws.Cells["I16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.NokParts")?.ResourceValue;


                ws.Cells["J15:Q15"].Merge = true;
                using (var rng = ws.Cells["J15:Q15"])
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
                ws.Cells["R16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfBlockedParts")?.ResourceValue;



                ws.Cells["S15"].Value = "NOK percentage";
                ws.Cells["S16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.NokPercentage")?.ResourceValue;

                ws.Cells["T15"].Value = "Reworked parts";
                ws.Cells["T16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ReworkedParts")?.ResourceValue;


                ws.Cells["U15:X15"].Merge = true;
                using (var rng = ws.Cells["U15:X15"])
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
                foreach (var item in model.Items)
                {
                    ws.Cells["A" + num].Value = num;
                    ws.Cells["B" + num].Value = item.DateOfInspection?.Date;
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
                using (var rng = ws.Cells["R17:R" + (rowDailyCount + 16)])
                {
                    var j = 1;
                    var allCriterias = _criteriaService.GetAllCriteriaValues(orderId).OrderBy(x => x.Id).ToList().Select(s => j++ + ". " + s.Description).ToList();
                    rng.Value = string.Join("\r\n", allCriterias);
                }

                var cellName = $"B{rowDailyCount + 19}:C{rowDailyCount + 19}";
                ws.Cells[cellName].Merge = true;
                using (var rng = ws.Cells[cellName])
                {
                    rng.Value = "Description of defects:";
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                }

                cellName = $"B{rowDailyCount + 19 + 1}:C{rowDailyCount + 19 + 1}";
                ws.Cells[cellName].Merge = true;
                using (var rng = ws.Cells[cellName])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfDefects")?.ResourceValue;
                }

                cellName = $"B{rowDailyCount + 19 + 8}:C{rowDailyCount + 19 + 8}";
                ws.Cells[cellName].Merge = true;
                using (var rng = ws.Cells[cellName])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Value = "Description of rework:";
                }

                cellName = $"B{rowDailyCount + 19 + 8 + 1}:C{rowDailyCount + 19 + 8 + 1}";
                ws.Cells[cellName].Merge = true;
                using (var rng = ws.Cells[cellName])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 8;
                    rng.Style.WrapText = true;
                    rng.Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfRework")?.ResourceValue;
                }

                for (var i = 0; i < 12; i++)
                {
                    ws.Row(rowDailyCount + 19 + i).Height = 13;
                }

                for (var i = 0; i < 8; i++)
                {
                    ws.Cells["D" + (rowDailyCount + 19 + i)].Value = i + 1;

                    var cn = string.Format("E{0}:G{0}", rowDailyCount + 19 + i);
                    ws.Cells[cn].Merge = true;
                    using (var rng = ws.Cells[cn])
                    {
                        rng.Value = model.BlockedCriterias.Count > i ? model.BlockedCriterias[i].Description : "";
                    }
                }
                for (var i = 0; i < 4; i++)
                {
                    ws.Cells["D" + (rowDailyCount + 19 + 8 + i)].Value = i + 1 + 8;

                    var cn = string.Format("E{0}:G{0}", rowDailyCount + 19 + 8 + i);
                    ws.Cells[cn].Merge = true;
                    using (var rng = ws.Cells[cn])
                    {
                        rng.Value = model.ReworkedCriterias.Count > i ? model.ReworkedCriterias[i].Description : "";
                    }
                }


                //Header

                //logo
                var image = Image.FromFile(HostingEnvironment.MapPath("~/Administration/Content/images/invenio.jpg") ?? throw new InvalidOperationException());
                var picture = ws.Drawings.AddPicture("logo", image);
                picture.SetSize(40);
                picture.SetPosition(1, 0, 1, 5);


                ws.Cells["H4"].Value = "SORTING & REWORK REPORT";
                ws.Cells["H4"].Style.Font.Size = 16;
                ws.Cells["H4"].Style.Font.Bold = true;

                ws.Cells["F6:G6"].Merge = true;
                using (var rng = ws.Cells["F6:G6"])
                {
                    rng.Value = "SUPPLIER :";
                    rng.Style.Font.Bold = true;
                }
                ws.Cells["F7:G7"].Merge = true;
                using (var rng = ws.Cells["F7:G7"])
                {
                    rng.Value = "PART NUMBER :";
                }
                ws.Cells["F8:G8"].Merge = true;
                using (var rng = ws.Cells["F8:G8"])
                {
                    rng.Value = "PART DESCRIPTION :";
                }
                ws.Cells["F9:G9"].Merge = true;
                using (var rng = ws.Cells["F9:G9"])
                {
                    rng.Value = "ORDER NO :";
                }
                ws.Cells["F10:G10"].Merge = true;
                using (var rng = ws.Cells["F10:G10"])
                {
                    rng.Value = "QUANTITY TO CHECK: ";
                }
                ws.Cells["F11:G11"].Merge = true;
                using (var rng = ws.Cells["F11:G11"])
                {
                    rng.Value = "TYPE OF REPORT :";
                }
                //------
                ws.Cells["H6:Q6"].Merge = true;
                using (var rng = ws.Cells["H6:Q6"])
                {
                    rng.Value = model.SupplierName;
                }
                ws.Cells["H7:Q7"].Merge = true;
                using (var rng = ws.Cells["H7:Q7"])
                {
                    rng.Value = model.PartNumber;
                }
                ws.Cells["H8:Q8"].Merge = true;
                using (var rng = ws.Cells["H8:Q8"])
                {
                    rng.Value = "";
                }
                ws.Cells["H9:Q9"].Merge = true;
                using (var rng = ws.Cells["H9:Q9"])
                {
                    rng.Value = model.OrderNo;
                }
                ws.Cells["H10:Q10"].Merge = true;
                using (var rng = ws.Cells["H10:Q10"])
                {
                    rng.Value = model.QuantityToCheck;
                }
                ws.Cells["H11:Q11"].Merge = true;
                using (var rng = ws.Cells["H11:Q11"])
                {
                    rng.Value = "Interim Report";
                }

                using (var rng = ws.Cells["F6:Q11"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }
                using (var rng = ws.Cells["F6:Q6"])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }


                pck.SaveAs(ms);

                ms.Position = 0;
                return File(ms.ToArray(), MimeTypes.ApplicationXlsx, $"Interim-Report-{model.OrderNo}.xlsx");
            }
        }
        #endregion

        #region Еfficiency

        public virtual ActionResult ЕfficiencyList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageЕfficiency))
                return AccessDeniedView();

            //var model = new ReportListModel();
            return View();
        }

        [HttpPost]
        public virtual ActionResult ЕfficiencyList(DataSourceRequest command, ЕfficiencyListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageЕfficiency))
                return AccessDeniedKendoGridJson();

            //var model = new ЕfficiencyModel();
            var rm = new List<ЕfficiencyModel>();

            var users = _userService
                .GetAllUsers(UserRoleIds: new[] { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id })
                .ToList();

            foreach (var user in users)
            {
                var userReports = _reportService
                    .GetAllReports(userId: user.Id, fromDate: model.DateFrom, toDate: model.DateTo, isAprroved: 1);

                //var monthlyHors = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) * 8;
                var workDays = userReports.Select(x => x.DateOfInspection.Value.Date).ToList().Distinct().ToList().Count;
                var workHours = workDays * 8;

                var hoursSold = userReports.Sum(x => x.Time);

                var listOfReports = userReports
                    .GroupBy(x => x.DateOfInspection.Value.Date)
                    .Select(x => x.ToList());

                var hoursSoldEf = 0;

                foreach (var reports in listOfReports)
                {
                    var tt = reports.GroupBy(x => x.OrderId).Select(x => x.ToList());
                    foreach (var reportsTt in tt)
                    {
                        var quantity = reportsTt.Sum(x => x.CheckedPartsQuantity);
                        var pph = _orderService.GetOrderById(reportsTt.First().OrderId)?.PartsPerHour;

                        hoursSoldEf += pph.HasValue ? (int)Math.Ceiling((decimal)quantity / pph.Value) : 0;
                    }
                }

                var efficiency = hoursSold.HasValue ? (double)hoursSold.Value / workHours : 0;

                rm.Add(new ЕfficiencyModel
                {
                    UserName = user.GetFullName(),
                    MonthlyHours = workHours,
                    HoursSold = hoursSold ?? 0,
                    HoursSoldEf = hoursSoldEf,
                    Difference = (hoursSold ?? 0) - workHours,
                    Efficiency = efficiency
                });
            }

            var result = new PagedList<ЕfficiencyModel>(rm, command.Page - 1, command.PageSize);
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