using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public class UserMonthlyWorkingHoursModel : BaseNopEntityModel
    {
        public int UserId { get; set; }

        public string Period { get; set; }

        public int WorkHours { get; set; }
    }
}