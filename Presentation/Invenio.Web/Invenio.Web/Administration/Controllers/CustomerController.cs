using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Customer;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Customers;
using Invenio.Core.Domain.Suppliers;
using Invenio.Services.Customers;
using Invenio.Services.Directory;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Security;
using Invenio.Services.Supplier;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Users;

namespace Invenio.Admin.Controllers
{
    public class CustomerController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserActivityService _userActivityService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ISupplierService _supplierService;
        private readonly IWorkContext _workContext;

        public CustomerController(
            IPermissionService permissionService,
            ICustomerService customerService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IUserActivityService userActivityService,
            IStateProvinceService stateProvinceService,
            ISupplierService supplierService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _customerService = customerService;
            _countryService = countryService;
            _localizationService = localizationService;
            _userActivityService = userActivityService;
            _stateProvinceService = stateProvinceService;
            _supplierService = supplierService;
            _workContext = workContext;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerListModel();

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublished.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var c in _stateProvinceService.GetStateProvinces())
                model.AvailableStates.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, CustomerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

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
                var customers2 = _customerService
                    .GetAllCustomers(
                        model.SearchCustomerName, model.CountryId, model.StateProvinceId, command.Page - 1, command.PageSize, showHidden: overridePublished);

                var gridModel2 = new DataSourceResult
                {
                    Data = customers2.Select(PrepareCustomerModel),
                    Total = customers2.TotalCount
                };

                return Json(gridModel2);
            }

            var customers = _customerService
                .GetAllCustomers(
                    model.SearchCustomerName, model.CountryId, model.StateProvinceId, showHidden: overridePublished);

            var crCustomer = new List<Customer>();
            var rCustomers = new List<Customer>();
            if (_workContext.CurrentUser.CustomerRegions.Any())
                crCustomer = customers.Where(x => _workContext.CurrentUser.CustomerRegions.Contains(x.StateProvince)).ToList();

            if (_workContext.CurrentUser.Customers.Any())
                rCustomers = customers.Where(x => _workContext.CurrentUser.Customers.Contains(x)).ToList();

            var result = crCustomer.Concat(rCustomers).Distinct().ToList();

            var pagedList = new PagedList<Customer>(result, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(PrepareCustomerModel),
                Total = pagedList.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerModel
            {
                Published = true
            };

            PrepareCountryAndStateModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(CustomerModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customer = model.ToEntity();

                if (model.CountryId == 0)
                    customer.CountryId = null;

                if (model.StateProvinceId == 0)
                    customer.StateProvinceId = null;

                customer.CreatedOnUtc = DateTime.UtcNow;
                customer.UpdatedOnUtc = DateTime.UtcNow;
                _customerService.InsertCustomer(customer);

                var supplier = new Supplier
                {
                    Name = customer.Name,
                    Comment = customer.Comment,
                    CreatedOnUtc = DateTime.Now,
                    UpdatedOnUtc = DateTime.Now,
                    DisplayOrder = 0,
                    Published = true,
                    Address = new Address
                    {
                        CountryId = customer.CountryId,
                        StateProvinceId = customer.StateProvinceId,
                        CreatedOnUtc = DateTime.Now
                    },
                    CustomerId = customer.Id
                };
                _supplierService.InsertSupplier(supplier);

                //ACL (supplier roles)
                //SaveCustomerAcl(customer, model);

                //activity log
                _userActivityService.InsertActivity("AddNewCustomer", _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Customers.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                return RedirectToAction("List");
            }

            PrepareCountryAndStateModel(model);
            return View(model);
        }

        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = customer.ToModel();
            PrepareCountryAndStateModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(CustomerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(model.Id);
            if (customer == null || customer.Deleted)
                //No customer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //int prevPictureId = customer.PictureId;
                customer = model.ToEntity(customer);

                if (customer.CountryId == 0)
                    customer.CountryId = null;
                if (customer.StateProvinceId == 0)
                    customer.StateProvinceId = null;

                customer.UpdatedOnUtc = DateTime.UtcNow;
                _customerService.UpdateCustomer(customer);

                //delete an old picture (if deleted or updated)
                //if (prevPictureId > 0 && prevPictureId != customer.PictureId)
                //{
                //    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                //    if (prevPicture != null)
                //        _pictureService.DeletePicture(prevPicture);
                //}

                //activity log
                _userActivityService.InsertActivity("EditCustomer", _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Customers.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                return RedirectToAction("List");
            }

            PrepareCountryAndStateModel(model);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            _customerService.DeleteCustomer(customer);

            //activity log
            _userActivityService.InsertActivity("DeleteCustomer", _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Customers.Deleted"));
            return RedirectToAction("List");
        }

        [NonAction]
        protected CustomerModel PrepareCustomerModel(Customer customer)
        {
            var model = customer.ToModel();

            if (model.CountryId.HasValue)
                model.CountryName = _countryService.GetCountryById(model.CountryId.Value).Name;

            if (model.StateProvinceId.HasValue)
                model.StateProvinceName = _stateProvinceService.GetStateProvinceById(model.StateProvinceId.Value).Name;

            return model;
        }

        [NonAction]
        protected virtual void PrepareCountryAndStateModel(CustomerModel model)
        {
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CountryId) });
            //states
            //var states = model.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.CountryId.Value, showHidden: false).ToList() : new List<StateProvince>();
            var states = _stateProvinceService.GetStateProvinces().ToList();
            if (states.Any())
            {
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
            }
            else
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
        }
    }
}