using Invenio.Core;

namespace Invenio.Services.Supplier
{
    public interface ISupplierService
    {
        /// <summary>
        /// Gets all Customers
        /// </summary>
        /// <param name="supplierName"></param>
        /// <param name="countryId">Country Id</param>
        /// <param name="stateId">State province Id</param>
        /// <param name="customerId"></param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Customers</returns>
        IPagedList<Core.Domain.Suppliers.Supplier> GetAllSuppliers(string supplierName = "",
            int countryId = 0,
            int stateId = 0,
            int customerId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool? showHidden = null);

        /// <summary>
        /// Delete a supplier
        /// </summary>
        /// <param name="supplier">Supplier</param>
        void DeleteSupplier(Core.Domain.Suppliers.Supplier supplier);

        /// <summary>
        /// Gets a supplier
        /// </summary>
        /// <param name="supplierId">Supplier identifier</param>
        /// <returns>A supplier</returns>
        Core.Domain.Suppliers.Supplier GetSupplierById(int supplierId);

        /// <summary>
        /// Insert a supplier
        /// </summary>
        /// <param name="supplier">Supplier</param>
        void InsertSupplier(Core.Domain.Suppliers.Supplier supplier);

        /// <summary>
        /// Updates the supplier
        /// </summary>
        /// <param name="supplier">Supplier</param>
        void UpdateSupplier(Core.Domain.Suppliers.Supplier supplier);

        IPagedList<Core.Domain.Suppliers.Supplier> GetSuppliersByCustomer(int CustomerId, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
