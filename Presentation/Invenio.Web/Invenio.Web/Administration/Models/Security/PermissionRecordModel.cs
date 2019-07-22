using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Security
{
    public partial class PermissionRecordModel : BaseNopModel
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}