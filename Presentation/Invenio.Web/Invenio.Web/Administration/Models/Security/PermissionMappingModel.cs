using System.Collections.Generic;
using Invenio.Admin.Models.Users;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Security
{
    public partial class PermissionMappingModel : BaseNopModel
    {
        public PermissionMappingModel()
        {
            AvailablePermissions = new List<PermissionRecordModel>();
            AvailableUserRoles = new List<UserRoleModel>();
            Allowed = new Dictionary<string, IDictionary<int, bool>>();
        }
        public IList<PermissionRecordModel> AvailablePermissions { get; set; }
        public IList<UserRoleModel> AvailableUserRoles { get; set; }

        //[permission system name] / [User role id] / [allowed]
        public IDictionary<string, IDictionary<int, bool>> Allowed { get; set; }
    }
}