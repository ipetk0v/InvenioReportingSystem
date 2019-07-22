using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Customers
{
    public class CustomerListModel : BaseNopModel
    {
        public CustomerListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Customers.List.SearchCustomerName")]
        [AllowHtml]
        public string SearchCustomerName { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        [NopResourceDisplayName("Admin.Orders.Shipments.List.Country")]
        public int CountryId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }
        [NopResourceDisplayName("Admin.Orders.Shipments.List.StateProvince")]
        public int StateProvinceId { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }
        [NopResourceDisplayName("Admin.Orders.Shipments.List.Manufacturers")]
        public int ManufacturerId { get; set; }
    }
}