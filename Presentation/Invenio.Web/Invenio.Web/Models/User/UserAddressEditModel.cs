using Invenio.Web.Framework.Mvc;
using Invenio.Web.Models.Common;

namespace Invenio.Web.Models.User
{
    public partial class UserAddressEditModel : BaseNopModel
    {
        public UserAddressEditModel()
        {
            this.Address = new AddressModel();
        }
        public AddressModel Address { get; set; }
    }
}