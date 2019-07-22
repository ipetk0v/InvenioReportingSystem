﻿using Invenio.Web.Framework.Mvc;

namespace Invenio.Web.Models.Common
{
    public partial class AdminHeaderLinksModel : BaseNopModel
    {
        public string ImpersonatedUserName { get; set; }
        public bool IsUserImpersonated { get; set; }
        public bool DisplayAdminLink { get; set; }
        public string EditPageUrl { get; set; }
    }
}