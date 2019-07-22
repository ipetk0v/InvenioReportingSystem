using System;
using Invenio.Core;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Customers;
using Invenio.Core.Infrastructure;
using Invenio.Services.Authentication;
using Invenio.Services.Criteria;
using Invenio.Services.Customers;
using Invenio.Services.DeliveryNumber;
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
using Invenio.Services.ChargeNumber;
using Invenio.Services.Parts;
using Invenio.Services.Reports;

namespace Invenio.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ICriteriaService _criteriaService;
        private readonly ILocalizationService _localizationService;
        private readonly IDeliveryNumberService _deliveryNumberService;
        private readonly IChargeNumberService _chargeNumberService;
        private readonly IReportService _reportService;
        private readonly IReportDetailService _reportDetailService;
        private readonly IPartService _partService;

        public HomeController(
            IWorkContext workContext,
            ICustomerService customerService,
            IOrderService orderService,
            ICriteriaService criteriaService,
            ILocalizationService localizationService,
            IDeliveryNumberService deliveryNumberService,
            IChargeNumberService chargeNumberService,
            IReportService reportService,
            IReportDetailService reportDetailService,
            IPartService partService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _orderService = orderService;
            _criteriaService = criteriaService;
            _localizationService = localizationService;
            _deliveryNumberService = deliveryNumberService;
            _chargeNumberService = chargeNumberService;
            _reportService = reportService;
            _reportDetailService = reportDetailService;
            _partService = partService;
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
        public virtual ActionResult GetAllOrdersByCustomer(int customerId)
        {
            if (customerId == 0)
                return new NullJsonResult();

            var result = _orderService.GetAllCustomerOrders(customerId);
            var viewModel = result.Select(x => new
            {
                Text = x.Number,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
        }

        [HttpPost]
        public virtual ActionResult GetOrderParts(int orderId)
        {
            if (orderId == 0)
                return new NullJsonResult();

            var result = _partService.GetAllOrderParts(orderId);
            var viewModel = result.Select(x => new
            {
                Text = x.SerNumber,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
        }

        [HttpPost]
        public virtual ActionResult GetDeliveryNumbers(int partId)
        {
            if (partId == 0)
                return new NullJsonResult();

            var result = _deliveryNumberService.GetAllPartDeliveryNumbers(partId);
            var viewModel = result.Select(x => new
            {
                Text = x.Number,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
        }

        [HttpPost]
        public virtual ActionResult GetChargeNumbers(int delNumberId)
        {
            if (delNumberId == 0)
                return new NullJsonResult();

            var result = _chargeNumberService.GetAllDeliveryChargeNumbers(delNumberId);
            var viewModel = result.Select(x => new
            {
                Text = x.Number,
                Value = x.Id.ToString()
            });

            return Json(new { viewModel });
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

        [Authorize]
        [HttpPost]
        public virtual ActionResult Index(IList<ReportModel> reports)
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
                    OkPartsQuantity = okQuantity,
                    NokPartsQuantity = nokQuantity,
                    ReworkPartsQuantity = reworkedQuantity,
                    WorkShift = (WorkShift)report.WorkShiftId,
                    UserId = _workContext.CurrentUser.Id,
                    Approved = false,
                    CreatedOn = DateTime.Now,
                    DateOfInspection = report.ReportDate,
                    ApprovedOn = null,
                    PartId = report.PartId,
                    DeliveryNumberId = report.DeliveryNumberId == 0 ? (int?)null : report.DeliveryNumberId,
                    ChargeNumberId = report.ChargeNumberId == 0 ? (int?)null : report.ChargeNumberId
                };

                _reportService.InsertReport(entity);

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
                    Text = WorkShifts.NightShift.GetLocalizedEnum(_localizationService, _workContext),
                    Value = ((int)WorkShifts.NightShift).ToString()
                });

            var customers = new List<Customer>();
            if (_workContext.CurrentUser.ManufacturerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.ManufacturerRegions)
                {
                    customers.AddRange(_customerService.GetAllCustomers(countryId: region.CountryId, stateId: region.Id));
                }
            }

            if (_workContext.CurrentUser.Manufacturers.Any())
            {
                foreach (var manufacturer in _workContext.CurrentUser.Manufacturers)
                {
                    customers.AddRange(_customerService.GetCustomersByManufacturer(manufacturer.Id));
                }
            }

            model.Customers.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Customer"), Value = "0" });
            foreach (var customer in customers)
            {
                var orders = _orderService.GetAllCustomerOrders(customer.Id);
                if (orders.Any())
                {
                    model.Customers.Add(new SelectListItem
                    {
                        Text = customer.Name,
                        Value = customer.Id.ToString()
                    });
                }
            }

            model.Orders.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Order"), Value = "0" });
            model.Parts.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.Parts"), Value = "0" });
            model.DeliveryNumbers.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.DeliveryNumers"), Value = "0" });
            model.ChargeNumbers.Add(new SelectListItem { Text = _localizationService.GetResource("Home.Index.Select.ChargeNumber"), Value = "0" });
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
                var part = _partService.GetPartById(report.PartId ?? 0);
                var delNumber = _deliveryNumberService.GetDeliveryNumberById(report.DeliveryNumberId ?? 0);
                var chNumber = _chargeNumberService.GetChargeNumberById(report.ChargeNumberId ?? 0);

                model.ReportHistoryList.Add(new HistoryModel
                {
                    WorkShift = report.WorkShift.GetLocalizedEnum(_localizationService, _workContext),
                    DateOfInspection = report.DateOfInspection?.Date.ToShortDateString(),
                    Order = report.Order?.Number,
                    Customer = order?.Customer?.Name,
                    PartName = part?.SerNumber,
                    DeliveryNumber = delNumber?.Number,
                    ChargeNumber = chNumber?.Number,
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
