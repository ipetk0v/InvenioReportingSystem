using Invenio.Core.Domain.Tasks;

namespace Invenio.Data.Mapping.Tasks
{
    public partial class ScheduleTaskMap : NopEntityTypeConfiguration<ScheduleTask>
    {
        public ScheduleTaskMap()
        {
            this.ToTable("ScheduleTask");
            this.HasKey(t => t.Id);
            this.Property(t => t.Name).IsRequired();
            this.Property(t => t.Type).IsRequired();
        }
    }
}
