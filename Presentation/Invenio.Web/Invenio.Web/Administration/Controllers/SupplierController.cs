using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Common;
using Invenio.Admin.Models.Supplier;
using Invenio.Core;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Suppliers;
using Invenio.Services.Common;
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
using Invenio.Core.Domain.Users;
using Invenio.Web.Framework;

namespace Invenio.Admin.Controllers
{
    public class SupplierController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly ISupplierService _supplierService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserActivityService _userActivityService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly ICustomerService _customerService;
        private readonly AddressSettings _addressSettings;
        private readonly IWorkContext _workContext;

        public SupplierController(
            IPermissionService permissionService,
            ISupplierService supplierService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IUserActivityService userActivityService,
            IStateProvinceService stateProvinceService,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser,
            ICustomerService customerService,
            IWorkContext workContext,
            AddressSettings addressSettings
            )
        {
            _permissionService = permissionService;
            _supplierService = supplierService;
            _countryService = countryService;
            _localizationService = localizationService;
            _userActivityService = userActivityService;
            _stateProvinceService = stateProvinceService;
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _customerService = customerService;
            _addressSettings = addressSettings;
            _workContext = workContext;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
                return AccessDeniedView();

            var model = new SupplierListModel();

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
            //Customers
            model.AvailableCustomers.Add(new SelectListItem { Text = "*", Value = "0" });

            if (_workContext.CurrentUser.IsAdmin())
            {
                foreach (var m in _customerService.GetAllCustomers())
                    model.AvailableCustomers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

                return View(model);
            }

            if (_workContext.CurrentUser.CustomerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.CustomerRegions)
                {
                    foreach (var m in _customerService.GetAllCustomers(null, region.CountryId, region.Id))
                        model.AvailableCustomers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                }

            }

            if (_workContext.CurrentUser.Customers.Any())
            {
                foreach (var m in _workContext.CurrentUser.Customers)
                    model.AvailableCustomers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, SupplierListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
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
                var suppliers2 = _supplierService
                    .GetAllSuppliers(model.SearchSupplierName, model.CountryId, model.StateProvinceId, model.CustomerId, command.Page - 1, command.PageSize, overridePublished);

                var gridModel2 = new DataSourceResult
                {
                    Data = suppliers2.Select(PrepareSupplierModel),
                    Total = suppliers2.TotalCount
                };

                return Json(gridModel2);
            }

            var suppliers = _supplierService
                .GetAllSuppliers(model.SearchSupplierName, model.CountryId, model.StateProvinceId, model.CustomerId, showHidden: overridePublished);

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
            var totalCount = filtredSuppliers.Count;

            var gridModel = new DataSourceResult
            {
                Data = filtredSuppliers.Select(PrepareSupplierModel).PagedForCommand(command),
                Total = totalCount
            };

            return Json(gridModel);
        }

        private SupplierModel PrepareSupplierModel(Supplier supplier)
        {
            var model = supplier.ToModel();

            if (supplier.CustomerId.HasValue)
                model.CustomerName = _customerService.GetCustomerById(supplier.CustomerId.Value).Name;

            return model;
        }

        private void PrepareSupplierModel(SupplierModel model)
        {
            model.AvailableCustomer.Add(new SelectListItem { Text = "None", Value = "0" });

            if (_workContext.CurrentUser.IsAdmin())
            {
                foreach (var m in _customerService.GetAllCustomers())
                    model.AvailableCustomer.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }
            else
            {
                if (_workContext.CurrentUser.CustomerRegions.Any())
                {
                    foreach (var region in _workContext.CurrentUser.CustomerRegions)
                    {
                        foreach (var m in _customerService.GetAllCustomers(null, region.CountryId, region.Id))
                            model.AvailableCustomer.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                    }

                }

                if (_workContext.CurrentUser.Customers.Any())
                {
                    foreach (var m in _workContext.CurrentUser.Customers)
                        model.AvailableCustomer.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
                }
            }
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
                return AccessDeniedView();

            var model = new SupplierModel
            {
                Published = true
            };

            PrepareSupplierModel(model);
            PrepareAddressModel(model, null, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(SupplierModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var supplier = model.ToEntity();
                supplier.CreatedOnUtc = DateTime.UtcNow;
                supplier.UpdatedOnUtc = DateTime.UtcNow;

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
                supplier.Address = address;
                _supplierService.InsertSupplier(supplier);

                //activity log
                _userActivityService.InsertActivity("AddNewSupplier", _localizationService.GetResource("ActivityLog.AddNewSupplier"), supplier.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Suppliers.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = supplier.Id });
                }
                return RedirectToAction("List");
            }

            PrepareSupplierModel(model);
            PrepareAddressModel(model, null, true);
            return View(model);
        }

        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
                return AccessDeniedView();

            var supplier = _supplierService.GetSupplierById(id);
            if (supplier == null || supplier.Deleted)
                //No supplier found with the specified id
                return RedirectToAction("List");

            var model = supplier.ToModel();

            PrepareSupplierModel(model);
            PrepareAddressModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(SupplierModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
                return AccessDeniedView();

            var supplier = _supplierService.GetSupplierById(model.Id);
            if (supplier == null || supplier.Deleted)
                //No supplier found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //int prevPictureId = supplier.PictureId;
                supplier = model.ToEntity(supplier);

                if (supplier.Address.CountryId == 0)
                    supplier.Address.CountryId = null;
                if (supplier.Address.StateProvinceId == 0)
                    supplier.Address.StateProvinceId = null;

                supplier.UpdatedOnUtc = DateTime.UtcNow;
                _supplierService.UpdateSupplier(supplier);

                //delete an old picture (if deleted or updated)
                //if (prevPictureId > 0 && prevPictureId != supplier.PictureId)
                //{
                //    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                //    if (prevPicture != null)
                //        _pictureService.DeletePicture(prevPicture);
                //}

                //activity log
                _userActivityService.InsertActivity("EditSupplier", _localizationService.GetResource("ActivityLog.EditSupplier"), supplier.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Supplier.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = supplier.Id });
                }
                return RedirectToAction("List");
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSuppliers))
                return AccessDeniedView();

            var supplier = _supplierService.GetSupplierById(id);
            if (supplier == null)
                //No supplier found with the specified id
                return RedirectToAction("List");

            _supplierService.DeleteSupplier(supplier);

            //activity log
            _userActivityService.InsertActivity("DeleteSupplier", _localizationService.GetResource("ActivityLog.DeleteSupplier"), supplier.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Supplier.Deleted"));
            return RedirectToAction("List");
        }

        [NonAction]
        protected virtual void PrepareAddressModel(SupplierModel model, Address address, /*Supplier supplier,*/ bool excludeProperties)
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
            //supplier attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);
        }
    }
}