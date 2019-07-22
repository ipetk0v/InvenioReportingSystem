using System.Collections.Generic;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Messages;
//using Invenio.Core.Domain.Orders;

namespace Invenio.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public partial interface IExportManager
    {
        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        //string ExportManufacturersToXml(IList<Manufacturer> manufacturers);

        /// <summary>
        /// Export manufacturers to XLSX
        /// </summary>
        /// <param name="manufacturers">Manufactures</param>
        //byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers);

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        //string ExportCategoriesToXml();

        /// <summary>
        /// Export categories to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        //byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories);

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        //string ExportProductsToXml(IList<Product> products);

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="products">Products</param>
        //byte[] ExportProductsToXlsx(IEnumerable<Product> products);

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        //string ExportOrdersToXml(IList<Order> orders);

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        //byte[] ExportOrdersToXlsx(IList<Order> orders);

        /// <summary>
        /// Export User list to XLSX
        /// </summary>
        /// <param name="Users">Users</param>
        byte[] ExportUsersToXlsx(IList<User> Users);

        /// <summary>
        /// Export User list to xml
        /// </summary>
        /// <param name="Users">Users</param>
        /// <returns>Result in XML format</returns>
        string ExportUsersToXml(IList<User> Users);

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        //string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions);

        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        string ExportStatesToTxt(IList<StateProvince> states);
    }
}
