using Invenio.Core.Domain.DeliveryNumbers;
using Invenio.Core.Domain.Reports;

namespace Invenio.Core.Domain.ChargeNumbers
{
    public class ChargeNumber : BaseEntity
    {
        public string Number { get; set; }

        public int DeliveryNumberId { get; set; }
        public DeliveryNumber DeliveryNumber { get; set; }
    }
}
