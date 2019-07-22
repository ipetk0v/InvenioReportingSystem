using System;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public partial class OnlineUserModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Users.OnlineUsers.Fields.UserInfo")]
        public string UserInfo { get; set; }

        [NopResourceDisplayName("Admin.Users.OnlineUsers.Fields.IPAddress")]
        public string LastIpAddress { get; set; }

        [NopResourceDisplayName("Admin.Users.OnlineUsers.Fields.Location")]
        public string Location { get; set; }

        [NopResourceDisplayName("Admin.Users.OnlineUsers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }
        
        [NopResourceDisplayName("Admin.Users.OnlineUsers.Fields.LastVisitedPage")]
        public string LastVisitedPage { get; set; }
    }
}