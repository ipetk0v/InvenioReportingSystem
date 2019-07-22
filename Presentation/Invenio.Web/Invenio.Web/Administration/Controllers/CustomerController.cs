using Invenio.Admin.Models.Customers;
using Invenio.Services.Security;
using Invenio.Web.Framework.Kendoui;
using System.Web.Mvc;
using Invenio.Services.Customers;
using System.Linq;
using Invenio.Admin.Extensions;
using Invenio.Web.Framework.Controllers;
using System;
using Invenio.Core.Domain.Common;
using Invenio.Admin.Models.Common;
using Invenio.Services.Directory;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Common;
using Invenio.Core.Domain.Directory;
using System.Collections.Generic;
using Invenio.Core;
using Invenio.Services.Catalog;
using Invenio.Core.Domain.Customers;

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
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IManufacturerService _manufacturerService;
        private readonly AddressSettings _addressSettings;
        private readonly IWorkContext _workContext;

        public CustomerController(
            IPermissionService permissionService,
            ICustomerService customerService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IUserActivityService userActivityService,
            IStateProvinceService stateProvinceService,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IManufacturerService manufacturerService,
            IWorkContext workContext,
            AddressSettings addressSettings
            )
        {
            _permissionService = permissionService;
            _customerService = customerService;
            _countryService = countryService;
            _localizationService = localizationService;
            _userActivityService = userActivityService;
            _stateProvinceService = stateProvinceService;
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _manufacturerService = manufacturerService;
            _addressSettings = addressSettings;
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

            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var c in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var c in _stateProvinceService.GetStateProvinces())
                model.AvailableStates.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = "*", Value = "0" });
            if (_workContext.CurrentUser.ManufacturerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.ManufacturerRegions)
                {
                    foreach (var m in _manufacturerService.GetAllManufacturers(null, region.CountryId, region.Id))
                        model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                }

            }

            if (_workContext.CurrentUser.Manufacturers.Any())
            {
                foreach (var m in _workContext.CurrentUser.Manufacturers)
                    model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, CustomerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedKendoGridJson();

            var customers = _customerService
                .GetAllCustomers(model.SearchCustomerName, model.CountryId, model.StateProvinceId, model.ManufacturerId, command.Page - 1, command.PageSize);

            var filtredCustomers = new List<Customer>();
            if (_workContext.CurrentUser.ManufacturerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.ManufacturerRegions)
                {
                    var manufacturers =
                        _manufacturerService.GetAllManufacturers(countryId: region.CountryId, stateId: region.Id);

                    foreach (var manufacturer in manufacturers)
                    {
                        filtredCustomers.AddRange(customers.Where(x => x.Manufacturer == manufacturer));
                    }
                }
            }

            if (_workContext.CurrentUser.Manufacturers.Any())
            {
                foreach (var manufacturer in _workContext.CurrentUser.Manufacturers)
                {
                    filtredCustomers.AddRange(customers.Where(x => x.Manufacturer == manufacturer));
                }
            }

            filtredCustomers = filtredCustomers.Distinct().ToList();

            var gridModel = new DataSourceResult
            {
                Data = filtredCustomers.Select(PrepareCustomerModel),
                Total = filtredCustomers.Count
            };

            return Json(gridModel);
        }

        private CustomerModel PrepareCustomerModel(Customer customer)
        {
            var model = customer.ToModel();

            if (customer.ManufacturerId.HasValue)
                model.ManufacturerName = _manufacturerService.GetManufacturerById(customer.ManufacturerId.Value).Name;

            return model;
        }

        private void PrepareCustomerModel(CustomerModel model)
        {
            model.AvailableManufacturer.Add(new SelectListItem { Text = "None", Value = "0" });

            if (_workContext.CurrentUser.ManufacturerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.ManufacturerRegions)
                {
                    foreach (var m in _manufacturerService.GetAllManufacturers(null, region.CountryId, region.Id))
                        model.AvailableManufacturer.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                }

            }

            if (_workContext.CurrentUser.Manufacturers.Any())
            {
                foreach (var m in _workContext.CurrentUser.Manufacturers)
                    model.AvailableManufacturer.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerModel
            {
                Published = true
            };

            PrepareCustomerModel(model);
            PrepareAddressModel(model, null, false);

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
                customer.CreatedOnUtc = DateTime.UtcNow;
                customer.UpdatedOnUtc = DateTime.UtcNow;

                //custom address attributes
                var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
                var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
                foreach (var error in customAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }

                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                customer.Address = address;
                _customerService.InsertCustomer(customer);

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

            PrepareCustomerModel(model);
            PrepareAddressModel(model, null, true);
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

            PrepareCustomerModel(model);
            PrepareAddressModel(model, null, false);
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

                if (customer.Address.CountryId == 0)
                    customer.Address.CountryId = null;
                if (customer.Address.StateProvinceId == 0)
                    customer.Address.StateProvinceId = null;

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

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Customer.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                return RedirectToAction("List");
            }

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

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Customer.Deleted"));
            return RedirectToAction("List");
        }

        [NonAction]
        protected virtual void PrepareAddressModel(CustomerModel model, Address address, /*Customer customer,*/ bool excludeProperties)
        {
            if (model.Address == null)
                model.Address = new AddressModel();

            model.Address.FirstNameEnabled = true;
            //model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            //model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            //model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            //model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            //model.Address.CountryRequired = _addressSettings.CountryEnabled; //country is required when enabled
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            //model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            //model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            //model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            //model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            //model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            //model.Address.FaxRequired = _addressSettings.FaxRequired;

            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            //customer attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);
        }
    }
}