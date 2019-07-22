using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Invenio.Admin.Validators.Users;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    [Validator(typeof(UserRoleValidator))]
    public partial class UserRoleModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.FreeShipping")]
        [AllowHtml]
        public bool FreeShipping { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.TaxExempt")]
        public bool TaxExempt { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.SystemName")]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.EnablePasswordLifetime")]
        public bool EnablePasswordLifetime { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.PurchasedWithProduct")]
        public int PurchasedWithProductId { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.PurchasedWithProduct")]
        public string PurchasedWithProductName { get; set; }


        #region Nested classes

        public partial class AssociateProductToUserRoleModel : BaseNopModel
        {
            public AssociateProductToUserRoleModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
            public int SearchStoreId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public int SearchVendorId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }


            public int AssociatedToProductId { get; set; }
        }
        #endregion
    }
}