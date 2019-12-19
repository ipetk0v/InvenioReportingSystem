using System;
using Invenio.Core;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Suppliers;
using Invenio.Core.Infrastructure;
using Invenio.Services.Authentication;
using Invenio.Services.Criteria;
using Invenio.Services.Supplier;
using Invenio.Services.Localization;
using Invenio.Services.Orders;
using Invenio.Web.Framework.Mvc;
using Invenio.Web.Framework.Security;
using Invenio.Web.Models.Common;
using Invenio.Web.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core.Domain.Reports;
using Invenio.Core.Domain.Users;
using Invenio.Services.Logging;
using Invenio.Services.Reports;

namespace Invenio.Web.Controllers
{
    public class HomeController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly ISupplierService _supplierService;
        private readonly IOrderService _orderService;
        private readonly ICriteriaService _criteriaService;
        private readonly ILocalizationService _localizationService;
        private readonly IReportService _reportService;
        private readonly IReportDetailService _reportDetailService;
        private readonly IUserActivityService _userActivityService;
        private readonly IOrderAttributeService _orderAttributeService;

        public HomeController(
            IWorkContext workContext,
            ISupplierService supplierService,
            IOrderService orderService,
            ICriteriaService criteriaService,
            ILocalizationService localizationService,
            IReportService reportService,
            IReportDetailService reportDetailService,
            IUserActivityService userActivityService,
            IOrderAttributeService orderAttributeService)
        {
            _workContext = workContext;
            _supplierService = supplierService;
            _orderService = orderService;
            _criteriaService = criteriaService;
            _localizationService = localizationService;
            _reportService = reportService;
            _reportDetailService = reportDetailService;
            _userActivityService = userActivityService;
            _orderAttributeService = orderAttributeService;
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Index()
        {
            if (EngineContext.Current.Resolve<IAuthenticationService>().GetAuthenticatedUser() == null)
            {
                return RedirectToRoute("Login");
            }

            var model = PrepareReportModel();
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult GetAllOrdersBySupplier(int supplierId)
        {
            if (supplierId == 0)
                return new NullJsonResult();

            var result = _orderService.GetAllSupplierOrders(supplierId);
            var viewModel = result.Select(x => new
            {
                Text = x.Number,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
        }

        [HttpPost]
        public virtual ActionResult GetOrderAttribute(int orderId)
        {
            if (orderId == 0)
                return new NullJsonResult();

            var orderAttributeMappings = _orderAttributeService
                                            .GetOrderAttributeMappingsByOrderId(orderId)
                                            .Where(x => !x.OrderAttribute.ParentOrderAttributeId.HasValue);

            var model = new OrderAttributeMappingModel();
            foreach (var oam in orderAttributeMappings)
            {
                var oAmodel = new OrderAttributeModel();

                oAmodel.OrderAttributeId = oam.OrderAttributeId;
                oAmodel.OrderAttribute = oam.OrderAttribute.Name;
                foreach (var oav in oam.OrderAttributeValues)
                    oAmodel.OrderAttributeValues.Add(oav);

                model.OrderAttributes.Add(oAmodel);
            }

            return PartialView("_OrderAttributes", model);
        }

        [HttpPost]
        public virtual ActionResult GetChildrenOrderAttribute(int orderAttributeId, int selectedValueId, int orderId)
        {
            var model = new OrderAttributeMappingModel();

            if (orderAttributeId != 0)
            {
                var parentOrderAttribute = _orderAttributeService
                                        .GetAllOrderAttributes()
                                        .FirstOrDefault(x => x.ParentOrderAttributeId == orderAttributeId);

                if (parentOrderAttribute != null)
                {
                    var oam = _orderAttributeService
                        .GetOrderAttributeMappingsByOrderAttributeId(parentOrderAttribute.Id)
                        .FirstOrDefault(x => x.OrderId == orderId);

                    if (oam != null)
                    {
                        var oAmodel = new OrderAttributeModel();
                        oAmodel.OrderAttributeId = oam.OrderAttributeId;
                        oAmodel.OrderAttribute = oam.OrderAttribute.Name;
                        foreach (var oav in oam.OrderAttributeValues.Where(x => x.ParentAttributeValueId == selectedValueId))
                            oAmodel.OrderAttributeValues.Add(oav);

                        if (!oAmodel.OrderAttributeValues.Any())
                            return null;

                        model.OrderAttributes.Add(oAmodel);
                    }

                    return PartialView("_OrderAttributes", model);
                }
            }

            return null;
        }

        [HttpPost]
        public virtual ActionResult GetBlockedPartsCriteria(int orderId)
        {
            if (orderId == 0)
                return new NullJsonResult();

            var result = _criteriaService.GetAllCriteriaValues(orderId).Where(x => x.CriteriaType == CriteriaType.BlockedParts);
            var viewModel = result.Select(x => new
            {
                Text = x.Description,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
        }

        [HttpPost]
        public virtual ActionResult GetReworkedPartsCriteria(int orderId)
        {
            if (orderId == 0)
                return new NullJsonResult();

            var result = _criteriaService.GetAllCriteriaValues(orderId).Where(x => x.CriteriaType == CriteriaType.ReworkParts);
            var viewModel = result.Select(x => new
            {
                Text = x.Description,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
        }

        [HttpPost]
        public virtual ActionResult Index(IList<ReportModel> reports, string returnUrl)
        {
            foreach (var report in reports)
            {
                if (report.CheckedQuantity == 0)
                    continue;

                if (report.WorkShiftId == 0)
                    continue;

                if (report.ReportDate == new DateTime())
                    continue;

                var nokQuantity = report.NokCriteria.Select(x => x.Quantity).Sum();
                var reworkedQuantity = report.ReworkedCriteria.Select(x => x.Quantity).Sum();
                var checkedQuantity = report.CheckedQuantity;
                var okQuantity = checkedQuantity - nokQuantity - reworkedQuantity;

                var entity = new Report
                {
                    OrderId = report.OrderId,
                    CheckedPartsQuantity = checkedQuantity,
                    OkPartsQuantity = okQuantity,
                    NokPartsQuantity = nokQuantity,
                    ReworkPartsQuantity = reworkedQuantity,
                    WorkShift = (WorkShift)report.WorkShiftId,
                    UserId = _workContext.CurrentUser.Id,
                    Approved = false,
                    CreatedOn = DateTime.Now,
                    DateOfInspection = report.ReportDate,
                    ApprovedOn = null,
                    Time = report.InputTime
                };

                _reportService.InsertReport(entity);
                _userActivityService.InsertActivity("InsertReport", _localizationService.GetResource("ActivityLog.InsertReport"), entity.Id);

                foreach (var attribute in report.PostedAttributes)
                {
                    var roa = new ReportOrderAttribute
                    {
                        AttributeId = attribute.AttributeId,
                        AttributeValueId = attribute.AttributeValueId,
                        ReportId = entity.Id
                    };
                    entity.ReportOrderAttributes.Add(roa);
                }

                if (entity.ReportOrderAttributes.Any())
                    _reportService.UpdateReport(entity);

                foreach (var nokCriteria in report.NokCriteria)
                {
                    if (nokCriteria.CriteriaId == 0)
                        continue;

                    var reportDetails = new ReportDetail
                    {
                        CriteriaId = nokCriteria.CriteriaId,
                        Quantity = nokCriteria.Quantity,
                        ReportId = entity.Id
                    };

                    _reportDetailService.InsertReport(reportDetails);
                }
                foreach (var reworkedCriteria in report.ReworkedCriteria)
                {
                    if (reworkedCriteria.CriteriaId == 0)
                        continue;

                    var reportDetails = new ReportDetail
                    {
                        CriteriaId = reworkedCriteria.CriteriaId,
                        Quantity = reworkedCriteria.Quantity,
                        ReportId = entity.Id
                    };

                    _reportDetailService.InsertReport(reportDetails);
                }
            }

            if (returnUrl == "logout")
                return RedirectToRoute("Logout");

            var model = PrepareReportModel();
            return View(model);
        }

        private ReportModel PrepareReportModel()
        {
            var model = new ReportModel();
            model.WorkShifts.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Work.Shift"), Value = "0" });
            model.WorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShifts.FirstShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShifts.FirstShift).ToString()
                });

            model.WorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShifts.SecondShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShifts.SecondShift).ToString()
                });

            model.WorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShifts.RegularShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShifts.RegularShift).ToString()
                });

