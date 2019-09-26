namespace Invenio.Data.Mapping.Supplier
{
    public class SupplierMap : NopEntityTypeConfiguration<Core.Domain.Suppliers.Supplier>
    {
        public SupplierMap()
        {
            ToTable("Supplier");
            this.HasKey(c => c.Id);
            this.Property(x => x.Comment).HasMaxLength(200);
            this.Property(x => x.Name).HasMaxLength(100);
        }
    }
}
