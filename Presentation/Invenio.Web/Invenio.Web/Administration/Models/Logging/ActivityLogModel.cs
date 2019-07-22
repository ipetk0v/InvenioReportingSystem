﻿using System;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Logging
{
    public partial class ActivityLogModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.User")]
        public int UserId { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.User")]
        public string UserEmail { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Comment")]
        public string Comment { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.ActivityLog.IpAddress")]
        public string IpAddress { get; set; }
    }
}
