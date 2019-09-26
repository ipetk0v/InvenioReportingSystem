using FluentValidation.Attributes;
using Invenio.Admin.Models.Common;
using Invenio.Admin.Validators.Supplier;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Supplier
{
    [Validator(typeof(SupplierValidator))]
    public class SupplierModel : BaseNopEntityModel
    {
        public SupplierModel()
        {
            AvailableCustomer = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Supplier.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Supplier.Fields.Description")]
        [AllowHtml]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Supplier.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Supplier.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Supplier.Fields.Customer")]
        public int CustomerId { get; set; }

        public IList<SelectListItem> AvailableCustomer { get; set; }
        
        public string CustomerName { get; set; }

        public AddressModel Address { get; set; }
    }
}