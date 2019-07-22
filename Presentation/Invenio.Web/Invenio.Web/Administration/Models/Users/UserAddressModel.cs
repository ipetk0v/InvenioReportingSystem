using Invenio.Admin.Models.Common;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public partial class UserAddressModel : BaseNopModel
    {
        public int UserId { get; set; }

        public AddressModel Address { get; set; }
    }
}