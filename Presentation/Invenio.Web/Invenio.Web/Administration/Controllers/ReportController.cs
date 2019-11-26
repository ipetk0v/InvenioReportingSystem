using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Report;
using Invenio.Core;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Reports;
using Invenio.Core.Domain.Suppliers;
using Invenio.Core.Domain.Users;
using Invenio.Services.Common;
using Invenio.Services.Criteria;
using Invenio.Services.Customers;
using Invenio.Services.Events;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Orders;
using Invenio.Services.Reports;
using Invenio.Services.Security;
using Invenio.Services.Supplier;
using Invenio.Services.Users;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Invenio.Core.Data;
using Image = System.Drawing.Image;

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
        private readonly IRepository<OrderAttribute> _orderAttributesRep;
        private readonly IRepository<OrderAttributeValue> _attributeValuesRep;
        private readonly ILocalizationService _localizationService;
        private readonly IUserActivityService _userActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ISupplierService _supplierService;
        private readonly IReportDetailService _reportDetailService;
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
            IReportDetailService reportDetailService,
            ICriteriaService criteriaService,
            IPdfService pdfService,
            ICustomerService customerService, IRepository<OrderAttribute> orderAttributesRep, IRepository<OrderAttributeValue> attrbuteValuesRep)
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
            _reportDetailService = reportDetailService;
            _criteriaService = criteriaService;
            _pdfService = pdfService;
            _customerService = customerService;
            _orderAttributesRep = orderAttributesRep;
            _attributeValuesRep = attrbuteValuesRep;
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

            var attributes = _orderAttributesRep.Table.ToList().OrderBy(w => w.Name);
            ViewBag.Attributes = attributes;

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

            var dfrs = TestGetDailyReportData(model.OrderId);

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

        private IList<DailyReportModel> TestGetDailyReportData(int orderId)
        {
            var reports = _reportService.GetAllReports(orderId: orderId, isAprroved: 1);

            var groupedReports = reports.GroupBy(c => new
            {
                Date = c.DateOfInspection?.Date,
                AttrsKey = string.Join("_", c.ReportOrderAttributes.OrderBy(o => o.AttributeId).ThenBy(o => o.AttributeValueId).Select(s => s.AttributeId.ToString() + "=" + s.AttributeValueId.ToString())),
            }).ToDictionary(w => new Tuple<DateTime?, string>(w.Key.Date?.Date, w.Key.AttrsKey), w => w);

            var dfrs = groupedReports.Select(x => new DailyReportModel
            {
                DateOfInspection = x.Key.Item1,
                AttrsKey = x.Key.Item2
            })
             .OrderBy(x => x.Id)
             .ToList();


            foreach (var dfr in dfrs)
            {
                var subReports = groupedReports[new Tuple<DateTime?, string>(dfr.DateOfInspection, dfr.AttrsKey)];

                var valIds = dfr.AttrsKey.Split('_').Select(s => int.Parse(s.Split('=')[1]));
                var attributeValues = _attributeValuesRep.Table.Where(w => valIds.Contains(w.Id))
                    .ToDictionary(w => w.Id, w => w.Name);

                dfr.Attributes = dfr.AttrsKey.Split('_').Select(s => new { AttributeId = int.Parse(s.Split('=')[0]), ValueId = int.Parse(s.Split('=')[1]) })
                    .ToDictionary(s => s.AttributeId, s => attributeValues.ContainsKey(s.ValueId) ? attributeValues[s.ValueId] : "");


                dfr.NokParts = subReports.Sum(x => x.NokPartsQuantity);
                dfr.ReworkedParts = subReports.Sum(x => x.ReworkPartsQuantity);
                dfr.FirstRunOkParts = subReports.Sum(x => x.OkPartsQuantity);
                dfr.BlockedParts = subReports.Sum(x => x.ReworkPartsQuantity) + subReports.Sum(x => x.NokPartsQuantity);
                dfr.Quantity = subReports.Sum(x => x.OkPartsQuantity) + subReports.Sum(x => x.NokPartsQuantity) + subReports.Sum(x => x.ReworkPartsQuantity);
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
                    Quantity = x.Quantity
                })
                .Select(x => new ReportDetailsModel
                {
                    CriteriaId = x.Key.Criteria.Id,
                    Quantity = x.Key.Quantity,
                    CriteriaType = x.Key.Criteria.CriteriaType,
                    DateOfInspection = x.Key.DateOfInspection
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
                                    && x.CriteriaId == criteria.Id)
                        //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                        //&& x.ChargeNumber == dailyReportModel.ChargeNumber)
                        .Sum(x => x.Quantity);

                    if (quantity == 0)
                        continue;

                    var entity = dfrs
                        .FirstOrDefault(x => x.DateOfInspection == dailyReportModel.DateOfInspection);
                    //&& x.PartNumber == dailyReportModel.PartNumber
                    //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                    //&& x.ChargeNumber == dailyReportModel.ChargeNumber);

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
                    Quantity = x.Quantity
                })
                .Select(x => new ReportDetailsModel
                {
                    CriteriaId = x.Key.Criteria.Id,
                    Quantity = x.Key.Quantity,
                    CriteriaType = x.Key.Criteria.CriteriaType,
                    DateOfInspection = x.Key.DateOfInspection
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
                                    && x.CriteriaId == criteria.Id)
                        //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                        //&& x.ChargeNumber == dailyReportModel.ChargeNumber)
                        .Sum(x => x.Quantity);

                    if (quantity == 0)
                        continue;

                    var entity = dfrs
                        .FirstOrDefault(x => x.DateOfInspection == dailyReportModel.DateOfInspection);
                    //&& x.PartNumber == dailyReportModel.PartNumber
                    //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                    //&& x.ChargeNumber == dailyReportModel.ChargeNumber);

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

        private List<DailyReportModel> GetDailyReportData(int orderId)
        {
            var reports = _reportService.GetAllReports(orderId: orderId, isAprroved: 1);

            var dfrs = reports.GroupBy(c => new
            {
                c.DateOfInspection?.Date
            })
            .Select(x => new DailyReportModel
            {
                DateOfInspection = x.Key.Date?.Date
            })
            .OrderBy(x => x.Id)
            .ToList();

            foreach (var dfr in dfrs)
            {
                var newTest = reports.Where(x => x.DateOfInspection?.Date == dfr.DateOfInspection?.Date);

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
                    Quantity = x.Quantity
                })
                .Select(x => new ReportDetailsModel
                {
                    CriteriaId = x.Key.Criteria.Id,
                    Quantity = x.Key.Quantity,
                    CriteriaType = x.Key.Criteria.CriteriaType,
                    DateOfInspection = x.Key.DateOfInspection
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
                                    && x.CriteriaId == criteria.Id)
                        //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                        //&& x.ChargeNumber == dailyReportModel.ChargeNumber)
                        .Sum(x => x.Quantity);

                    if (quantity == 0)
                        continue;

                    var entity = dfrs
                        .FirstOrDefault(x => x.DateOfInspection == dailyReportModel.DateOfInspection);
                    //&& x.PartNumber == dailyReportModel.PartNumber
                    //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                    //&& x.ChargeNumber == dailyReportModel.ChargeNumber);

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
                    Quantity = x.Quantity
                })
                .Select(x => new ReportDetailsModel
                {
                    CriteriaId = x.Key.Criteria.Id,
                    Quantity = x.Key.Quantity,
                    CriteriaType = x.Key.Criteria.CriteriaType,
                    DateOfInspection = x.Key.DateOfInspection
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
                                    && x.CriteriaId == criteria.Id)
                        //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                        //&& x.ChargeNumber == dailyReportModel.ChargeNumber)
                        .Sum(x => x.Quantity);

                    if (quantity == 0)
                        continue;

                    var entity = dfrs
                        .FirstOrDefault(x => x.DateOfInspection == dailyReportModel.DateOfInspection);
                    //&& x.PartNumber == dailyReportModel.PartNumber
                    //&& x.DeliveryNumber == dailyReportModel.DeliveryNumber
                    //&& x.ChargeNumber == dailyReportModel.ChargeNumber);

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
            //var dfrs = GetDailyReportData(orderId).ToList();
            var dfrs = TestGetDailyReportData(orderId).ToList();
            var result = dfrs.OrderBy(x => x.DateOfInspection?.Date);
            var allCreterias = _criteriaService.GetAllCriteriaValues(orderId);
            var order = _orderService.GetOrderById(orderId);
            var attributes = _orderAttributesRep.Table.ToList().OrderBy(w => w.Name);

            return new DailyReportExportModel(allCreterias, result, order, attributes);
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
            var attributesCount = model.Attributes.Count;

            using (var ms = new MemoryStream())
            {
                var pck = new ExcelPackage();
                var ws = pck.Workbook.Worksheets.Add("100% Report");
                ws.View.ZoomScale = 82;

                using (var rng = ws.Cells["F6:Q10"])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                using (var rng = ws.Cells[15, 1, rowDailyCount + 16, 22 + attributesCount])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using (var rng = ws.Cells[15, 1, 16, 22 + attributesCount])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (var rng = ws.Cells[17, 1, 16 + rowDailyCount, 22 + attributesCount])
                {
                    rng.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                using (var rng = ws.Cells[15, 1, 16 + rowDailyCount, 22 + attributesCount])
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


                using (var rng = ws.Cells[rowDailyCount + 19, 15 + attributesCount, rowDailyCount + 19 + 1, 15 + attributesCount])
                {
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                using (var rng = ws.Cells[rowDailyCount + 19, 19 + attributesCount, rowDailyCount + 19 + 1, 22 + attributesCount])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                }

                using (var rng = ws.Cells[rowDailyCount + 18, 18 + attributesCount, rowDailyCount + 18, 22 + attributesCount])
                {
                    rng.Style.Font.Bold = true;
                }

                ws.Cells[19 + rowDailyCount, 17 + attributesCount].Value = "Quantities";
                ws.Cells[20 + rowDailyCount, 17 + attributesCount].Value = "Percentages";
                ws.Cells[18 + rowDailyCount, 18 + attributesCount].Value = "Total checked";
                ws.Cells[18 + rowDailyCount, 19 + attributesCount].Value = "Total OK";
                ws.Cells[18 + rowDailyCount, 20 + attributesCount].Value = "Total Blocked";
                ws.Cells[18 + rowDailyCount, 21 + attributesCount].Value = "Total NOK";
                ws.Cells[18 + rowDailyCount, 22 + attributesCount].Value = "Total Reworked";

                ws.Cells[19 + rowDailyCount, 18 + attributesCount].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                ws.Cells[19 + rowDailyCount, 18 + attributesCount].Formula = $"=SUM({ws.Cells[17, 3 + attributesCount].Address}:{ws.Cells[16 + rowDailyCount, 3 + attributesCount].Address})";
                ws.Cells[19 + rowDailyCount, 19 + attributesCount].Formula = $"=SUM({ws.Cells[17, 4 + attributesCount].Address}:{ws.Cells[16 + rowDailyCount, 4 + attributesCount].Address})";
                ws.Cells[19 + rowDailyCount, 20 + attributesCount].Formula = $"=SUM({ws.Cells[17, 6 + attributesCount].Address}:{ws.Cells[16 + rowDailyCount, 5 + attributesCount].Address})";
                ws.Cells[19 + rowDailyCount, 21 + attributesCount].Formula = $"=SUM({ws.Cells[17, 6 + attributesCount].Address}:{ws.Cells[16 + rowDailyCount, 6 + attributesCount].Address})";
                ws.Cells[19 + rowDailyCount, 22 + attributesCount].Formula = $"=SUM({ws.Cells[17, 17 + attributesCount].Address}:{ws.Cells[16 + rowDailyCount, 17 + attributesCount].Address})";

                var totalCheckedAddress = ws.Cells[19 + rowDailyCount, 18 + attributesCount].Address;

                ws.Cells[20 + rowDailyCount, 19 + attributesCount].Formula = $"={ws.Cells[19 + rowDailyCount, 19 + attributesCount].Address}/{totalCheckedAddress}";
                ws.Cells[20 + rowDailyCount, 19 + attributesCount].Style.Numberformat.Format = "#0.00%";

                ws.Cells[20 + rowDailyCount, 20 + attributesCount].Formula = $"={ws.Cells[19 + rowDailyCount, 20 + attributesCount].Address}/{totalCheckedAddress}";
                ws.Cells[20 + rowDailyCount, 20 + attributesCount].Style.Numberformat.Format = "#0.00%";

                ws.Cells[20 + rowDailyCount, 21 + attributesCount].Formula = $"={ws.Cells[19 + rowDailyCount, 21 + attributesCount].Address}/{totalCheckedAddress}";
                ws.Cells[20 + rowDailyCount, 21 + attributesCount].Style.Numberformat.Format = "#0.00%";

                ws.Cells[20 + rowDailyCount, 22 + attributesCount].Formula = $"={ws.Cells[19 + rowDailyCount, 22 + attributesCount].Address}/{totalCheckedAddress}";
                ws.Cells[20 + rowDailyCount, 22 + attributesCount].Style.Numberformat.Format = "#0.00%";


                using (var rng = ws.Cells[rowDailyCount + 18, 17 + attributesCount, rowDailyCount + 18 + 2, 22 + attributesCount])
                {
                    rng.Style.Font.Name = "Arial CE";
                    rng.Style.Font.Size = 10;
                    rng.Style.WrapText = true;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }


                ws.Column(1).Width = 3.8;
                ws.Column(2).Width = 14.5;
                ws.Column(6 + attributesCount - 1).Width = 14.5;
                ws.Column(7 + attributesCount - 1).Width = 14.5;
                ws.Column(8 + attributesCount - 1).Width = 6.5;
                ws.Column(9 + attributesCount - 1).Width = 6.5;

                for (var i = 3; i < 3 + attributesCount; i++)
                    ws.Column(i).Width = 9.4;

                for (var i = 7 + attributesCount; i <= 14 + attributesCount; i++)
                    ws.Column(i).Width = 5.5;

                for (var i = 15 + attributesCount; i <= 20 + attributesCount; i++)
                    ws.Column(i).Width = 13;

                ws.Column(15 + attributesCount).Width = 26;
                ws.Column(22 + attributesCount).Width = 14.2;

                ws.Row(15).Height = 30;
                ws.Row(16).Height = 30;
                ws.Row(16 + rowDailyCount + 2).Height = 30;

                ws.Cells["A15"].Value = "No";
                ws.Cells["A16"].Value = "Номер";

                ws.Cells["B15"].Value = "Date of inspection";
                ws.Cells["B16"].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DateOfInspection")?.ResourceValue;

                ws.Cells[15, 3, 15, 3 + attributesCount - 1].Merge = true;
                using (var rng = ws.Cells[15, 3, 15, 3 + attributesCount - 1])
                {
                    rng.Value = "Attributes";
                }

                var k = 0;
                foreach (var attr in model.Attributes)
                {
                    ws.Cells[16, 3 + k++].Value = attr.Name;
                }


                ws.Cells[15, 3 + attributesCount].Value = "Quantity";
                ws.Cells[16, 3 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.Quantity")?.ResourceValue;

                ws.Cells[15, 4 + attributesCount].Value = "First run OK parts";
                ws.Cells[16, 4 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.FirstRunOkParts")?.ResourceValue;


                ws.Cells[15, 5 + attributesCount].Value = "Blocked parts";
                ws.Cells[16, 5 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.BlockedParts")?.ResourceValue;

                ws.Cells[15, 6 + attributesCount].Value = "NOK parts";
                ws.Cells[16, 6 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.NokParts")?.ResourceValue;


                ws.Cells[15, 7 + attributesCount, 15, 7 + attributesCount + 7].Merge = true;
                using (var rng = ws.Cells[15, 7 + attributesCount, 15, 7 + attributesCount + 7])
                {
                    rng.Value = "Due to   (Razlog)";
                }

                ws.Cells[16, 7 + attributesCount].Value = "1";
                ws.Cells[16, 8 + attributesCount].Value = "2";
                ws.Cells[16, 9 + attributesCount].Value = "3";
                ws.Cells[16, 10 + attributesCount].Value = "4";
                ws.Cells[16, 11 + attributesCount].Value = "5";
                ws.Cells[16, 12 + attributesCount].Value = "6";
                ws.Cells[16, 13 + attributesCount].Value = "7";
                ws.Cells[16, 14 + attributesCount].Value = "8";

                ws.Cells[15, 15 + attributesCount].Value = "Description of blocked parts";
                ws.Cells[16, 15 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.DescriptionOfBlockedParts")?.ResourceValue;



                ws.Cells[15, 16 + attributesCount].Value = "NOK percentage";
                ws.Cells[16, 16 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.NokPercentage")?.ResourceValue;

                ws.Cells[15, 17 + attributesCount].Value = "Reworked parts";
                ws.Cells[16, 17 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ReworkedParts")?.ResourceValue;


                ws.Cells[15, 18 + attributesCount, 15, 18 + attributesCount + 3].Merge = true;
                using (var rng = ws.Cells[15, 18 + attributesCount, 15, 18 + attributesCount + 3])
                {
                    rng.Value = "Due to";
                }

                ws.Cells[16, 18 + attributesCount].Value = "9";
                ws.Cells[16, 19 + attributesCount].Value = "10";
                ws.Cells[16, 20 + attributesCount].Value = "11";
                ws.Cells[16, 21 + attributesCount].Value = "12";


                ws.Cells[15, 22 + attributesCount].Value = "Rework percentage";
                ws.Cells[16, 22 + attributesCount].Value = _localizationService.GetLocaleStringResourceByName("Admin.Report.DailyFinalReport.ReworkedPercentage").ResourceValue;


                ws.Cells[17, 15 + attributesCount, 16 + rowDailyCount, 15 + attributesCount].Merge = true;
                using (var rng = ws.Cells[17, 15 + attributesCount, 16 + rowDailyCount, 15 + attributesCount])
                {
                    var j = 1;
                    var allCriterias = _criteriaService.GetAllCriteriaValues(orderId).OrderBy(x => x.Id).ToList().Select(s => j++ + ". " + s.Description).ToList();
                    rng.Value = string.Join("\r\n", allCriterias);
                }


                var num = 17;
                foreach (var item in model.Items)
                {
                    ws.Cells["A" + num].Value = num;
                    ws.Cells["B" + num].Value = item.DateOfInspection?.Date;
                    ws.Cells["B" + num].Style.Numberformat.Format = "[$-409]DD.MMM.YY;@";

                    var j = 0;
                    foreach (var attr in model.Attributes)
                    {
                        ws.Cells[num, 3 + j++].Value = item.Attributes.ContainsKey(attr.Id)
                            ? item.Attributes[attr.Id]
                            : string.Empty;
                    }

                    ws.Cells[num, 3 + attributesCount].Value = item.Quantity;
                    ws.Cells[num, 4 + attributesCount].Value = item.FirstRunOkParts;
                    ws.Cells[num, 5 + attributesCount].Value = item.BlockedParts;
                    ws.Cells[num, 6 + attributesCount].Value = item.NokParts;
                    ws.Cells[num, 7 + attributesCount].Value = item.Dod1;
                    ws.Cells[num, 8 + attributesCount].Value = item.Dod2;
                    ws.Cells[num, 9 + attributesCount].Value = item.Dod3;
                    ws.Cells[num, 10 + attributesCount].Value = item.Dod4;
                    ws.Cells[num, 11 + attributesCount].Value = item.Dod5;
                    ws.Cells[num, 12 + attributesCount].Value = item.Dod6;
                    ws.Cells[num, 13 + attributesCount].Value = item.Dod7;
                    ws.Cells[num, 14 + attributesCount].Value = item.Dod8;

                    ws.Cells[num, 16 + attributesCount].Formula = string.Format(@"={0}/{1}", ws.Cells[num, 6 + attributesCount].Address, ws.Cells[num, 3 + attributesCount].Address);
                    ws.Cells[num, 16 + attributesCount].Style.Numberformat.Format = "#0.00%";

                    ws.Cells[num, 17 + attributesCount].Value = item.ReworkedParts;
                    ws.Cells[num, 18 + attributesCount].Value = item.Dor1;
                    ws.Cells[num, 19 + attributesCount].Value = item.Dor2;
                    ws.Cells[num, 20 + attributesCount].Value = item.Dor3;
                    ws.Cells[num, 21 + attributesCount].Value = item.Dor4;
                    ws.Cells[num, 22 + attributesCount].Formula = string.Format(@"={0}/{1}", ws.Cells[num, 17 + attributesCount].Address, ws.Cells[num, 3 + attributesCount].Address);
                    ws.Cells[num, 22 + attributesCount].Style.Numberformat.Format = "#0.00%";


                    num++;
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
                    rng.Value = "PART DESCRIPTION :";
                }
                ws.Cells["F8:G8"].Merge = true;
                using (var rng = ws.Cells["F8:G8"])
                {
                    rng.Value = "ORDER NO :";
                }
                ws.Cells["F9:G9"].Merge = true;
                using (var rng = ws.Cells["F9:G9"])
                {
                    rng.Value = "QUANTITY TO CHECK: ";
                }
                ws.Cells["F10:G10"].Merge = true;
                using (var rng = ws.Cells["F10:G10"])
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
                    rng.Value = "";
                }
                ws.Cells["H8:Q8"].Merge = true;
                using (var rng = ws.Cells["H8:Q8"])
                {
                    rng.Value = model.OrderNo;
                }
                ws.Cells["H9:Q9"].Merge = true;
                using (var rng = ws.Cells["H9:Q9"])
                {
                    rng.Value = model.QuantityToCheck;
                }
                ws.Cells["H10:Q10"].Merge = true;
                using (var rng = ws.Cells["H10:Q10"])
                {
                    rng.Value = "Interim Report";
                }

                using (var rng = ws.Cells["F6:Q10"])
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

            var model = new ЕfficiencyListModel();

            var date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            model.DateFrom = firstDayOfMonth;
            model.DateTo = lastDayOfMonth;

            model.OrderBy.Add(new SelectListItem { Text = "Потребителско име", Value = "0" });
            model.OrderBy.Add(new SelectListItem { Text = "Работни часове", Value = "1" });
            model.OrderBy.Add(new SelectListItem { Text = "Продадени часове", Value = "2" });
            model.OrderBy.Add(new SelectListItem { Text = "Разлика", Value = "3" });
            model.OrderBy.Add(new SelectListItem { Text = "Ефикасност", Value = "4" });

            return View(model);
        }

        private IEnumerable<User> GetFiltredUsers(ЕfficiencyListModel model)
        {
            if (_workContext.CurrentUser.IsAdmin())
            {
                if (!string.IsNullOrEmpty(model.Name))
                {
                    return _userService.GetAllUsers(
                            UserRoleIds: new[] { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id })
                        .Where(x => x.GetFullName().Contains(model.Name));
                }

                return _userService.GetAllUsers(
                    UserRoleIds: new[] { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id });
            }

            var users = _userService.GetAllUsers(
                UserRoleIds: new[] { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id });

            if (!string.IsNullOrEmpty(model.Name))
            {
                users = new PagedList<User>(users.Where(x => x.GetFullName().Contains(model.Name)).ToList(), 0, int.MaxValue);
            }

            var filtretUsers = new List<User>();
            if (_workContext.CurrentUser.CustomerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.CustomerRegions)
                {
                    filtretUsers.AddRange(users.Where(x => x.CustomerRegions.Contains(region)));

                    foreach (var user in users)
                    {
                        foreach (var customer in user.Customers)
                        {
                            if (customer.StateProvinceId == region.Id)
                            {
                                filtretUsers.Add(user);
                            }
                        }
                    }
                }
            }

            if (_workContext.CurrentUser.Customers.Any())
                filtretUsers.AddRange(from user in users from c in _workContext.CurrentUser.Customers where user.Customers.Contains(c) select user);

            var topRoleId = _workContext.CurrentUser.GetUserRoleIds().Max();
            return filtretUsers.Distinct().Where(x => x.UserRoles.Select(ur => ur.Id).Max() < topRoleId).ToList();
        }

        [HttpPost]
        public virtual ActionResult ЕfficiencyList(DataSourceRequest command, ЕfficiencyListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageЕfficiency))
                return AccessDeniedKendoGridJson();

            //var model = new ЕfficiencyModel();
            var rm = new List<ЕfficiencyModel>();

            //var users = _userService
            //    .GetAllUsers(UserRoleIds: new[] { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id })
            //    .ToList();

            var users = GetFiltredUsers(model);

            foreach (var user in users)
            {
                var userReports = _reportService
                    .GetAllReports(userId: user.Id, fromDate: model.DateFrom, toDate: model.DateTo, isAprroved: 1);

                var workDays = userReports.Where(x => x.DateOfInspection.HasValue).Select(x => x.DateOfInspection.Value.Date).ToList().Distinct().ToList().Count;
                var workHours = workDays * 8;

                var hoursSold = userReports.Sum(x => x.Time);

                var listOfReports = userReports
                    .Where(x => x.DateOfInspection.HasValue)
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

                        hoursSoldEf += pph.HasValue && pph.Value > 0 ? (int)Math.Ceiling((decimal)quantity / pph.Value) : 0;
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

            switch (model.OrderById)
            {
                case 0:
                    rm = rm.OrderBy(x => x.UserName).ToList();
                    break;

                case 1:
                    rm = rm.OrderByDescending(x => x.MonthlyHours).ToList();
                    break;

                case 2:
                    rm = rm.OrderByDescending(x => x.HoursSoldEf).ToList();
                    break;

                case 3:
                    rm = rm.OrderByDescending(x => x.Difference).ToList();
                    break;

                case 4:
                    rm = rm.OrderByDescending(x => x.Efficiency).ToList();
                    break;
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