using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Supplier
{
    public class SupplierListModel : BaseNopModel
    {
        public SupplierListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableCustomers = new List<SelectListItem>();
            AvailablePublished = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Suppliers.List.SearchSupplierName")]
        [AllowHtml]
        public string SearchSupplierName { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        [NopResourceDisplayName("Admin.Orders.Shipments.List.Country")]
        public int CountryId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }
        [NopResourceDisplayName("Admin.Orders.Shipments.List.StateProvince")]
        public int StateProvinceId { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; }
        [NopResourceDisplayName("Admin.Orders.Shipments.List.Customers")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.Published")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublished { get; set; }
    }
}