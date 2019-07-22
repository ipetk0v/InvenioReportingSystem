using Invenio.Core;
using Invenio.Core.Domain.Customers;
using System;
using Invenio.Core.Data;
using System.Linq;
using Invenio.Services.Events;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IEventPublisher _eventPublisher;

        public CustomerService(
            IRepository<Customer> customerRepository,
            IEventPublisher eventPublisher
            )
        {
            _customerRepository = customerRepository;
            _eventPublisher = eventPublisher;
        }

        public IPagedList<Customer> GetAllCustomers(
            string customerName = "", int countryId = 0, int stateId = 0, int manufacturerId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _customerRepository.Table;
            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!string.IsNullOrWhiteSpace(customerName))
                query = query.Where(m => m.Name.Contains(customerName));
            if (countryId > 0)
                query = query.Where(m => m.Address.CountryId == countryId);
            if (stateId > 0)
                query = query.Where(s => s.Address.StateProvinceId == stateId);
            if (manufacturerId > 0)
                query = query.Where(c => c.ManufacturerId == manufacturerId);
            query = query.Where(m => !m.Deleted);
            query = query.OrderByDescending(m => m.Published).ThenBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Customer>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void DeleteCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            customer.Deleted = true;
            UpdateCustomer(customer);

            //event notification
            _eventPublisher.EntityDeleted(customer);
        }

        /// <summary>
        /// Gets a customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A customer</returns>
        public virtual Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;

            return _customerRepository.GetById(customerId);
        }

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void UpdateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Update(customer);

            //event notification
            _eventPublisher.EntityUpdated(customer);
        }

        /// <summary>
        /// Insert a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        public virtual void InsertCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Insert(customer);

            //event notification
            _eventPublisher.EntityInserted(customer);
        }

        public IPagedList<Customer> GetCustomersByManufacturer(int manufacturerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerRepository.Table;
            //if (!showHidden)
            //    query = query.Where(m => m.Published);
            //if (!string.IsNullOrWhiteSpace(customerName))
            //    query = query.Where(m => m.Name.Contains(customerName));
            //if (countryId > 0)
            //    query = query.Where(m => m.Address.CountryId == countryId);
            //if (stateId > 0)
            //    query = query.Where(s => s.Address.StateProvinceId == stateId);
            if (manufacturerId > 0)
                query = query.Where(c => c.ManufacturerId == manufacturerId);
            query = query.Where(m => !m.Deleted);
            query = query.OrderByDescending(m => m.Published).ThenBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Customer>(query, pageIndex, pageSize);
        }
    }
}
