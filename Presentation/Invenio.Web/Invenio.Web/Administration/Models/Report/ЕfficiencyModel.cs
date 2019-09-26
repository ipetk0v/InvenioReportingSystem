using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class ЕfficiencyModel : BaseNopEntityModel
    {
        public string UserName { get; set; }

        public int MonthlyHours { get; set; }

        public int HoursSold { get; set; }

        public int Difference { get; set; }

        public double Efficiency { get; set; }
    }
}