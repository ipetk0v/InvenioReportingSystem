using Invenio.Web.Framework.Mvc;

namespace Invenio.Web.Models.User
{
    public partial class UserAvatarModel : BaseNopModel
    {
        public string AvatarUrl { get; set; }
    }
}