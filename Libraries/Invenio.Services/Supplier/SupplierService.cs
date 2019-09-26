using Invenio.Core;
using Invenio.Core.Data;
using Invenio.Services.Events;
using System;
using System.Linq;

namespace Invenio.Services.Supplier
{
    public class SupplierService : ISupplierService
    {
        private readonly IRepository<Core.Domain.Suppliers.Supplier> _supplierRepository;
        private readonly IEventPublisher _eventPublisher;

        public SupplierService(
            IRepository<Core.Domain.Suppliers.Supplier> supplierRepository,
            IEventPublisher eventPublisher
            )
        {
            _supplierRepository = supplierRepository;
            _eventPublisher = eventPublisher;
        }

        public IPagedList<Core.Domain.Suppliers.Supplier> GetAllSuppliers(
            string supplierName = "", int countryId = 0, int stateId = 0, int customerId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool? showHidden = null)
        {
            var query = _supplierRepository.Table;
            query = query.Where(m => !m.Deleted);

            if (showHidden.HasValue)
                query = query.Where(m => m.Published == showHidden);

            if (!string.IsNullOrWhiteSpace(supplierName))
                query = query.Where(m => m.Name.Contains(supplierName));

            if (countryId > 0)
                query = query.Where(m => m.Address.CountryId == countryId);

            if (stateId > 0)
                query = query.Where(s => s.Address.StateProvinceId == stateId);

            if (customerId > 0)
                query = query.Where(c => c.CustomerId == customerId);

            query = query.OrderByDescending(m => m.Published).ThenBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Core.Domain.Suppliers.Supplier>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Delete a supplier
        /// </summary>
        /// <param name="supplier">Supplier</param>
        public virtual void DeleteSupplier(Core.Domain.Suppliers.Supplier supplier)
        {
            if (supplier == null)
                throw new ArgumentNullException(nameof(supplier));

            supplier.Deleted = true;
            UpdateSupplier(supplier);

            //event notification
            _eventPublisher.EntityDeleted(supplier);
        }

        /// <summary>
        /// Gets a supplier
        /// </summary>
        /// <param name="supplierId">Supplier identifier</param>
        /// <returns>A supplier</returns>
        public virtual Core.Domain.Suppliers.Supplier GetSupplierById(int supplierId)
        {
            if (supplierId == 0)
                return null;

            return _supplierRepository.GetById(supplierId);
        }

        /// <summary>
        /// Updates the supplier
        /// </summary>
        /// <param name="supplier">Supplier</param>
        public virtual void UpdateSupplier(Core.Domain.Suppliers.Supplier supplier)
        {
            if (supplier == null)
                throw new ArgumentNullException(nameof(supplier));

            _supplierRepository.Update(supplier);

            //event notification
            _eventPublisher.EntityUpdated(supplier);
        }

        /// <summary>
        /// Insert a supplier
        /// </summary>
        /// <param name="supplier">Supplier</param>
        public virtual void InsertSupplier(Core.Domain.Suppliers.Supplier supplier)
        {
            if (supplier == null)
                throw new ArgumentNullException(nameof(supplier));

            _supplierRepository.Insert(supplier);

            //event notification
            _eventPublisher.EntityInserted(supplier);
        }

        public IPagedList<Core.Domain.Suppliers.Supplier> GetSuppliersByCustomer(int CustomerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _supplierRepository.Table;
            //if (!showHidden)
            //    query = query.Where(m => m.Published);
            //if (!string.IsNullOrWhiteSpace(SupplierName))
            //    query = query.Where(m => m.Name.Contains(SupplierName));
            //if (countryId > 0)
            //    query = query.Where(m => m.Address.CountryId == countryId);
            //if (stateId > 0)
            //    query = query.Where(s => s.Address.StateProvinceId == stateId);
            if (CustomerId > 0)
                query = query.Where(c => c.CustomerId == CustomerId);
            query = query.Where(m => !m.Deleted);
            query = query.OrderByDescending(m => m.Published).ThenBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Core.Domain.Suppliers.Supplier>(query, pageIndex, pageSize);
        }
    }
}
