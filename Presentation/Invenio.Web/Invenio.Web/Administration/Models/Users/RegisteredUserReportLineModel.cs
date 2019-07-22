using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public partial class RegisteredUserReportLineModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Users.Reports.RegisteredUsers.Fields.Period")]
        public string Period { get; set; }

        [NopResourceDisplayName("Admin.Users.Reports.RegisteredUsers.Fields.Users")]
        public int Users { get; set; }
    }
}