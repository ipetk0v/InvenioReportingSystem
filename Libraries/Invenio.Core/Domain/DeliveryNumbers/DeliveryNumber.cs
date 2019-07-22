using Invenio.Core.Domain.Parts;

namespace Invenio.Core.Domain.DeliveryNumbers
{
    public class DeliveryNumber : BaseEntity
    {
        public string Number { get; set; }

        public int PartId { get; set; }
        public Part Part { get; set; }
    }
}
