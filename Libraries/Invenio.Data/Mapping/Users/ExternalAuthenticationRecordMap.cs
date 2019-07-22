using Invenio.Core.Domain.Users;

namespace Invenio.Data.Mapping.Users
{
    public partial class ExternalAuthenticationRecordMap : NopEntityTypeConfiguration<ExternalAuthenticationRecord>
    {
        public ExternalAuthenticationRecordMap()
        {
            this.ToTable("ExternalAuthenticationRecord");

            this.HasKey(ear => ear.Id);

            this.HasRequired(ear => ear.User)
                .WithMany(c => c.ExternalAuthenticationRecords)
                .HasForeignKey(ear => ear.UserId);

        }
    }
}