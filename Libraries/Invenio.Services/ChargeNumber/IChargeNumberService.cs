using System.Collections.Generic;

namespace Invenio.Services.ChargeNumber
{
    public interface IChargeNumberService
    {
        Core.Domain.ChargeNumbers.ChargeNumber GetChargeNumberById(int chargeNumberId);
        void InsertChargeNumber(Core.Domain.ChargeNumbers.ChargeNumber chargeNumber);
        void UpdateChargeNumber(Core.Domain.ChargeNumbers.ChargeNumber chargeNumber);
        void DeleteChargeNumber(Core.Domain.ChargeNumbers.ChargeNumber chargeNumber);
        ICollection<Core.Domain.ChargeNumbers.ChargeNumber> GetAllDeliveryChargeNumbers(int delNumberId);
    }
}
