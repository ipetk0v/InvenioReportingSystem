using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Orders;
using Invenio.Core;
using Invenio.Core.Domain.Orders;
using Invenio.Services.Supplier;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Orders;
using Invenio.Services.Security;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core.Domain.Customers;
using Invenio.Core.Domain.Suppliers;
using Invenio.Core.Domain.Users;
using Invenio.Services;
using Invenio.Services.Customers;
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
        private readonly IReportService _reportService;
        private readonly ICustomerService _customerService;
        private readonly IOrderAttributeService _orderAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public OrderController(
            IPermissionService permissionService,
            IOrderService orderService,
            IUserActivityService userActivityService,
            ILocalizationService localizationService,
            ISupplierService supplierService,
            IWorkContext workContext,
            IReportService reportService,
            ICustomerService customerService,
            IOrderAttributeService orderAttributeService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService
            )
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _userActivityService = userActivityService;
            _localizationService = localizationService;
            _supplierService = supplierService;
            _workContext = workContext;
            _reportService = reportService;
            _customerService = customerService;
            _orderAttributeService = orderAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new OrderListModel { AvailableStatus = OrderStatus.Created.ToSelectList(false).ToList() };
            model.AvailableStatus.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = null, Selected = true });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Orders.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Orders.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Orders.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            model.AvailableSuppliers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Orders.List.SupplierId.All"), Value = "0" });
            model.AvailableCustomers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Orders.List.CustomerId.All"), Value = "0" });

            //admin
            if (_workContext.CurrentUser.IsAdmin())
            {
                var customers2 = _customerService.GetAllCustomers();
                customers2.ToList().ForEach(x =>
                    model.AvailableCustomers.Add(new SelectListItem { Text = x.Name, Value = x.Id.ToString() }));

                return View(model);
            }

            //customer
            var customersList = _customerService
                .GetAllCustomers();

            var crCustomer = new List<Customer>();
            var rCustomers = new List<Customer>();
            if (_workContext.CurrentUser.CustomerRegions.Any())
                crCustomer = customersList.Where(x => _workContext.CurrentUser.CustomerRegions.Contains(x.StateProvince)).ToList();

            if (_workContext.CurrentUser.Customers.Any())
                rCustomers = customersList.Where(x => _workContext.CurrentUser.Customers.Contains(x)).ToList();

            var customerListResult = crCustomer.Concat(rCustomers).Distinct().ToList();
            customerListResult.ForEach(x =>
                model.AvailableCustomers.Add(new SelectListItem { Text = x.Name, Value = x.Id.ToString() }));

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
                        showHidden: true)
                        .OrderByDescending(x => x.Published)
                        .ThenByDescending(x => x.Id)
                        .ToList();

                if (model.SupplierId > 0)
                    orders2 = orders2.Where(x => x.SupplierId == model.SupplierId).ToList();

                if (model.CustomerId > 0)
                    orders2 = orders2.Where(x => x.Supplier.CustomerId == model.CustomerId).ToList();

                var resultOrders = new PagedList<Order>(orders2, command.Page - 1, command.PageSize);
                var gridModel2 = new DataSourceResult
                {
                    Data = resultOrders.Select(PrepareOrderListModel),
                    Total = resultOrders.TotalCount
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
                    showHidden: true)
                    .OrderByDescending(x => x.Published)
                    .ThenByDescending(x => x.Id)
                    .ToList();

            if (model.SupplierId > 0)
                orders = orders.Where(x => x.SupplierId == model.SupplierId).ToList();

            if (model.CustomerId > 0)
                orders = orders.Where(x => x.Supplier.CustomerId == model.CustomerId).ToList();

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

        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult GetSuppliersByCustomerId(string customerId, bool? addAsterisk)
        {
            //permission validation is not required here


            // This action method gets called via an ajax request
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var customer = _customerService.GetCustomerById(Convert.ToInt32(customerId));
            var states = customer != null ? _supplierService.GetSuppliersByCustomer(customer.Id).ToList() : new List<Supplier>();
            var result = (from s in states
                          select new { id = s.Id, name = s.Name }).ToList();
            if (addAsterisk.HasValue && addAsterisk.Value)
            {
                //asterisk
                result.Insert(0, new { id = 0, name = "*" });
            }
            else
            {
                if (customer == null)
                {
                    //customer is not selected ("choose customer" item)
                    result.Insert(0, new { id = 0, name = _localizationService.GetResource("Admin.Customer.SelectCustomer") });
                }
                else
                {
                    //some country is selected
                    result.Insert(0,
                        !result.Any()
                            ? new { id = 0, name = _localizationService.GetResource("Admin.Supplier.IsEmpty") }
                            : new { id = 0, name = _localizationService.GetResource("Admin.Address.SelectSupplier") });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
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

            var avOrderAttributes = _orderAttributeService.GetAllOrderAttributes();
            foreach (var oa in avOrderAttributes)
            {
                model.AvailableOrderAttributes.Add(new OrderAttributeModel
                {
                    Name = oa.Name,
                    Id = oa.Id,
                });
            }

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
                if (order.CheckedPartsQuantity != 0)
                {
                    model.CheckedPartsQuantity = order.CheckedPartsQuantity;
                }

                order = model.ToEntity(order);

                order.UpdatedOnUtc = DateTime.UtcNow;
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

        #region OrderAttribute
        [HttpPost]
        public virtual ActionResult OrderAttributeMappingList(DataSourceRequest command, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var attributes = _orderAttributeService.GetOrderAttributeMappingsByOrderId(orderId);
            var attributesModel = attributes
                .Select(x =>
                {
                    var attributeModel = new OrderModel.OrderAttributeMappingModel
                    {
                        Id = x.Id,
                        OrderId = x.OrderId,
                        OrderAttribute = _orderAttributeService.GetOrderAttributeById(x.OrderAttributeId).Name,
                        OrderAttributeId = x.OrderAttributeId,
                        TextPrompt = x.TextPrompt,
                        //IsRequired = x.IsRequired,
                        //AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
                        //AttributeControlTypeId = x.AttributeControlTypeId,
                        //DisplayOrder = x.DisplayOrder
                    };

                    attributeModel.ShouldHaveValues = true;
                    attributeModel.TotalValues = x.OrderAttributeValues.Count;

                    //if (x.ValidationRulesAllowed())
                    //{
                    //    var validationRules = new StringBuilder(string.Empty);
                    //    attributeModel.ValidationRulesAllowed = true;
                    //    if (x.ValidationMinLength != null)
                    //        validationRules.AppendFormat("{0}: {1}<br />",
                    //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.MinLength"),
                    //            x.ValidationMinLength);
                    //    if (x.ValidationMaxLength != null)
                    //        validationRules.AppendFormat("{0}: {1}<br />",
                    //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.MaxLength"),
                    //            x.ValidationMaxLength);
                    //    if (!string.IsNullOrEmpty(x.ValidationFileAllowedExtensions))
                    //        validationRules.AppendFormat("{0}: {1}<br />",
                    //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
                    //            HttpUtility.HtmlEncode(x.ValidationFileAllowedExtensions));
                    //    if (x.ValidationFileMaximumSize != null)
                    //        validationRules.AppendFormat("{0}: {1}<br />",
                    //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.FileMaximumSize"),
                    //            x.ValidationFileMaximumSize);
                    //    if (!string.IsNullOrEmpty(x.DefaultValue))
                    //        validationRules.AppendFormat("{0}: {1}<br />",
                    //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.DefaultValue"),
                    //            HttpUtility.HtmlEncode(x.DefaultValue));
                    //    attributeModel.ValidationRulesString = validationRules.ToString();
                    //}


                    //currenty any attribute can have condition. why not?
                    attributeModel.ConditionAllowed = true;
                    //var conditionAttribute = _orderAttributeParser.ParseOrderAttributeMappings(x.ConditionAttributeXml).FirstOrDefault();
                    //var conditionValue = _orderAttributeParser.ParseOrderAttributeValues(x.ConditionAttributeXml).FirstOrDefault();
                    //if (conditionAttribute != null && conditionValue != null)
                    //    attributeModel.ConditionString = string.Format("{0}: {1}",
                    //        HttpUtility.HtmlEncode(conditionAttribute.OrderAttribute.Name),
                    //        HttpUtility.HtmlEncode(conditionValue.Name));
                    //else
                    //    attributeModel.ConditionString = string.Empty;
                    return attributeModel;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = attributesModel,
                Total = attributesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public virtual ActionResult OrderAttributeMappingInsert(OrderModel.OrderAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his orders
            //if (_workContext.CurrentVendor != null && order.VendorId != _workContext.CurrentVendor.Id)
            //{
            //    return Content("This is not your order");
            //}

            //ensure this attribute is not mapped yet
            //if (_orderAttributeService.GetOrderAttributeMappingsByOrderId(order.Id).Any(x => x.OrderAttributeId == model.OrderAttributeId))
            //{
            //    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.AlreadyExists") });
            //}

            //insert mapping
            var orderAttributeMapping = new OrderAttributeMapping
            {
                OrderId = model.OrderId,
                OrderAttributeId = model.OrderAttributeId,
                TextPrompt = model.TextPrompt,
                //IsRequired = model.IsRequired,
                //AttributeControlTypeId = model.AttributeControlTypeId,
                //DisplayOrder = model.DisplayOrder
            };
            _orderAttributeService.InsertOrderAttributeMapping(orderAttributeMapping);

            //predefined values
            var predefinedValues = _orderAttributeService.GetPredefinedOrderAttributeValues(model.OrderAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = new OrderAttributeValue
                {
                    OrderAttributeMappingId = orderAttributeMapping.Id,
                    //AttributeValueType = AttributeValueType.Simple,
                    Name = predefinedValue.Name,
                    //PriceAdjustment = predefinedValue.PriceAdjustment,
                    //WeightAdjustment = predefinedValue.WeightAdjustment,
                    //Cost = predefinedValue.Cost,
                    //IsPreSelected = predefinedValue.IsPreSelected,
                    //DisplayOrder = predefinedValue.DisplayOrder
                };
                _orderAttributeService.InsertOrderAttributeValue(pav);
                //locales
                var languages = _languageService.GetAllLanguages(true);
                //localization
                foreach (var lang in languages)
                {
                    var name = predefinedValue.GetLocalized(x => x.Name, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(name))
                        _localizedEntityService.SaveLocalizedValue(pav, x => x.Name, name, lang.Id);
                }
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual ActionResult OrderAttributeMappingUpdate(OrderModel.OrderAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(model.Id);
            if (orderAttributeMapping == null)
                throw new ArgumentException("No order attribute mapping found with the specified id");

            var order = _orderService.GetOrderById(orderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his orders
            //if (_workContext.CurrentVendor != null && order.VendorId != _workContext.CurrentVendor.Id)
            //    return Content("This is not your order");

            orderAttributeMapping.OrderAttributeId = model.OrderAttributeId;
            orderAttributeMapping.TextPrompt = model.TextPrompt;
            //orderAttributeMapping.IsRequired = model.IsRequired;
            //orderAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
            //orderAttributeMapping.DisplayOrder = model.DisplayOrder;
            _orderAttributeService.UpdateOrderAttributeMapping(orderAttributeMapping);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual ActionResult OrderAttributeMappingDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(id);
            if (orderAttributeMapping == null)
                throw new ArgumentException("No order attribute mapping found with the specified id");

            var orderId = orderAttributeMapping.OrderId;
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            _orderAttributeService.DeleteOrderAttributeMapping(orderAttributeMapping);

            return new NullJsonResult();
        }


        #endregion

        #region Order attribute values

        //list
        public virtual ActionResult EditAttributeValues(int orderAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(orderAttributeMappingId);
            if (orderAttributeMapping == null)
                throw new ArgumentException("No order attribute mapping found with the specified id");

            var order = _orderService.GetOrderById(orderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var model = new OrderModel.OrderAttributeValueListModel
            {
                OrderName = order.Name,
                OrderId = orderAttributeMapping.OrderId,
                OrderAttributeName = orderAttributeMapping.OrderAttribute.Name,
                OrderAttributeMappingId = orderAttributeMapping.Id,
            };

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult OrderAttributeValueList(int orderAttributeMappingId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(orderAttributeMappingId);
            if (orderAttributeMapping == null)
                throw new ArgumentException("No order attribute mapping found with the specified id");

            var order = _orderService.GetOrderById(orderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var values = _orderAttributeService.GetOrderAttributeValues(orderAttributeMappingId);
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x =>
                {
                    return new OrderModel.OrderAttributeValueModel
                    {
                        Id = x.Id,
                        OrderAttributeMappingId = x.OrderAttributeMappingId,
                        Name = x.Name,
                        ParentAttributeName = x.ParentAttributeValueId.HasValue
                            ? _orderAttributeService.GetOrderAttributeValueById(x.ParentAttributeValue.Id).Name
                            : string.Empty
                    };
                }),
                Total = values.Count()
            };

            return Json(gridModel);
        }

        //create
        public virtual ActionResult OrderAttributeValueCreatePopup(int orderAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(orderAttributeMappingId);
            if (orderAttributeMapping == null)
                throw new ArgumentException("No order attribute mapping found with the specified id");

            var order = _orderService.GetOrderById(orderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var parentSelectList = PrepareParentAttributeValues(orderAttributeMapping, order);

            var model = new OrderModel.OrderAttributeValueModel
            {
                OrderAttributeMappingId = orderAttributeMappingId,
                ParentAttributeValues = parentSelectList
            };


            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult OrderAttributeValueCreatePopup(string btnId, string formId, OrderModel.OrderAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(model.OrderAttributeMappingId);
            if (orderAttributeMapping == null)
                //No order attribute found with the specified id
                return RedirectToAction("List", "Order");

            var order = _orderService.GetOrderById(orderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            if (ModelState.IsValid)
            {
                var pav = new OrderAttributeValue
                {
                    OrderAttributeMappingId = model.OrderAttributeMappingId,
                    ParentAttributeValueId = model.ParentAttributeValueId == 0 ? null : model.ParentAttributeValueId,
                    Name = model.Name,
                };

                _orderAttributeService.InsertOrderAttributeValue(pav);
                //UpdateLocales(pav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            return View(model);
        }

        //edit
        public virtual ActionResult OrderAttributeValueEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var oav = _orderAttributeService.GetOrderAttributeValueById(id);
            if (oav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Order");

            var order = _orderService.GetOrderById(oav.OrderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var parentAttributeValues = PrepareParentAttributeValues(oav.OrderAttributeMapping, order);

            var model = new OrderModel.OrderAttributeValueModel
            {
                OrderAttributeMappingId = oav.OrderAttributeMappingId,
                ParentAttributeValueId = oav.ParentAttributeValueId == 0 ? null : oav.ParentAttributeValueId,
                Name = oav.Name,
                ParentAttributeValues = parentAttributeValues
            };

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = oav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        private IList<SelectListItem> PrepareParentAttributeValues(OrderAttributeMapping orderAttributeMapping, Order order)
        {
            if (orderAttributeMapping == null)
                return new List<SelectListItem>();

            if (order == null)
                return new List<SelectListItem>();

            var parentOrderAttributeId = _orderAttributeService
                                             .GetOrderAttributeById(orderAttributeMapping.OrderAttributeId).ParentOrderAttributeId ?? 0;

            var parentAttribute = _orderAttributeService.GetOrderAttributeById(parentOrderAttributeId);

            var parentSelectList = new List<SelectListItem> { new SelectListItem { Text = "...", Value = "0" } };
            if (parentAttribute != null)
            {
                //moga da napravq service da vzima i attribute i order id
                var parentOrderAttributeMapping = _orderAttributeService
                    .GetOrderAttributeMappingsByOrderAttributeId(parentAttribute.Id)
                    .Where(x => x.OrderId == order.Id)
                    .FirstOrDefault();

                if (parentOrderAttributeMapping != null)
                {
                    var parentAttributeValues = _orderAttributeService.GetOrderAttributeValues(parentOrderAttributeMapping.Id);

                    foreach (var item in parentAttributeValues)
                    {
                        parentSelectList.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
                    }
                }
            }

            return parentSelectList;
        }

        [HttpPost]
        public virtual ActionResult OrderAttributeValueEditPopup(string btnId, string formId, OrderModel.OrderAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var pav = _orderAttributeService.GetOrderAttributeValueById(model.Id);
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Order");

            var order = _orderService.GetOrderById(pav.OrderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            if (ModelState.IsValid)
            {
                pav.Name = model.Name;
                pav.ParentAttributeValueId = model.ParentAttributeValueId == 0 ? null : model.ParentAttributeValueId;

                _orderAttributeService.UpdateOrderAttributeValue(pav);

                //UpdateLocales(pav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            return View(model);
        }

        //delete
        [HttpPost]
        public virtual ActionResult OrderAttributeValueDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var pav = _orderAttributeService.GetOrderAttributeValueById(id);
            if (pav == null)
                throw new ArgumentException("No order attribute value found with the specified id");

            var order = _orderService.GetOrderById(pav.OrderAttributeMapping.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            _orderAttributeService.DeleteOrderAttributeValue(pav);

            return new NullJsonResult();
        }
        #endregion
    }
}