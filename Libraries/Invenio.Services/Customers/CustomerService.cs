using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Data;
using Invenio.Core.Domain.Customers;
using Invenio.Core.Domain.Security;
using Invenio.Core.Domain.Stores;
using Invenio.Services.Events;
using System;
using System.Linq;

namespace Invenio.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomerService : ICustomerService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : Customer ID
        /// </remarks>
        private const string CustomerS_BY_ID_KEY = "Nop.Customer.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : Customer ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current supplier ID
        /// {5} : store ID
        /// </remarks>
        private const string PRODUCTCustomerS_ALLBYCustomerID_KEY = "Nop.productCustomer.allbyCustomerid-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : product ID
        /// {2} : current supplier ID
        /// {3} : store ID
        /// </remarks>
        private const string PRODUCTCustomerS_ALLBYPRODUCTID_KEY = "Nop.productCustomer.allbyproductid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CustomerS_PATTERN_KEY = "Nop.Customer.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTCustomerS_PATTERN_KEY = "Nop.productCustomer.";

        #endregion

        #region Fields

        private readonly IRepository<Customer> _CustomerRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="CustomerRepository">Category repository</param>
        /// <param name="productCustomerRepository">ProductCategory repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event published</param>
        public CustomerService(ICacheManager cacheManager,
            IRepository<Customer> CustomerRepository,
            IRepository<AclRecord> aclRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._CustomerRepository = CustomerRepository;
            this._aclRepository = aclRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Deletes a Customer
        /// </summary>
        /// <param name="Customer">Customer</param>
        public virtual void DeleteCustomer(Customer Customer)
        {
            if (Customer == null)
                throw new ArgumentNullException("Customer");

            Customer.Deleted = true;
            UpdateCustomer(Customer);

            //event notification
            _eventPublisher.EntityDeleted(Customer);
        }

        /// <summary>
        /// Gets all Customers
        /// </summary>
        /// <param name="customerName">Customer name</param>
        /// <param name="stateId"></param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="countryId"></param>
        /// <returns>Customers</returns>
        public virtual IPagedList<Customer> GetAllCustomers(string customerName = "",
            int countryId = 0,
            int stateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool? showHidden = null)
        {
            var query = _CustomerRepository.Table;
            if (showHidden.HasValue)
                query = query.Where(m => m.Published == showHidden);
            if (!string.IsNullOrWhiteSpace(customerName))
                query = query.Where(m => m.Name.Contains(customerName));
            if (countryId > 0)
                query = query.Where(m => m.CountryId == countryId);
            if (stateId > 0)
                query = query.Where(s => s.StateProvinceId == stateId);
            query = query.Where(m => !m.Deleted);
            query = query.OrderByDescending(m => m.Published).ThenBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Customer>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Customer
        /// </summary>
        /// <param name="CustomerId">Customer identifier</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerById(int CustomerId)
        {
            if (CustomerId == 0)
                return null;

            string key = string.Format(CustomerS_BY_ID_KEY, CustomerId);
            return _cacheManager.Get(key, () => _CustomerRepository.GetById(CustomerId));
        }

        /// <summary>
        /// Inserts a Customer
        /// </summary>
        /// <param name="Customer">Customer</param>
        public virtual void InsertCustomer(Customer Customer)
        {
            if (Customer == null)
                throw new ArgumentNullException("Customer");

            _CustomerRepository.Insert(Customer);

            //cache
            _cacheManager.RemoveByPattern(CustomerS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCustomerS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(Customer);
        }

        /// <summary>
        /// Updates the Customer
        /// </summary>
        /// <param name="Customer">Customer</param>
        public virtual void UpdateCustomer(Customer Customer)
        {
            if (Customer == null)
                throw new ArgumentNullException("Customer");

            _CustomerRepository.Update(Customer);

            //cache
            _cacheManager.RemoveByPattern(CustomerS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTCustomerS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(Customer);
        }
        #endregion
    }
}
