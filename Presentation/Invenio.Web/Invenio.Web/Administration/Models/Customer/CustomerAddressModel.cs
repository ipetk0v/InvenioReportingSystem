using Invenio.Admin.Models.Common;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Customer
{
    public class CustomerAddressModel : BaseNopModel
    {
        public int CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}