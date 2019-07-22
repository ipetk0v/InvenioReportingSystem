using FluentValidation.Attributes;
using Invenio.Admin.Models.Common;
using Invenio.Admin.Validators.Customers;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Customers
{
    [Validator(typeof(CustomerValidator))]
    public class CustomerModel : BaseNopEntityModel
    {
        public CustomerModel()
        {
            AvailableManufacturer = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Customer.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customer.Fields.Description")]
        [AllowHtml]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customer.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customer.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Customer.Fields.Manufacturer")]
        public int ManufacturerId { get; set; }

        public IList<SelectListItem> AvailableManufacturer { get; set; }
        
        public string ManufacturerName { get; set; }

        public AddressModel Address { get; set; }
    }
}