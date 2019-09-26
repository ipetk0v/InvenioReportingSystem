using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ListDetailsModel : BaseNopModel
    {
        public string Criteria { get; set; }
        public int CriteriaQuantity { get; set; }

        //public string ReworkCriteria { get; set; }
        //public int ReworkCriteriaQuantity { get; set; }
    }
}