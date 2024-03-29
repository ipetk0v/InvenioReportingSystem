using Invenio.Core.Domain.Users;

namespace Invenio.Data.Mapping.Users
{
    public partial class UserMap : NopEntityTypeConfiguration<User>
    {
        public UserMap()
        {
            this.ToTable("User");
            this.HasKey(c => c.Id);
            this.Property(u => u.Username).HasMaxLength(1000);
            this.Property(u => u.Email).HasMaxLength(1000);
            this.Property(u => u.EmailToRevalidate).HasMaxLength(1000);
            this.Property(u => u.SystemName).HasMaxLength(400);

            this.HasMany(c => c.UserRoles)
                .WithMany()
                .Map(m => m.ToTable("User_UserRole_Mapping"));

            this.HasMany(c => c.Customers)
                .WithMany()
                .Map(m => m.ToTable("User_Customer_Mapping"));

            this.HasMany(c => c.CustomerRegions)
                .WithMany()
                .Map(m => m.ToTable("User_Region_Mapping"));

            this.HasMany(c => c.Addresses)
                .WithMany()
                .Map(m => m.ToTable("UserAddresses"));

            this.HasMany(uwmh => uwmh.UserMonthlyWorkingHours)
                .WithMany()
                .Map(uwmh => uwmh.ToTable("User_UserMonthlyWorkingHours_Mapping"));

            this.HasOptional(c => c.BillingAddress);
            this.HasOptional(c => c.ShippingAddress);
        }
    }
}