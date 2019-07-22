using Invenio.Core.Domain.Users;

namespace Invenio.Data.Mapping.Users
{
    public partial class UserAttributeMap : NopEntityTypeConfiguration<UserAttribute>
    {
        public UserAttributeMap()
        {
            this.ToTable("UserAttribute");
            this.HasKey(ca => ca.Id);
            this.Property(ca => ca.Name).IsRequired().HasMaxLength(400);

            this.Ignore(ca => ca.AttributeControlType);
        }
    }
}