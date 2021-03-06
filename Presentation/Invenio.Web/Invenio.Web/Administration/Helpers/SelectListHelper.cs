﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Admin.Infrastructure.Cache;
using Invenio.Core.Caching;
using Invenio.Services.Catalog;
//using Invenio.Services.Vendors;

namespace Invenio.Admin.Helpers
{
    /// <summary>
    /// Select list helper
    /// </summary>
    public static class SelectListHelper
    {
        ///// <summary>
        ///// Get category list
        ///// </summary>
        ///// <param name="categoryService">Category service</param>
        ///// <param name="cacheManager">Cache manager</param>
        ///// <param name="showHidden">A value indicating whether to show hidden records</param>
        ///// <returns>Category list</returns>
        //public static List<SelectListItem> GetCategoryList(ICategoryService categoryService, ICacheManager cacheManager, bool showHidden = false)
        //{
        //    if (categoryService == null)
        //        throw new ArgumentNullException("categoryService");

        //    if (cacheManager == null)
        //        throw new ArgumentNullException("cacheManager");

        //    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORIES_LIST_KEY, showHidden);
        //    var listItems = cacheManager.Get(cacheKey, () =>
        //    {
        //        var categories = categoryService.GetAllCategories(showHidden: showHidden);
        //        return categories.Select(c => new SelectListItem
        //        {
        //            Text = c.GetFormattedBreadCrumb(categories),
        //            Value = c.Id.ToString()
        //        });
        //    });

        //    var result = new List<SelectListItem>();
        //    //clone the list to ensure that "selected" property is not set
        //    foreach (var item in listItems)
        //    {
        //        result.Add(new SelectListItem
        //        {
        //            Text = item.Text,
        //            Value = item.Value
        //        });
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Get Customer list
        ///// </summary>
        ///// <param name="CustomerService">Customer service</param>
        ///// <param name="cacheManager">Cache manager</param>
        ///// <param name="showHidden">A value indicating whether to show hidden records</param>
        ///// <returns>Customer list</returns>
        //public static List<SelectListItem> GetCustomerList(ICustomerService CustomerService, ICacheManager cacheManager, bool showHidden = false)
        //{
        //    if (CustomerService == null)
        //        throw new ArgumentNullException("CustomerService");

        //    if (cacheManager == null)
        //        throw new ArgumentNullException("cacheManager");

        //    string cacheKey = string.Format(ModelCacheEventConsumer.CustomerS_LIST_KEY, showHidden);
        //    var listItems = cacheManager.Get(cacheKey, () =>
        //    {
        //        var Customers = CustomerService.GetAllCustomers(showHidden: showHidden);
        //        return Customers.Select(m => new SelectListItem
        //        {
        //            Text = m.Name,
        //            Value = m.Id.ToString()
        //        });
        //    });

        //    var result = new List<SelectListItem>();
        //    //clone the list to ensure that "selected" property is not set
        //    foreach (var item in listItems)
        //    {
        //        result.Add(new SelectListItem
        //        {
        //            Text = item.Text,
        //            Value = item.Value
        //        });
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Get vendor list
        ///// </summary>
        ///// <param name="vendorService">Vendor service</param>
        ///// <param name="cacheManager">Cache manager</param>
        ///// <param name="showHidden">A value indicating whether to show hidden records</param>
        ///// <returns>Vendor list</returns>
        //public static List<SelectListItem> GetVendorList(IVendorService vendorService, ICacheManager cacheManager, bool showHidden = false)
        //{
        //    if (vendorService == null)
        //        throw new ArgumentNullException("vendorService");

        //    if (cacheManager == null)
        //        throw new ArgumentNullException("cacheManager");

        //    string cacheKey = string.Format(ModelCacheEventConsumer.VENDORS_LIST_KEY, showHidden);
        //    var listItems = cacheManager.Get(cacheKey, () =>
        //    {
        //        var vendors = vendorService.GetAllVendors(showHidden: showHidden);
        //        return vendors.Select(v => new SelectListItem
        //        {
        //            Text = v.Name,
        //            Value = v.Id.ToString()
        //        });
        //    });

        //    var result = new List<SelectListItem>();
        //    //clone the list to ensure that "selected" property is not set
        //    foreach (var item in listItems)
        //    {
        //        result.Add(new SelectListItem
        //        {
        //            Text = item.Text,
        //            Value = item.Value
        //        });
        //    }

        //    return result;
        //}
    }
}