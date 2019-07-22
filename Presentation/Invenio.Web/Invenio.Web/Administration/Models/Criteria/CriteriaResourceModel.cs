using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Criteria
{
    public class CriteriaResourceModel : BaseNopModel
    {
        public int OrderId { get; internal set; }
        public int Id { get; internal set; }
        public string Description { get; internal set; }
    }
}