using System;

namespace Invenio.Core.Domain.Users
{
    public class UserMonthlyWorkingHours : BaseEntity
    {
        public DateTime FirstDateOfMonth { get; set; }
        public DateTime LastDateOfMonth { get; set; }
        public int WorkHours { get; set; }
    }
}
