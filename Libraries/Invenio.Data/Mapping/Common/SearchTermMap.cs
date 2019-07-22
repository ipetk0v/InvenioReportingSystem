using Invenio.Core.Domain.Common;

namespace Invenio.Data.Mapping.Common
{
    public partial class SearchTermMap : NopEntityTypeConfiguration<SearchTerm>
    {
        public SearchTermMap()
        {
            this.ToTable("SearchTerm");
            this.HasKey(st => st.Id);
        }
    }
}
