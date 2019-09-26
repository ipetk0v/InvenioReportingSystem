using System.Collections.Generic;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Customer
{
    public partial class CustomerListModel : BaseNopModel
    {
        public CustomerListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailablePublished = new List<SelectListItem>();
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

        [NopResourceDisplayName("Admin.Order.List.Search.Published")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublished { get; set; }
    }
}