using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Logging
{
    public partial class ActivityLogSearchModel : BaseNopModel
    {
        public ActivityLogSearchModel()
        {
            ActivityLogType = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public int ActivityLogTypeId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public IList<SelectListItem> ActivityLogType { get; set; }
        
        [NopResourceDisplayName("Admin.Users.Users.ActivityLog.IpAddress")]
        public string IpAddress { get; set; }
    }
}