            model.WorkShifts.Add(
                new SelectListItem
                {
                    Text = WorkShifts.NightShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShifts.NightShift).ToString()
                });

            var suppliers = new List<Supplier>();

            if (_workContext.CurrentUser.IsAdmin())
            {
                model.Suppliers.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Supplier"), Value = "0" });
                foreach (var supplier in _supplierService.GetAllSuppliers())
                {
                    var orders = _orderService.GetAllSupplierOrders(supplier.Id);
                    if (orders.Any())
                    {
                        model.Suppliers.Add(new SelectListItem
                        {
                            Text = supplier.Name,
                            Value = supplier.Id.ToString()
                        });
                    }
                }
            }
            else
            {
                if (_workContext.CurrentUser.CustomerRegions.Any())
                {
                    foreach (var region in _workContext.CurrentUser.CustomerRegions)
                    {
                        suppliers.AddRange(_supplierService.GetAllSuppliers(countryId: region.CountryId, stateId: region.Id));
                    }
                }

                if (_workContext.CurrentUser.Customers.Any())
                {
                    foreach (var customer in _workContext.CurrentUser.Customers)
                    {
                        suppliers.AddRange(_supplierService.GetSuppliersByCustomer(customer.Id));
                    }
                }

                model.Suppliers.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Supplier"), Value = "0" });
                foreach (var supplier in suppliers)
                {
                    var orders = _orderService.GetAllSupplierOrders(supplier.Id);
                    if (orders.Any())
                    {
                        model.Suppliers.Add(new SelectListItem
                        {
                            Text = supplier.Name,
                            Value = supplier.Id.ToString()
                        });
                    }
                }
            }

            model.Orders.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Order"), Value = "0" });
            model.BlockedParts.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.BlockedParts"), Value = "0" });
            model.ReworkedParts.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.ReworkedParts"), Value = "0" });
            return model;
        }

        public virtual ActionResult ReportHistory()
        {
            if (EngineContext.Current.Resolve<IAuthenticationService>().GetAuthenticatedUser() == null)
            {
                return RedirectToRoute("Login");
            }

            var model = PrepareReportHistoryModel();
            return View(model);
        }

        private ReportHistoryModel PrepareReportHistoryModel()
        {
            var model = new ReportHistoryModel();

            var reports = _reportService.GetAllReports(userId: _workContext.CurrentUser.Id).OrderByDescending(x => x.DateOfInspection).Take(50);

            foreach (var report in reports)
            {
                var order = _orderService.GetOrderById(report.OrderId);

                model.ReportHistoryList.Add(new HistoryModel
                {
                    WorkShift = report.WorkShift.GetLocalizedEnum(_localizationService, _workContext),
                    DateOfInspection = report.DateOfInspection?.Date.ToShortDateString(),
                    Order = report.Order?.Number,
                    Supplier = order?.Supplier?.Name,
                    OkQuantity = report.OkPartsQuantity,
                    NokQuantity = report.NokPartsQuantity,
                    ReworkedQuantity = report.ReworkPartsQuantity,
                    IsApproved = report.Approved
                });
            }

            return model;
        }
    }
}
