using System.Collections.Generic;
using Invenio.Web.Framework.Mvc;
using Invenio.Web.Models.Common;

namespace Invenio.Web.Models.User
{
    public partial class UserAddressListModel : BaseNopModel
    {
        public UserAddressListModel()
        {
            Addresses = new List<AddressModel>();
        }

        public IList<AddressModel> Addresses { get; set; }
    }
}