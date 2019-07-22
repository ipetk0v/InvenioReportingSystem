using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Admin.Extensions;
using Invenio.Admin.Helpers;
using Invenio.Admin.Models.Users;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Users;
using Invenio.Services;
using Invenio.Services.Catalog;
using Invenio.Services.Users;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Security;
using Invenio.Services.Stores;
//using Invenio.Services.Vendors;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;

namespace Invenio.Admin.Controllers
{
    public partial class UserRoleController : BaseAdminController
	{
		#region Fields

		private readonly IUserService _UserService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserActivityService _UserActivityService;
        private readonly IPermissionService _permissionService;
        //private readonly IProductService _productService;
        //private readonly ICategoryService _categoryService;
        //private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        //private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;

		#endregion

		#region Constructors

        public UserRoleController(IUserService UserService,
            ILocalizationService localizationService, 
            IUserActivityService UserActivityService,
            IPermissionService permissionService,
            //IProductService productService,
            //ICategoryService categoryService,
            //IManufacturerService manufacturerService,
            IStoreService storeService,
            //IVendorService vendorService,
            IWorkContext workContext, 
            ICacheManager cacheManager)
		{
            this._UserService = UserService;
            this._localizationService = localizationService;
            this._UserActivityService = UserActivityService;
            this._permissionService = permissionService;
            //this._productService = productService;
            //this._categoryService = categoryService;
            //this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            //this._vendorService = vendorService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
		}

		#endregion 

        #region Utilities

        [NonAction]
        protected virtual UserRoleModel PrepareUserRoleModel(UserRole UserRole)
        {
            var model = UserRole.ToModel();
            //var product = _productService.GetProductById(UserRole.PurchasedWithProductId);
            //if (product != null)
            //{
            //    model.PurchasedWithProductName = product.Name;
            //}
            return model;
        }

        #endregion

        #region User roles

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();
            
			return View();
		}

		[HttpPost]
		public virtual ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();
            
            var UserRoles = _UserService.GetAllUserRoles(true);
            var gridModel = new DataSourceResult
			{
                Data = UserRoles.Select(PrepareUserRoleModel),
                Total = UserRoles.Count()
			};

            return Json(gridModel);
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();
            
            var model = new UserRoleModel();
            //default values
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(UserRoleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();
            
            if (ModelState.IsValid)
            {
                var UserRole = model.ToEntity();
                _UserService.InsertUserRole(UserRole);

                //activity log
                _UserActivityService.InsertActivity("AddNewUserRole", _localizationService.GetResource("ActivityLog.AddNewUserRole"), UserRole.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Users.UserRoles.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = UserRole.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

		public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();
            
            var UserRole = _UserService.GetUserRoleById(id);
            if (UserRole == null)
                //No User role found with the specified id
                return RedirectToAction("List");
		    
            var model = PrepareUserRoleModel(UserRole);
            return View(model);
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(UserRoleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();
            
            var UserRole = _UserService.GetUserRoleById(model.Id);
            if (UserRole == null)
                //No User role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    if (UserRole.IsSystemRole && !model.Active)
                        throw new InvenioException(_localizationService.GetResource("Admin.Users.UserRoles.Fields.Active.CantEditSystem"));

                    if (UserRole.IsSystemRole && !UserRole.SystemName.Equals(model.SystemName, StringComparison.InvariantCultureIgnoreCase))
                        throw new InvenioException(_localizationService.GetResource("Admin.Users.UserRoles.Fields.SystemName.CantEditSystem"));

                    if (SystemUserRoleNames.Registered.Equals(UserRole.SystemName, StringComparison.InvariantCultureIgnoreCase) &&
                        model.PurchasedWithProductId > 0)
                        throw new InvenioException(_localizationService.GetResource("Admin.Users.UserRoles.Fields.PurchasedWithProduct.Registered"));
                    
                    UserRole = model.ToEntity(UserRole);
                    _UserService.UpdateUserRole(UserRole);

                    //activity log
                    _UserActivityService.InsertActivity("EditUserRole", _localizationService.GetResource("ActivityLog.EditUserRole"), UserRole.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Users.UserRoles.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = UserRole.Id}) : RedirectToAction("List");
                }

                //If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = UserRole.Id });
            }
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();
            
            var UserRole = _UserService.GetUserRoleById(id);
            if (UserRole == null)
                //No User role found with the specified id
                return RedirectToAction("List");

            try
            {
                _UserService.DeleteUserRole(UserRole);

                //activity log
                _UserActivityService.InsertActivity("DeleteUserRole", _localizationService.GetResource("ActivityLog.DeleteUserRole"), UserRole.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Users.UserRoles.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = UserRole.Id });
            }

		}



        //public virtual ActionResult AssociateProductToUserRolePopup()
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedView();

        //    var model = new UserRoleModel.AssociateProductToUserRoleModel();
        //    //a vendor should have access only to his products
        //    model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

        //    //categories
        //    model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //    var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
        //    foreach (var c in categories)
        //        model.AvailableCategories.Add(c);

        //    //manufacturers
        //    model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //    var manufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);
        //    foreach (var m in manufacturers)
        //        model.AvailableManufacturers.Add(m);

        //    //stores
        //    model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //    foreach (var s in _storeService.GetAllStores())
        //        model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

        //    //vendors
        //    model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //    var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
        //    foreach (var v in vendors)
        //        model.AvailableVendors.Add(v);

        //    //product types
        //    model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
        //    model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

        //    return View(model);
        //}

        //[HttpPost]
        //public virtual ActionResult AssociateProductToUserRolePopupList(DataSourceRequest command,
        //    UserRoleModel.AssociateProductToUserRoleModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedKendoGridJson();

        //    //a vendor should have access only to his products
        //    if (_workContext.CurrentVendor != null)
        //    {
        //        model.SearchVendorId = _workContext.CurrentVendor.Id;
        //    }

        //    var products = _productService.SearchProducts(
        //        categoryIds: new List<int> { model.SearchCategoryId },
        //        manufacturerId: model.SearchManufacturerId,
        //        storeId: model.SearchStoreId,
        //        vendorId: model.SearchVendorId,
        //        productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
        //        keywords: model.SearchProductName,
        //        pageIndex: command.Page - 1,
        //        pageSize: command.PageSize,
        //        showHidden: true
        //        );
        //    var gridModel = new DataSourceResult();
        //    gridModel.Data = products.Select(x => x.ToModel());
        //    gridModel.Total = products.TotalCount;

        //    return Json(gridModel);
        //}

        //[HttpPost]
        //[FormValueRequired("save")]
        //public virtual ActionResult AssociateProductToUserRolePopup(string btnId, string productIdInput,
        //    string productNameInput, UserRoleModel.AssociateProductToUserRoleModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedView();

        //    var associatedProduct = _productService.GetProductById(model.AssociatedToProductId);
        //    if (associatedProduct == null)
        //        return Content("Cannot load a product");

        //    //a vendor should have access only to his products
        //    if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
        //        return Content("This is not your product");

        //    //a vendor should have access only to his products
        //    model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
        //    ViewBag.RefreshPage = true;
        //    ViewBag.productIdInput = productIdInput;
        //    ViewBag.productNameInput = productNameInput;
        //    ViewBag.btnId = btnId;
        //    ViewBag.productId = associatedProduct.Id;
        //    ViewBag.productName = associatedProduct.Name;
        //    return View(model);
        //}

		#endregion
    }
}
