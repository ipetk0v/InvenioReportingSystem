using Invenio.Admin.Models.Common;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Manufacturer
{
    public class ManufacturerAddressModel : BaseNopModel
    {
        public int ManufacturerId { get; set; }

        public AddressModel Address { get; set; }
    }
}