using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Manufacturer;
using Invenio.Services.Catalog;
using Invenio.Services.Directory;
using Invenio.Services.Security;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using System;
using System.Linq;
using System.Web.Mvc;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Admin.Models.Common;
using Invenio.Core.Domain.Common;
using System.Collections.Generic;
using Invenio.Core.Domain.Directory;
using Invenio.Services.Common;
using Invenio.Core.Domain.Customers;
using Invenio.Services.Customers;
using Invenio.Core.Domain.Manufacturers;

namespace Invenio.Admin.Controllers
{
    public class ManufacturerController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserActivityService _userActivityService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;

        public ManufacturerController(
            IPermissionService permissionService,
            IManufacturerService manufacturerService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IUserActivityService userActivityService,
            IStateProvinceService stateProvinceService,
            ICustomerService customerService)
        {
            _permissionService = permissionService;
            _manufacturerService = manufacturerService;
            _countryService = countryService;
            _localizationService = localizationService;
            _userActivityService = userActivityService;
            _stateProvinceService = stateProvinceService;
            _customerService = customerService;
        }

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var model = new ManufacturerListModel();

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
        public virtual ActionResult List(DataSourceRequest command, ManufacturerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedKendoGridJson();

            var manufacturer = _manufacturerService
                .GetAllManufacturers(
                    model.SearchManufacturerName, model.CountryId, model.StateProvinceId, command.Page - 1, command.PageSize, showHidden: true);

            var gridModel = new DataSourceResult
            {
                Data = manufacturer.Select(x => PrepareManufacturerModel(x)),
                Total = manufacturer.Count
            };

            return Json(gridModel);
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var model = new ManufacturerModel
            {
                Published = true
            };

            PrepareCountryAndStateModel(model);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(ManufacturerModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var manufacturer = model.ToEntity();

                if (model.CountryId == 0)
                    manufacturer.CountryId = null;

                if (model.StateProvinceId == 0)
                    manufacturer.StateProvinceId = null;

                manufacturer.CreatedOnUtc = DateTime.UtcNow;
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                _manufacturerService.InsertManufacturer(manufacturer);

                var customer = new Customer
                {
                    Name = manufacturer.Name,
                    Comment = manufacturer.Comment,
                    CreatedOnUtc = DateTime.Now,
                    UpdatedOnUtc = DateTime.Now,
                    DisplayOrder = 0,
                    Published = true
                };
                customer.Address = new Address
                {
                    CountryId = manufacturer.CountryId,
                    StateProvinceId = manufacturer.StateProvinceId,
                    CreatedOnUtc = DateTime.Now
                };
                customer.ManufacturerId = manufacturer.Id;
                _customerService.InsertCustomer(customer);

                //ACL (customer roles)
                //SaveManufacturerAcl(manufacturer, model);

                //activity log
                _userActivityService.InsertActivity("AddNewManufacturer", _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = manufacturer.Id });
                }
                return RedirectToAction("List");
            }

            PrepareCountryAndStateModel(model);
            return View(model);
        }

        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null || manufacturer.Deleted)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            var model = manufacturer.ToModel();
            PrepareCountryAndStateModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer == null || manufacturer.Deleted)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //int prevPictureId = manufacturer.PictureId;
                manufacturer = model.ToEntity(manufacturer);

                if (manufacturer.CountryId == 0)
                    manufacturer.CountryId = null;
                if (manufacturer.StateProvinceId == 0)
                    manufacturer.StateProvinceId = null;

                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                _manufacturerService.UpdateManufacturer(manufacturer);

                //delete an old picture (if deleted or updated)
                //if (prevPictureId > 0 && prevPictureId != manufacturer.PictureId)
                //{
                //    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                //    if (prevPicture != null)
                //        _pictureService.DeletePicture(prevPicture);
                //}

                //activity log
                _userActivityService.InsertActivity("EditManufacturer", _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = manufacturer.Id });
                }
                return RedirectToAction("List");
            }

            PrepareCountryAndStateModel(model);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                //No manufacturer found with the specified id
                return RedirectToAction("List");

            _manufacturerService.DeleteManufacturer(manufacturer);

            //activity log
            _userActivityService.InsertActivity("DeleteManufacturer", _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Deleted"));
            return RedirectToAction("List");
        }

        [NonAction]
        protected ManufacturerModel PrepareManufacturerModel(Manufacturer manufacturer)
        {
            var model = manufacturer.ToModel();

            if (model.CountryId.HasValue)
                model.CountryName = _countryService.GetCountryById(model.CountryId.Value).Name;

            if (model.StateProvinceId.HasValue)
                model.StateProvinceName = _stateProvinceService.GetStateProvinceById(model.StateProvinceId.Value).Name;

            return model;
        }

        [NonAction]
        protected virtual void PrepareCountryAndStateModel(ManufacturerModel model)
        {
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: false))
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