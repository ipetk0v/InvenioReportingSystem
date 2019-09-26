using FluentValidation.Attributes;
using Invenio.Admin.Validators.Customer;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Customer
{
    [Validator(typeof(CustomerValidator))]
    public partial class CustomerModel : BaseNopEntityModel
    {
        public CustomerModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Customers.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customers.Fields.Description")]
        [AllowHtml]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customers.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customers.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        [NopResourceDisplayName("Admin.Catalog.Customers.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }
        public string StateProvinceName { get; set; }
        
        [NopResourceDisplayName("Admin.Catalog.Customers.Fields.Country")]
        public int? CountryId { get; set; }
        public string CountryName { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
    }
}