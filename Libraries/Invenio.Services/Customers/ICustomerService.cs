using System.Collections.Generic;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Domain.Customers;

namespace Invenio.Services.Customers
{
    public interface ICustomerService
    {
        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="countryId">Country Id</param>
        /// <param name="stateId">State province Id</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturers</returns>
        IPagedList<Customer> GetAllCustomers(string customerName = "",
            int countryId = 0,
            int stateId = 0,
            int manufacturerId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false);

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void DeleteCustomer(Customer customer);

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        Customer GetCustomerById(int customerId);

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void InsertCustomer(Customer customer);

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        void UpdateCustomer(Customer customer);

        IPagedList<Customer> GetCustomersByManufacturer(int manufacturerId, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
