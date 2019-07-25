using System.Collections.Generic;
using System.IO;
using Invenio.Core.Domain.Catalog;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Shipping;

namespace Invenio.Services.Common
{
    /// <summary>
    /// User service interface
    /// </summary>
    public partial interface IPdfService
    {
        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to to print all products. If specified, then totals won't be printed</param>
        /// <returns>A path of generated file</returns>
        //string PrintOrderToPdf(Order order, int languageId = 0, int vendorId = 0);

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to to print all products. If specified, then totals won't be printed</param>
        //void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0);

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        //void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, int languageId = 0);


        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        //void PrintProductsToPdf(Stream stream, IList<Product> products);

        byte[] PrintDailyReportToPdf(string html, string css);
    }
}