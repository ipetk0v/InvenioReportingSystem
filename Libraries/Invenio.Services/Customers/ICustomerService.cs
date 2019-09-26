using Invenio.Core;
using Invenio.Core.Domain.Customers;
using System.Collections.Generic;

namespace Invenio.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial interface ICustomerService
    {
        /// <summary>
        /// Deletes a Customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void DeleteCustomer(Customer customer);

        /// <summary>
        /// Gets all Customers
        /// </summary>
        /// <param name="customerName">Customer name</param>
        /// <param name="countryId">Country Id</param>
        /// <param name="stateId">State province Id</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Customers</returns>
        IPagedList<Customer> GetAllCustomers(string customerName = "",
            int countryId = 0,
            int stateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool? showHidden = null);

        /// <summary>
        /// Gets a Customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Customer</returns>
        Customer GetCustomerById(int customerId);

        /// <summary>
        /// Inserts a Customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertCustomer(Customer customer);

        /// <summary>
        /// Updates the Customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomer(Customer customer);
       
    }
}
