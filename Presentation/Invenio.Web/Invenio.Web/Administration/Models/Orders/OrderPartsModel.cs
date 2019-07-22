using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Orders
{
    public class OrderPartsModel : BaseNopEntityModel
    {
        public int OrderId { get; set; }
        public string SerNumber { get; set; }
    }
}