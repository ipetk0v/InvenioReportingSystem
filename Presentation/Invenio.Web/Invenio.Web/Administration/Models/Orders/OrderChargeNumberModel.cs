using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Orders
{
    public class OrderChargeNumberModel : BaseNopEntityModel
    {
        public string DeliverNumber { get; set; }
        public int DeliveryNumberId { get; set; }
        public string ChargeNumber { get; set; }
        public int ChargeNumberQuantity { get; set; }
    }
}