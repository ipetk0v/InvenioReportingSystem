using Invenio.Web.Framework.Mvc;

namespace Invenio.Web.Models.User
{
    public partial class EmailRevalidationModel : BaseNopModel
    {
        public string Result { get; set; }
    }
}