using Invenio.Core.Domain.Parts;
using System.Collections.Generic;

namespace Invenio.Services.Parts
{
    public interface IPartService
    {
        Part GetPartById(int partId);
        void InsertPart(Part part);
        void UpdatePart(Part part);
        void DeletePart(Part part);
        ICollection<Part> GetAllOrderParts(int orderId);
        //Part GetPartByReportId(int reportId);
    }
}