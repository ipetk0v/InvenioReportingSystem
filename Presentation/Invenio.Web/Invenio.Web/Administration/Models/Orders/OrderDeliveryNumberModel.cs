using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Orders
{
    public class OrderDeliveryNumberModel : BaseNopEntityModel
    {
        public int OrderId { get; set; }
        public string DeliveryNumber { get; set; }
        public string PartNumber { get; set; }
    }
}