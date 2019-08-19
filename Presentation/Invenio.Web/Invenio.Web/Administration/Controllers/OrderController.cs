using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Orders;
using Invenio.Core;
using Invenio.Core.Domain.DeliveryNumbers;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Parts;
using Invenio.Services.ChargeNumber;
using Invenio.Services.Supplier;
using Invenio.Services.DeliveryNumber;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Orders;
using Invenio.Services.Parts;
using Invenio.Services.Security;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core.Domain.ChargeNumbers;
using Invenio.Core.Domain.Users;
using Invenio.Services;
using Invenio.Services.Reports;

namespace Invenio.Admin.Controllers
{
    public class OrderController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IUserActivityService _userActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ISupplierService _supplierService;
        private readonly IWorkContext _workContext;
        private readonly IPartService _partService;
        private readonly IDeliveryNumberService _delNumberService;
        private readonly IChargeNumberService _chargeNumberService;
        private readonly IReportService _reportService;

        public OrderController(
            IPermissionService permissionService,
            IOrderService orderService,
            IUserActivityService userActivityService,
            ILocalizationService localizationService,
            ISupplierService supplierService,
            IWorkContext workContext,
            IPartService partService,
            IDeliveryNumberService delNumberService,
            IChargeNumberService chargeNumberService,
            IReportService reportService
            )
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _userActivityService = userActivityService;
            _localizationService = localizationService;
            _supplierService = supplierService;
            _workContext = workContext;
            _partService = partService;
            _delNumberService = delNumberService;
            _chargeNumberService = chargeNumberService;
            _reportService = reportService;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new OrderListModel();
            model.AvailableStatus = OrderStatus.Created.ToSelectList(false).ToList();
            model.AvailableStatus.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = null, Selected = true });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult OrderList(DataSourceRequest command, OrderListModel model)
        {
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            if (_workContext.CurrentUser.IsAdmin())
            {
                var orders2 = _orderService
                    .GetAllOrders(
                        model.SearchOrderNumber,
                        model.Status,
                        model.CreateDate,
                        model.StartDate,
                        model.EndDate,
                        overridePublished,
                        showHidden: true,
                        pageIndex: command.Page - 1,
                        pageSize: command.PageSize);

                var gridModel2 = new DataSourceResult
                {
                    Data = orders2.Select(PrepareOrderListModel),
                    Total = orders2.TotalCount
                };

                return Json(gridModel2);
            }

            var orders = _orderService
                .GetAllOrders(
                    model.SearchOrderNumber,
                    model.Status,
                    model.CreateDate,
                    model.StartDate,
                    model.EndDate,
                    overridePublished,
                    showHidden: true);

            var crOrders = new List<Order>();
            var cOrders = new List<Order>();
            if (_workContext.CurrentUser.CustomerRegions.Any())
                crOrders = orders.Where(x => _workContext.CurrentUser.CustomerRegions.Contains(x.Supplier.Customer.StateProvince)).ToList();

            if (_workContext.CurrentUser.Customers.Any())
                cOrders = orders.Where(x => _workContext.CurrentUser.Customers.Contains(x.Supplier.Customer)).ToList();

            var result = crOrders.Concat(cOrders).Distinct().ToList();

            var pagedList = new PagedList<Order>(result, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(PrepareOrderListModel),
                Total = pagedList.TotalCount
            };

            return Json(gridModel);
        }

        private OrderModel PrepareOrderListModel(Order x)
        {
            var model = x.ToModel();

            if (!x.StartDate.HasValue)
            {
                model.OrderStatus = _localizationService.GetResource("Admin.Order.Status.Created");
                model.OrderStatusId = (int)OrderStatus.Created;
            }
            else if (x.EndDate.HasValue)
            {
                model.OrderStatus = _localizationService.GetResource("Admin.Order.Status.Finished");
                model.OrderStatusId = (int)OrderStatus.Finished;
            }
            else
            {
                model.OrderStatus = _localizationService.GetResource("Admin.Order.Status.InProcess");
                model.OrderStatusId = (int)OrderStatus.InProcess;
            }

            //model.CheckedPartsQuantity = _reportService.GetCheckedOrderQuantity(x.Id);

            return model;
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new OrderModel();
            PrepareOrderModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(OrderModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {

                var order = model.ToEntity();
                order.CreatedOnUtc = DateTime.UtcNow;
                order.UpdatedOnUtc = DateTime.UtcNow;

                _orderService.InsertOrder(order);

                //activity log
                _userActivityService.InsertActivity("AddNewOrder", _localizationService.GetResource("ActivityLog.AddNewOrder"), order.Number);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Orders.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = order.Id });
                }
                return RedirectToAction("List");
            }

            PrepareOrderModel(model);
            return View(model);
        }

        private void PrepareOrderModel(OrderModel model)
        {
            model.StartDate = model.StartDate ?? new DateTime?();
            model.EndDate = model.EndDate ?? new DateTime?();
            model.Published = model.Published != false;

            model.AvailableSuppliers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Suppliers.Order.Select.Supplier"), Value = "0" });
            if (_workContext.CurrentUser.IsAdmin())
            {
                var suppliers = _supplierService.GetAllSuppliers();
                foreach (var supplier in suppliers)
                {
                    model.AvailableSuppliers.Add(new SelectListItem
                    {
                        Text = supplier.Name,
                        Value = supplier.Id.ToString()
                    });
                }
            }
            else
            {
                foreach (var region in _workContext.CurrentUser.CustomerRegions)
                {
                    var suppliers = _supplierService.GetAllSuppliers(countryId: region.CountryId, stateId: region.Id);
                    foreach (var supplier in suppliers)
                    {
                        model.AvailableSuppliers.Add(new SelectListItem
                        {
                            Text = supplier.Name,
                            Value = supplier.Id.ToString()
                        });
                    }
                }

                foreach (var customer in _workContext.CurrentUser.Customers)
                {
                    foreach (var supplier in _supplierService.GetSuppliersByCustomer(customer.Id))
                    {
                        model.AvailableSuppliers.Add(new SelectListItem
                        {
                            Text = supplier.Name,
                            Value = supplier.Id.ToString()
                        });
                    }
                }
            }

            model.AvailablePartNumbers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Suppliers.Order.Select.PartNumber"), Value = "0" });
            model.AvailableDeliveryNumbers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Suppliers.Order.Select.DeliveryNumber"), Value = "0" });

            var parts = _partService.GetAllOrderParts(model.Id);
            if (parts == null) return;
            foreach (var part in parts)
            {
                model.AvailablePartNumbers.Add(new SelectListItem
                {
                    Text = part.SerNumber,
                    Value = part.Id.ToString()
                });
            }

            var delNumbers = new List<DeliveryNumber>();
            foreach (var part in parts)
            {
                delNumbers.AddRange(_delNumberService.GetAllPartDeliveryNumbers(part.Id));
            }
            foreach (var delNumber in delNumbers)
            {
                model.AvailableDeliveryNumbers.Add(new SelectListItem
                {
                    Text = delNumber.Number,
                    Value = delNumber.Id.ToString()
                });
            }

            var chargeNumbers = new List<ChargeNumber>();
            if (delNumbers.Any())
            {
                foreach (var delNumber in delNumbers)
                {
                    chargeNumbers.AddRange(_chargeNumberService.GetAllDeliveryChargeNumbers(delNumber.Id));
                }
            }

            model.IsChargeNumberQuantityAvailable = false;
            if (chargeNumbers.Any())
            {
                model.TotalPartsQuantity = chargeNumbers.Select(x => x.Quantity ?? 0).Sum();
                model.IsChargeNumberQuantityAvailable = true;
            }
        }

        public virtual ActionResult Edit(int id, string tab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                //No order found with the specified id
                return RedirectToAction("List");

            var model = order.ToModel();

            PrepareOrderModel(model);
            model.TabReload = tab;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(OrderModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.Id);
            if (order == null || order.Deleted)
                //No Customer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var part = _partService.GetPartById(model.PartId);

                if (part != null && (part.Name != model.PartName || part.SerNumber != model.PartSerNumer))
                {
                    part.Name = model.PartName;
                    part.SerNumber = model.PartSerNumer;

                    _partService.UpdatePart(part);
                    _userActivityService.InsertActivity("AddNewPart", _localizationService.GetResource("ActivityLog.AddNewPart"), part.Name);
                }

                if (order.CheckedPartsQuantity != 0)
                {
                    model.CheckedPartsQuantity = order.CheckedPartsQuantity;
                }

                order = model.ToEntity(order);

                order.UpdatedOnUtc = DateTime.UtcNow;
                //order.PartId = part.Id;
                _orderService.UpdateOrder(order);

                //activity log
                _userActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), order.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Orders.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = order.Id });
                }
                return RedirectToAction("List");
            }

            PrepareOrderModel(model);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No Customer found with the specified id
                return RedirectToAction("List");

            var reports = _reportService.GetAllReports(orderId: id);
            foreach (var report in reports)
            {
                _reportService.DeleteReport(report);
            }

            _orderService.DeleteOrder(order);

            //activity log
            _userActivityService.InsertActivity("DeleteOrder", _localizationService.GetResource("ActivityLog.DeleteOrder"), order.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Orders.Deleted"));
            return RedirectToAction("List");
        }

        #region AddParts
        public virtual ActionResult GetAllOrderParts(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            if (orderId == 0)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var query = _partService
                .GetAllOrderParts(orderId);

            var resources = query
                .Select(x => new OrderPartsModel
                {
                    OrderId = orderId,
                    Id = x.Id,
                    SerNumber = x.SerNumber
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = resources.PagedForCommand(command),
                Total = resources.Count()
            };

            return Json(gridModel);
        }

        public virtual ActionResult DeleteOrderParts(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var part = _partService.GetPartById(id);
            if (part == null)
                throw new ArgumentException("No part found with the specified id");

            _partService.DeletePart(part);

            return new NullJsonResult();
        }

        [ValidateInput(false)]
        public virtual ActionResult AddOrderParts(int orderId, string serNumber)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(serNumber))
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var part = new Part()
            {
                OrderId = orderId,
                SerNumber = serNumber.Trim(),
            };

            _partService.InsertPart(part);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DeliveryNumber
        public virtual ActionResult GetAllDeliveryNumbers(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            if (orderId == 0)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var parts = _partService.GetAllOrderParts(orderId);
            var query = new List<DeliveryNumber>();
            foreach (var part in parts)
            {
                query.AddRange(_delNumberService.GetAllPartDeliveryNumbers(part.Id));
            }

            var resources = query
                .Select(x => new OrderDeliveryNumberModel
                {
                    OrderId = orderId,
                    Id = x.Id,
                    DeliveryNumber = x.Number,
                    PartNumber = x.Part.SerNumber
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = resources.PagedForCommand(command),
                Total = resources.Count
            };

            return Json(gridModel);
        }

        public virtual ActionResult DeliveryNumberDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var deliveryNumber = _delNumberService.GetDeliveryNumberById(id);
            if (deliveryNumber == null)
                throw new ArgumentException("No delivery number found with the specified id");

            var reports = _reportService.GetAllReports().Where(x => x.DeliveryNumberId == id);
            foreach (var report in reports)
            {
                _reportService.DeleteReport(report);
            }

            var chargeNumbers = _chargeNumberService.GetAllDeliveryChargeNumbers(id);
            foreach (var chargeNumber in chargeNumbers)
            {
                _chargeNumberService.DeleteChargeNumber(chargeNumber);
            }

            _delNumberService.DeleteDeliveryNumber(deliveryNumber);

            return new NullJsonResult();
        }

        [ValidateInput(false)]
        public virtual ActionResult DeliveryNumberAdd(int partId, string delNumber)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(delNumber))
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var part = _partService.GetPartById(partId);
            if (part == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var deliveryNumber = new DeliveryNumber
            {
                //OrderId = orderId,
                PartId = part.Id,
                Number = delNumber.Trim()
            };

            _delNumberService.InsertDeliveryNumber(deliveryNumber);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ChargeNumber
        public virtual ActionResult GetAllChargeNumbers(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            if (orderId == 0)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var parts = _partService.GetAllOrderParts(orderId);

            var delNumbers = new List<DeliveryNumber>();
            foreach (var part in parts)
            {
                delNumbers.AddRange(_delNumberService.GetAllPartDeliveryNumbers(part.Id));
            }

            var queries = new List<ChargeNumber>();

            foreach (var delNumber in delNumbers)
            {
                queries.AddRange(_chargeNumberService.GetAllDeliveryChargeNumbers(delNumber.Id));
            }

            var resources = queries
                .Select(x => new OrderChargeNumberModel
                {
                    DeliverNumber = x.DeliveryNumber.Number,
                    Id = x.Id,
                    ChargeNumber = x.Number,
                    ChargeNumberQuantity = x.Quantity ?? 0
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = resources.PagedForCommand(command),
                Total = resources.Count
            };

            return Json(gridModel);
        }

        public virtual ActionResult ChargeNumberDelete(int id, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var chargeNumber = _chargeNumberService.GetChargeNumberById(id);
            if (chargeNumber == null)
                throw new ArgumentException("No charge number found with the specified id");

            var reports = _reportService.GetAllReports().Where(x => x.ChargeNumberId == id);
            foreach (var report in reports)
            {
                _reportService.DeleteReport(report);
            }

            if (orderId > 0)
            {
                var order = _orderService.GetOrderById(orderId);
                order.TotalPartsQuantity -= chargeNumber.Quantity ?? 0;
                _orderService.UpdateOrder(order);
            }

            _chargeNumberService.DeleteChargeNumber(chargeNumber);
            return new NullJsonResult();
        }

        [ValidateInput(false)]
        public virtual ActionResult ChargeNumberAdd(int deliveryNumberId, string charNumber, int quantity, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(charNumber))
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var deliveryNumber = _delNumberService.GetDeliveryNumberById(deliveryNumberId);
            if (deliveryNumber == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var chargeNumber = new ChargeNumber
            {
                DeliveryNumberId = deliveryNumber.Id,
                Number = charNumber.Trim(),
                Quantity = quantity
            };

            _chargeNumberService.InsertChargeNumber(chargeNumber);

            if (orderId > 0)
            {
                var order = _orderService.GetOrderById(orderId);
                order.TotalPartsQuantity = _chargeNumberService.GetAllDeliveryChargeNumbers(deliveryNumberId).Select(x => x.Quantity ?? 0).Sum();
                _orderService.UpdateOrder(order);
            }

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}