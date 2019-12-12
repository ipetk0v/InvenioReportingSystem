using Invenio.Core.Domain.Users;

namespace Invenio.Data.Mapping.Users
{
    public class UserMonthlyWorkingHoursMap : NopEntityTypeConfiguration<UserMonthlyWorkingHours>
    {
        public UserMonthlyWorkingHoursMap()
        { 
            this.ToTable("UserMonthlyWorkingHoursMap");
            this.HasKey(c => c.Id);
        }
    }
}
