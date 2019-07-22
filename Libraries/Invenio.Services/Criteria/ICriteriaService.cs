using System.Collections.Generic;
using Invenio.Core.Domain.Criterias;

namespace Invenio.Services.Criteria
{
    public interface ICriteriaService
    {
        IList<Core.Domain.Criterias.Criteria> GetAllCriteriaValues(int orderId);
        IList<int> GetAllCriteriaValues(int orderId, CriteriaType ct);
        Core.Domain.Criterias.Criteria GetCriteriaById(int id);
        IList<Core.Domain.Criterias.Criteria> GetCriteriaByDescription(string description, int orderId, bool v);
        void UpdateCriteria(Core.Domain.Criterias.Criteria criteria);
        void InsertCriteria(Core.Domain.Criterias.Criteria criteria);
        void DeleteCriteria(Core.Domain.Criterias.Criteria criteria);
    }
}
