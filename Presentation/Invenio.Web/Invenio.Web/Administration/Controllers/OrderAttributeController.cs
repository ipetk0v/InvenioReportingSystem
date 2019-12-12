using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Orders;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Orders;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Orders;
using Invenio.Services.Security;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core;
using System.Web;
using System.Text;
using Invenio.Services;
using System.Collections.Generic;

namespace Invenio.Admin.Controllers
{
    public class OrderAttributeController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderAttributeService _orderAttributeService;
        private readonly IUserActivityService _userActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        public OrderAttributeController(
            IPermissionService permissionService,
            IOrderAttributeService orderAttributeService,
            IUserActivityService userActivityService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IOrderService orderService,
            IWorkContext workContext
            )
        {
            _permissionService = permissionService;
            _orderAttributeService = orderAttributeService;
            _userActivityService = userActivityService;
            _localizationService = localizationService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _orderService = orderService;
            _workContext = workContext;
        }

        [NonAction]
        protected virtual void UpdateLocales(OrderAttribute orderAttribute, OrderAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(orderAttribute,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(orderAttribute,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdateLocales(PredefinedOrderAttributeValue ppav, PredefinedOrderAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(ppav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        //list
        public virtual ActionResult Index()
        {
            return RedirectToAction(nameof(List));
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            var orderAttributes = _orderAttributeService.GetAllOrderAttributes(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orderAttributes.Select(x => PrepareOrderAttributeModel(x)),
                Total = orderAttributes.TotalCount
            };

            return Json(gridModel);
        }

        private OrderAttributeModel PrepareOrderAttributeModel(OrderAttribute oa)
        {
            var orderAttribute = new OrderAttributeModel();

            if (oa != null)
                orderAttribute = oa.ToModel();

            var parentOrderAttributeList = _orderAttributeService
                .GetAllOrderAttributes()
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });

            orderAttribute.ParentOrderAttributeList.Add(new SelectListItem { Text = "...", Value = "0" });
            foreach (var item in parentOrderAttributeList)
            {
                orderAttribute.ParentOrderAttributeList.Add(item);
            }

            return orderAttribute;
        }

        //create
        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = PrepareOrderAttributeModel(null);
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(OrderAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var orderAttribute = model.ToEntity();

                if (model.ParentOrderAttributeId == 0)
                    orderAttribute.ParentOrderAttributeId = null;

                _orderAttributeService.InsertOrderAttribute(orderAttribute);
                UpdateLocales(orderAttribute, model);

                //activity log
                _userActivityService.InsertActivity("AddNewOrderAttribute", _localizationService.GetResource("ActivityLog.AddNewOrderAttribute"), orderAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.OrderAttributes.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction(nameof(Edit), new { id = orderAttribute.Id });
                }
                return RedirectToAction(nameof(List));

            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual ActionResult GetParentAttributeValue(int parentId)
        {
            var orderAttributes = new List<object>();

            var orderAttribute = _orderAttributeService.GetOrderAttributeById(parentId);
            if (orderAttribute == null || !orderAttribute.ParentOrderAttributeId.HasValue)
                return Json(new { result = orderAttributes.ToArray() });

            var parentAttributeValues = _orderAttributeService
                .GetOrderAttributeMappingsByOrderAttributeId(orderAttribute.ParentOrderAttributeId.Value);

            foreach (var pav in parentAttributeValues)
            {
                var ob = new { pav.Id, Name = pav.TextPrompt };
                orderAttributes.Add(ob);
            }

            //var result = orderAttributes.ToArray();
            return Json(new { result = orderAttributes.ToArray() });
        }

        //edit
        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttribute = _orderAttributeService.GetOrderAttributeById(id);
            if (orderAttribute == null)
                //No order attribute found with the specified id
                return RedirectToAction(nameof(List));

            var model = PrepareOrderAttributeModel(orderAttribute);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = orderAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = orderAttribute.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(OrderAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttribute = _orderAttributeService.GetOrderAttributeById(model.Id);
            if (orderAttribute == null)
                //No order attribute found with the specified id
                return RedirectToAction(nameof(List));

            if (ModelState.IsValid)
            {
                orderAttribute = model.ToEntity(orderAttribute);

                if (model.ParentOrderAttributeId == 0)
                    orderAttribute.ParentOrderAttributeId = null;

                _orderAttributeService.UpdateOrderAttribute(orderAttribute);

                //UpdateLocales(orderAttribute, model);

                //activity log
                _userActivityService.InsertActivity("EditOrderAttribute", _localizationService.GetResource("ActivityLog.EditOrderAttribute"), orderAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.OrderAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction(nameof(Edit), new { id = orderAttribute.Id });
                }
                return RedirectToAction(nameof(List));
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttribute = _orderAttributeService.GetOrderAttributeById(id);
            if (orderAttribute == null)
                //No order attribute found with the specified id
                return RedirectToAction(nameof(List));

            _orderAttributeService.DeleteOrderAttribute(orderAttribute);

            //activity log
            _userActivityService.InsertActivity("DeleteOrderAttribute", _localizationService.GetResource("ActivityLog.DeleteOrderAttribute"), orderAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.OrderAttributes.Deleted"));
            return RedirectToAction(nameof(List));
        }

        //used by orders
        [HttpPost]
        public virtual ActionResult UsedByOrders(DataSourceRequest command, int orderAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            var orders = _orderService.GetOrdersByOrderAtributeId(
                orderAttributeId: orderAttributeId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    return new OrderAttributeModel.UsedByOrderModel
                    {
                        Id = x.Id,
                        OrderNumber = x.Number,
                        Published = x.Published
                    };
                }),
                Total = orders.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public virtual ActionResult PredefinedOrderAttributeValueList(int orderAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            var values = _orderAttributeService.GetPredefinedOrderAttributeValues(orderAttributeId);
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x =>
                {
                    return new PredefinedOrderAttributeValueModel
                    {
                        Id = x.Id,
                        OrderAttributeId = x.OrderAttributeId,
                        Name = x.Name,
                        //PriceAdjustment = x.PriceAdjustment,
                        //PriceAdjustmentStr = x.PriceAdjustment.ToString("G29"),
                        //WeightAdjustment = x.WeightAdjustment,
                        //WeightAdjustmentStr = x.WeightAdjustment.ToString("G29"),
                        //Cost = x.Cost,
                        //IsPreSelected = x.IsPreSelected,
                        DisplayOrder = x.DisplayOrder
                    };
                }),
                Total = values.Count()
            };

            return Json(gridModel);
        }

        //create
        public virtual ActionResult PredefinedOrderAttributeValueCreatePopup(int orderAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttribute = _orderAttributeService.GetOrderAttributeById(orderAttributeId);
            if (orderAttribute == null)
                throw new ArgumentException("No order attribute found with the specified id");

            var model = new PredefinedOrderAttributeValueModel();
            model.OrderAttributeId = orderAttributeId;

            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult PredefinedOrderAttributeValueCreatePopup(string btnId, string formId, PredefinedOrderAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orderAttribute = _orderAttributeService.GetOrderAttributeById(model.OrderAttributeId);
            if (orderAttribute == null)
                throw new ArgumentException("No order attribute found with the specified id");

            if (ModelState.IsValid)
            {
                var ppav = new PredefinedOrderAttributeValue
                {
                    OrderAttributeId = model.OrderAttributeId,
                    Name = model.Name,
                    //PriceAdjustment = model.PriceAdjustment,
                    //WeightAdjustment = model.WeightAdjustment,
                    //Cost = model.Cost,
                    //IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };

                _orderAttributeService.InsertPredefinedOrderAttributeValue(ppav);
                UpdateLocales(ppav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public virtual ActionResult PredefinedOrderAttributeValueEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var ppav = _orderAttributeService.GetPredefinedOrderAttributeValueById(id);
            if (ppav == null)
                throw new ArgumentException("No order attribute value found with the specified id");

            var model = new PredefinedOrderAttributeValueModel
            {
                OrderAttributeId = ppav.OrderAttributeId,
                Name = ppav.Name,
                //PriceAdjustment = ppav.PriceAdjustment,
                //WeightAdjustment = ppav.WeightAdjustment,
                //Cost = ppav.Cost,
                //IsPreSelected = ppav.IsPreSelected,
                DisplayOrder = ppav.DisplayOrder
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = ppav.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult PredefinedOrderAttributeValueEditPopup(string btnId, string formId, PredefinedOrderAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var ppav = _orderAttributeService.GetPredefinedOrderAttributeValueById(model.Id);
            if (ppav == null)
                throw new ArgumentException("No order attribute value found with the specified id");

            if (ModelState.IsValid)
            {
                ppav.Name = model.Name;
                //ppav.PriceAdjustment = model.PriceAdjustment;
                //ppav.WeightAdjustment = model.WeightAdjustment;
                //ppav.Cost = model.Cost;
                //ppav.IsPreSelected = model.IsPreSelected;
                ppav.DisplayOrder = model.DisplayOrder;
                _orderAttributeService.UpdatePredefinedOrderAttributeValue(ppav);

                UpdateLocales(ppav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public virtual ActionResult PredefinedOrderAttributeValueDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var ppav = _orderAttributeService.GetPredefinedOrderAttributeValueById(id);
            if (ppav == null)
                throw new ArgumentException("No predefined order attribute value found with the specified id");

            _orderAttributeService.DeletePredefinedOrderAttributeValue(ppav);

            return new NullJsonResult();
        }

        //[HttpPost]
        //public virtual ActionResult OrderAttributeMappingList(DataSourceRequest command, int orderId)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
        //        return AccessDeniedKendoGridJson();

        //    var order = _orderService.GetOrderById(orderId);
        //    if (order == null)
        //        throw new ArgumentException("No order found with the specified id");

        //    //a vendor should have access only to his orders
        //    //if (_workContext.CurrentVendor != null && order.VendorId != _workContext.CurrentVendor.Id)
        //    //    return Content("This is not your order");

        //    var attributes = _orderAttributeService.GetOrderAttributeMappingsByOrderId(orderId);
        //    var attributesModel = attributes
        //        .Select(x =>
        //        {
        //            var attributeModel = new OrderModel.OrderAttributeMappingModel
        //            {
        //                Id = x.Id,
        //                OrderId = x.OrderId,
        //                OrderAttribute = _orderAttributeService.GetOrderAttributeById(x.OrderAttributeId).Name,
        //                OrderAttributeId = x.OrderAttributeId,
        //                //TextPrompt = x.TextPrompt,
        //                //IsRequired = x.IsRequired,
        //                //AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
        //                //AttributeControlTypeId = x.AttributeControlTypeId,
        //                //DisplayOrder = x.DisplayOrder
        //            };


        //            //if (x.ShouldHaveValues())
        //            //{
        //            //    attributeModel.ShouldHaveValues = true;
        //            //    attributeModel.TotalValues = x.OrderAttributeValues.Count;
        //            //}

        //            //if (x.ValidationRulesAllowed())
        //            //{
        //            //    var validationRules = new StringBuilder(string.Empty);
        //            //    attributeModel.ValidationRulesAllowed = true;
        //            //    if (x.ValidationMinLength != null)
        //            //        validationRules.AppendFormat("{0}: {1}<br />",
        //            //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.MinLength"),
        //            //            x.ValidationMinLength);
        //            //    if (x.ValidationMaxLength != null)
        //            //        validationRules.AppendFormat("{0}: {1}<br />",
        //            //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.MaxLength"),
        //            //            x.ValidationMaxLength);
        //            //    if (!string.IsNullOrEmpty(x.ValidationFileAllowedExtensions))
        //            //        validationRules.AppendFormat("{0}: {1}<br />",
        //            //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
        //            //            HttpUtility.HtmlEncode(x.ValidationFileAllowedExtensions));
        //            //    if (x.ValidationFileMaximumSize != null)
        //            //        validationRules.AppendFormat("{0}: {1}<br />",
        //            //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.FileMaximumSize"),
        //            //            x.ValidationFileMaximumSize);
        //            //    if (!string.IsNullOrEmpty(x.DefaultValue))
        //            //        validationRules.AppendFormat("{0}: {1}<br />",
        //            //            _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.DefaultValue"),
        //            //            HttpUtility.HtmlEncode(x.DefaultValue));
        //            //    attributeModel.ValidationRulesString = validationRules.ToString();
        //            //}


        //            //currenty any attribute can have condition. why not?
        //            attributeModel.ConditionAllowed = true;
        //            //var conditionAttribute = _orderAttributeParser.ParseOrderAttributeMappings(x.ConditionAttributeXml).FirstOrDefault();
        //            //var conditionValue = _orderAttributeParser.ParseOrderAttributeValues(x.ConditionAttributeXml).FirstOrDefault();
        //            //if (conditionAttribute != null && conditionValue != null)
        //            //    attributeModel.ConditionString = string.Format("{0}: {1}",
        //            //        HttpUtility.HtmlEncode(conditionAttribute.OrderAttribute.Name),
        //            //        HttpUtility.HtmlEncode(conditionValue.Name));
        //            //else
        //            //    attributeModel.ConditionString = string.Empty;
        //            return attributeModel;
        //        })
        //        .ToList();

        //    var gridModel = new DataSourceResult
        //    {
        //        Data = attributesModel,
        //        Total = attributesModel.Count
        //    };

        //    return Json(gridModel);
        //}

        //[HttpPost]
        //public virtual ActionResult OrderAttributeMappingInsert(OrderModel.OrderAttributeMappingModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
        //        return AccessDeniedView();

        //    var order = _orderService.GetOrderById(model.OrderId);
        //    if (order == null)
        //        throw new ArgumentException("No order found with the specified id");

        //    //a vendor should have access only to his orders
        //    //if (_workContext.CurrentVendor != null && order.VendorId != _workContext.CurrentVendor.Id)
        //    //{
        //    //    return Content("This is not your order");
        //    //}

        //    //ensure this attribute is not mapped yet
        //    if (_orderAttributeService.GetOrderAttributeMappingsByOrderId(order.Id).Any(x => x.OrderAttributeId == model.OrderAttributeId))
        //    {
        //        return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.Orders.OrderAttributes.Attributes.AlreadyExists") });
        //    }

        //    //insert mapping
        //    var orderAttributeMapping = new OrderAttributeMapping
        //    {
        //        OrderId = model.OrderId,
        //        OrderAttributeId = model.OrderAttributeId,
        //        //TextPrompt = model.TextPrompt,
        //        //IsRequired = model.IsRequired,
        //        //AttributeControlTypeId = model.AttributeControlTypeId,
        //        //DisplayOrder = model.DisplayOrder
        //    };
        //    _orderAttributeService.InsertOrderAttributeMapping(orderAttributeMapping);

        //    //predefined values
        //    var predefinedValues = _orderAttributeService.GetPredefinedOrderAttributeValues(model.OrderAttributeId);
        //    foreach (var predefinedValue in predefinedValues)
        //    {
        //        var pav = new OrderAttributeValue
        //        {
        //            OrderAttributeMappingId = orderAttributeMapping.Id,
        //            //AttributeValueType = AttributeValueType.Simple,
        //            Name = predefinedValue.Name,
        //            //PriceAdjustment = predefinedValue.PriceAdjustment,
        //            //WeightAdjustment = predefinedValue.WeightAdjustment,
        //            //Cost = predefinedValue.Cost,
        //            //IsPreSelected = predefinedValue.IsPreSelected,
        //            //DisplayOrder = predefinedValue.DisplayOrder
        //        };
        //        _orderAttributeService.InsertOrderAttributeValue(pav);
        //        //locales
        //        var languages = _languageService.GetAllLanguages(true);
        //        //localization
        //        foreach (var lang in languages)
        //        {
        //            var name = predefinedValue.GetLocalized(x => x.Name, lang.Id, false, false);
        //            if (!string.IsNullOrEmpty(name))
        //                _localizedEntityService.SaveLocalizedValue(pav, x => x.Name, name, lang.Id);
        //        }
        //    }

        //    return new NullJsonResult();
        //}

        //[HttpPost]
        //public virtual ActionResult OrderAttributeMappingUpdate(OrderModel.OrderAttributeMappingModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
        //        return AccessDeniedView();

        //    var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(model.Id);
        //    if (orderAttributeMapping == null)
        //        throw new ArgumentException("No order attribute mapping found with the specified id");

        //    var order = _orderService.GetOrderById(orderAttributeMapping.OrderId);
        //    if (order == null)
        //        throw new ArgumentException("No order found with the specified id");

        //    //a vendor should have access only to his orders
        //    //if (_workContext.CurrentVendor != null && order.VendorId != _workContext.CurrentVendor.Id)
        //    //    return Content("This is not your order");

        //    orderAttributeMapping.OrderAttributeId = model.OrderAttributeId;
        //    //orderAttributeMapping.TextPrompt = model.TextPrompt;
        //    //orderAttributeMapping.IsRequired = model.IsRequired;
        //    //orderAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
        //    //orderAttributeMapping.DisplayOrder = model.DisplayOrder;
        //    _orderAttributeService.UpdateOrderAttributeMapping(orderAttributeMapping);

        //    return new NullJsonResult();
        //}

        //[HttpPost]
        //public virtual ActionResult OrderAttributeMappingDelete(int id)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
        //        return AccessDeniedView();

        //    var orderAttributeMapping = _orderAttributeService.GetOrderAttributeMappingById(id);
        //    if (orderAttributeMapping == null)
        //        throw new ArgumentException("No order attribute mapping found with the specified id");

        //    var orderId = orderAttributeMapping.OrderId;
        //    var order = _orderService.GetOrderById(orderId);
        //    if (order == null)
        //        throw new ArgumentException("No order found with the specified id");


        //    //a vendor should have access only to his orders
        //    //if (_workContext.CurrentVendor != null && order.VendorId != _workContext.CurrentVendor.Id)
        //    //    return Content("This is not your order");

        //    _orderAttributeService.DeleteOrderAttributeMapping(orderAttributeMapping);

        //    return new NullJsonResult();
        //}

    }
}