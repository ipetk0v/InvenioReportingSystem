using System.Collections.Generic;

namespace Invenio.Services.DeliveryNumber
{
    public interface IDeliveryNumberService
    {
        Core.Domain.DeliveryNumbers.DeliveryNumber GetDeliveryNumberById(int delNumberId);
        void InsertDeliveryNumber(Core.Domain.DeliveryNumbers.DeliveryNumber delNumber);
        void UpdateDeliveryNumber(Core.Domain.DeliveryNumbers.DeliveryNumber delNumber);
        void DeleteDeliveryNumber(Core.Domain.DeliveryNumbers.DeliveryNumber delNumber);
        ICollection<Core.Domain.DeliveryNumbers.DeliveryNumber> GetAllPartDeliveryNumbers(int partId);
    }
}
