using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Invenio.Core;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Messages;
using Invenio.Services.Common;
using Invenio.Services.Users;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Shipping;
//using Invenio.Core.Domain.Tax;
//using Invenio.Core.Domain.Vendors;
//using Invenio.Services.Catalog;
//using Invenio.Services.Common;
//using Invenio.Services.Users;
//using Invenio.Services.Directory;
using Invenio.Services.ExportImport.Help;
using Invenio.Services.Media;
using Invenio.Services.Messages;
//using Invenio.Services.Seo;
//using Invenio.Services.Shipping.Date;
using Invenio.Services.Stores;
//using Invenio.Services.Tax;
//using Invenio.Services.Vendors;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Invenio.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        //private readonly ICategoryService _categoryService;
        //private readonly ICustomerService _CustomerService;
        private readonly IUserService _UserService;
        //private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        //private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        //private readonly ProductEditorSettings _productEditorSettings;
        //private readonly IVendorService _vendorService;
        //private readonly IProductTemplateService _productTemplateService;
        //private readonly IDateRangeService _dateRangeService;
        //private readonly ITaxCategoryService _taxCategoryService;
        //private readonly IMeasureService _measureService;
        //private readonly CatalogSettings _catalogSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUserAttributeFormatter _UserAttributeFormatter;
        //private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public ExportManager(
            //ICategoryService categoryService,
            //ICustomerService CustomerService,
            IUserService UserService,
            //IProductAttributeService productAttributeService,
            IPictureService pictureService,
            //INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreService storeService,
            IWorkContext workContext,
            //ProductEditorSettings productEditorSettings,
            //IVendorService vendorService,
            //IProductTemplateService productTemplateService,
            //IDateRangeService dateRangeService,
            //ITaxCategoryService taxCategoryService,
            //IMeasureService measureService,
            //CatalogSettings catalogSettings,
            IGenericAttributeService genericAttributeService,
            IUserAttributeFormatter UserAttributeFormatter
            //OrderSettings orderSettings
            )
        {
            //this._categoryService = categoryService;
            //this._CustomerService = CustomerService;
            this._UserService = UserService;
            //this._productAttributeService = productAttributeService;
            this._pictureService = pictureService;
            //this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._storeService = storeService;
            this._workContext = workContext;
            //this._productEditorSettings = productEditorSettings;
            //this._vendorService = vendorService;
            //this._productTemplateService = productTemplateService;
            //this._dateRangeService = dateRangeService;
            //this._taxCategoryService = taxCategoryService;
            //this._measureService = measureService;
            //this._catalogSettings = catalogSettings;
            this._genericAttributeService = genericAttributeService;
            this._UserAttributeFormatter = UserAttributeFormatter;
            //this._orderSettings = orderSettings;
        }

        #endregion

        #region Utilities

        //protected virtual void WriteCategories(XmlWriter xmlWriter, int parentCategoryId)
        //{
        //    var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
        //    if (categories != null && categories.Any())
        //    {
        //        foreach (var category in categories)
        //        {
        //            xmlWriter.WriteStartElement("Category");

        //            xmlWriter.WriteString("Id", category.Id);

        //            xmlWriter.WriteString("Name", category.Name);
        //            xmlWriter.WriteString("Description", category.Description);
        //            xmlWriter.WriteString("CategoryTemplateId", category.CategoryTemplateId);
        //            xmlWriter.WriteString("MetaKeywords", category.MetaKeywords, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("MetaDescription", category.MetaDescription, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("MetaTitle", category.MetaTitle, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("SeName", category.GetSeName(0), IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("ParentCategoryId", category.ParentCategoryId);
        //            xmlWriter.WriteString("PictureId", category.PictureId);
        //            xmlWriter.WriteString("PageSize", category.PageSize, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("AllowUsersToSelectPageSize", category.AllowUsersToSelectPageSize, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("PageSizeOptions", category.PageSizeOptions, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("PriceRanges", category.PriceRanges, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("ShowOnHomePage", category.ShowOnHomePage, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("IncludeInTopMenu", category.IncludeInTopMenu, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("Published", category.Published, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("Deleted", category.Deleted, true);
        //            xmlWriter.WriteString("DisplayOrder", category.DisplayOrder);
        //            xmlWriter.WriteString("CreatedOnUtc", category.CreatedOnUtc, IgnoreExportCategoryProperty());
        //            xmlWriter.WriteString("UpdatedOnUtc", category.UpdatedOnUtc, IgnoreExportCategoryProperty());

        //            xmlWriter.WriteStartElement("Products");
        //            var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, showHidden: true);
        //            foreach (var productCategory in productCategories)
        //            {
        //                var product = productCategory.Product;
        //                if (product != null && !product.Deleted)
        //                {
        //                    xmlWriter.WriteStartElement("ProductCategory");
        //                    xmlWriter.WriteString("ProductCategoryId", productCategory.Id);
        //                    xmlWriter.WriteString("ProductId", productCategory.ProductId);
        //                    xmlWriter.WriteString("ProductName", product.Name);
        //                    xmlWriter.WriteString("IsFeaturedProduct", productCategory.IsFeaturedProduct);
        //                    xmlWriter.WriteString("DisplayOrder", productCategory.DisplayOrder);
        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //            xmlWriter.WriteEndElement();

        //            xmlWriter.WriteStartElement("SubCategories");
        //            WriteCategories(xmlWriter, category.Id);
        //            xmlWriter.WriteEndElement();
        //            xmlWriter.WriteEndElement();
        //        }
        //    }
        //}

        protected virtual void SetCaptionStyle(ExcelStyle style)
        {
            style.Fill.PatternType = ExcelFillStyle.Solid;
            style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
            style.Font.Bold = true;
        }

        /// <summary>
        /// Returns the path to the image file by ID
        /// </summary>
        /// <param name="pictureId">Picture ID</param>
        /// <returns>Path to the image file</returns>
        protected virtual string GetPictures(int pictureId)
        {
            var picture = _pictureService.GetPictureById(pictureId);
            return _pictureService.GetThumbLocalPath(picture);
        }

        /// <summary>
        /// Returns the list of categories for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of categories</returns>
        //protected virtual string GetCategories(Product product)
        //{
        //    string categoryNames = null;
        //    foreach (var pc in _categoryService.GetProductCategoriesByProductId(product.Id, true))
        //    {
        //        categoryNames += pc.Category.Name;
        //        categoryNames += ";";
        //    }
        //    return categoryNames;
        //}

        /// <summary>
        /// Returns the list of Customer for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of Customer</returns>
        //protected virtual string GetCustomers(Product product)
        //{
        //    string CustomerNames = null;
        //    foreach (var pm in _CustomerService.GetProductCustomersByProductId(product.Id, true))
        //    {
        //        CustomerNames += pm.Customer.Name;
        //        CustomerNames += ";";
        //    }
        //    return CustomerNames;
        //}

        /// <summary>
        /// Returns the list of product tag for a product separated by a ";"
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product tag</returns>
        //protected virtual string GetProductTags(Product product)
        //{
        //    string productTagNames = null;

        //    foreach (var productTag in product.ProductTags)
        //    {
        //        productTagNames += productTag.Name;
        //        productTagNames += ";";
        //    }
        //    return productTagNames;
        //}

        /// <summary>
        /// Returns the three first image associated with the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>three first image</returns>
        //protected virtual string[] GetPictures(Product product)
        //{
        //    //pictures (up to 3 pictures)
        //    string picture1 = null;
        //    string picture2 = null;
        //    string picture3 = null;
        //    var pictures = _pictureService.GetPicturesByProductId(product.Id, 3);
        //    for (var i = 0; i < pictures.Count; i++)
        //    {
        //        var pictureLocalPath = _pictureService.GetThumbLocalPath(pictures[i]);
        //        switch (i)
        //        {
        //            case 0:
        //                picture1 = pictureLocalPath;
        //                break;
        //            case 1:
        //                picture2 = pictureLocalPath;
        //                break;
        //            case 2:
        //                picture3 = pictureLocalPath;
        //                break;
        //        }
        //    }
        //    return new[] { picture1, picture2, picture3 };
        //}

        //private bool IgnoreExportPoductProperty(Func<ProductEditorSettings, bool> func)
        //{
        //    var productAdvancedMode = _workContext.CurrentUser.GetAttribute<bool>("product-advanced-mode");
        //    return !productAdvancedMode && !func(_productEditorSettings);
        //}

        //private bool IgnoreExportCategoryProperty()
        //{
        //    return !_workContext.CurrentUser.GetAttribute<bool>("category-advanced-mode");
        //}

        //private bool IgnoreExportCustomerProperty()
        //{
        //    return !_workContext.CurrentUser.GetAttribute<bool>("Customer-advanced-mode");
        //}

        /// <summary>
        /// Export objects to XLSX
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="properties">Class access to the object through its properties</param>
        /// <param name="itemsToExport">The objects to export</param>
        /// <returns></returns>
        protected virtual byte[] ExportToXlsx<T>(PropertyByName<T>[] properties, IEnumerable<T> itemsToExport)
        {
            using (var stream = new MemoryStream())
            {
                // ok, we can run the real code of the sample now
                using (var xlPackage = new ExcelPackage(stream))
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handles to the worksheets
                    var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(T).Name);
                    var fWorksheet = xlPackage.Workbook.Worksheets.Add("DataForFilters");
                    fWorksheet.Hidden = eWorkSheetHidden.VeryHidden;

                    //create Headers and format them 
                    var manager = new PropertyManager<T>(properties.Where(p => !p.Ignore));
                    manager.WriteCaption(worksheet, SetCaptionStyle);

                    var row = 2;
                    foreach (var items in itemsToExport)
                    {
                        manager.CurrentObject = items;
                        manager.WriteToXlsx(worksheet, row++, false, fWorksheet: fWorksheet);
                    }

                    xlPackage.Save();
                }
                return stream.ToArray();
            }
        }

        //private byte[] ExportProductsToXlsxWithAttributes(PropertyByName<Product>[] properties, IEnumerable<Product> itemsToExport)
        //{
        //    var attributeProperties = new[]
        //    {
        //        new PropertyByName<ExportProductAttribute>("AttributeId", p => p.AttributeId),
        //        new PropertyByName<ExportProductAttribute>("AttributeName", p => p.AttributeName),
        //        new PropertyByName<ExportProductAttribute>("AttributeTextPrompt", p => p.AttributeTextPrompt),
        //        new PropertyByName<ExportProductAttribute>("AttributeIsRequired", p => p.AttributeIsRequired),
        //        new PropertyByName<ExportProductAttribute>("AttributeControlType", p => p.AttributeControlTypeId)
        //        {
        //            DropDownElements = AttributeControlType.TextBox.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<ExportProductAttribute>("AttributeDisplayOrder", p => p.AttributeDisplayOrder),
        //        new PropertyByName<ExportProductAttribute>("ProductAttributeValueId", p => p.Id),
        //        new PropertyByName<ExportProductAttribute>("ValueName", p => p.Name),
        //        new PropertyByName<ExportProductAttribute>("AttributeValueType", p => p.AttributeValueTypeId)
        //        {
        //            DropDownElements = AttributeValueType.Simple.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<ExportProductAttribute>("AssociatedProductId", p => p.AssociatedProductId),
        //        new PropertyByName<ExportProductAttribute>("ColorSquaresRgb", p => p.ColorSquaresRgb),
        //        new PropertyByName<ExportProductAttribute>("ImageSquaresPictureId", p => p.ImageSquaresPictureId),
        //        new PropertyByName<ExportProductAttribute>("PriceAdjustment", p => p.PriceAdjustment),
        //        new PropertyByName<ExportProductAttribute>("WeightAdjustment", p => p.WeightAdjustment),
        //        new PropertyByName<ExportProductAttribute>("Cost", p => p.Cost),
        //        new PropertyByName<ExportProductAttribute>("UserEntersQty", p => p.UserEntersQty),
        //        new PropertyByName<ExportProductAttribute>("Quantity", p => p.Quantity),
        //        new PropertyByName<ExportProductAttribute>("IsPreSelected", p => p.IsPreSelected),
        //        new PropertyByName<ExportProductAttribute>("DisplayOrder", p => p.DisplayOrder),
        //        new PropertyByName<ExportProductAttribute>("PictureId", p => p.PictureId)
        //    };

        //    var attributeManager = new PropertyManager<ExportProductAttribute>(attributeProperties);

        //    using (var stream = new MemoryStream())
        //    {
        //        // ok, we can run the real code of the sample now
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            // uncomment this line if you want the XML written out to the outputDir
        //            //xlPackage.DebugMode = true; 

        //            // get handles to the worksheets
        //            var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(Product).Name);
        //            var fpWorksheet = xlPackage.Workbook.Worksheets.Add("DataForProductsFilters");
        //            fpWorksheet.Hidden = eWorkSheetHidden.VeryHidden;
        //            var faWorksheet = xlPackage.Workbook.Worksheets.Add("DataForProductAttributesFilters");
        //            faWorksheet.Hidden = eWorkSheetHidden.VeryHidden;

        //            //create Headers and format them 
        //            var manager = new PropertyManager<Product>(properties.Where(p => !p.Ignore));
        //            manager.WriteCaption(worksheet, SetCaptionStyle);

        //            var row = 2;
        //            foreach (var item in itemsToExport)
        //            {
        //                manager.CurrentObject = item;
        //                manager.WriteToXlsx(worksheet, row++, _catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities, fWorksheet: fpWorksheet);

        //                var attributes = item.ProductAttributeMappings.SelectMany(pam => pam.ProductAttributeValues.Select(pav => new ExportProductAttribute
        //                {
        //                    AttributeId = pam.ProductAttribute.Id,
        //                    AttributeName = pam.ProductAttribute.Name,
        //                    AttributeTextPrompt = pam.TextPrompt,
        //                    AttributeIsRequired = pam.IsRequired,
        //                    AttributeControlTypeId = pam.AttributeControlTypeId,
        //                    AssociatedProductId = pav.AssociatedProductId,
        //                    AttributeDisplayOrder = pam.DisplayOrder,
        //                    Id = pav.Id,
        //                    Name = pav.Name,
        //                    AttributeValueTypeId = pav.AttributeValueTypeId,
        //                    ColorSquaresRgb = pav.ColorSquaresRgb,
        //                    ImageSquaresPictureId = pav.ImageSquaresPictureId,
        //                    PriceAdjustment = pav.PriceAdjustment,
        //                    WeightAdjustment = pav.WeightAdjustment,
        //                    Cost = pav.Cost,
        //                    UserEntersQty = pav.UserEntersQty,
        //                    Quantity = pav.Quantity,
        //                    IsPreSelected = pav.IsPreSelected,
        //                    DisplayOrder = pav.DisplayOrder,
        //                    PictureId = pav.PictureId
        //                })).ToList();

        //                attributes.AddRange(item.ProductAttributeMappings.Where(pam => !pam.ProductAttributeValues.Any()).Select(pam => new ExportProductAttribute
        //                {
        //                    AttributeId = pam.ProductAttribute.Id,
        //                    AttributeName = pam.ProductAttribute.Name,
        //                    AttributeTextPrompt = pam.TextPrompt,
        //                    AttributeIsRequired = pam.IsRequired,
        //                    AttributeControlTypeId = pam.AttributeControlTypeId
        //                }));

        //                if (!attributes.Any())
        //                    continue;

        //                attributeManager.WriteCaption(worksheet, SetCaptionStyle, row, ExportProductAttribute.ProducAttributeCellOffset);
        //                worksheet.Row(row).OutlineLevel = 1;
        //                worksheet.Row(row).Collapsed = true;

        //                foreach (var exportProducAttribute in attributes)
        //                {
        //                    row++;
        //                    attributeManager.CurrentObject = exportProducAttribute;
        //                    attributeManager.WriteToXlsx(worksheet, row, _catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities, ExportProductAttribute.ProducAttributeCellOffset, faWorksheet);
        //                    worksheet.Row(row).OutlineLevel = 1;
        //                    worksheet.Row(row).Collapsed = true;
        //                }

        //                row++;
        //            }

        //            xlPackage.Save();
        //        }
        //        return stream.ToArray();
        //    }
        //}

        //private byte[] ExportOrderToXlsxWithProducts(PropertyByName<Order>[] properties, IEnumerable<Order> itemsToExport)
        //{
        //    var orderItemProperties = new[]
        //    {
        //        new PropertyByName<OrderItem>("Name", oi => oi.Product.Name),
        //        new PropertyByName<OrderItem>("Sku", oi => oi.Product.Sku),
        //        new PropertyByName<OrderItem>("PriceExclTax", oi => oi.UnitPriceExclTax),
        //        new PropertyByName<OrderItem>("PriceInclTax", oi => oi.UnitPriceInclTax),
        //        new PropertyByName<OrderItem>("Quantity", oi => oi.Quantity),
        //        new PropertyByName<OrderItem>("DiscountExclTax", oi => oi.DiscountAmountExclTax),
        //        new PropertyByName<OrderItem>("DiscountInclTax", oi => oi.DiscountAmountInclTax),
        //        new PropertyByName<OrderItem>("TotalExclTax", oi => oi.PriceExclTax),
        //        new PropertyByName<OrderItem>("TotalInclTax", oi => oi.PriceInclTax)
        //    };

        //    var orderItemsManager = new PropertyManager<OrderItem>(orderItemProperties);

        //    using (var stream = new MemoryStream())
        //    {
        //        // ok, we can run the real code of the sample now
        //        using (var xlPackage = new ExcelPackage(stream))
        //        {
        //            // uncomment this line if you want the XML written out to the outputDir
        //            //xlPackage.DebugMode = true; 

        //            // get handles to the worksheets
        //            var worksheet = xlPackage.Workbook.Worksheets.Add(typeof(Order).Name);
        //            var fpWorksheet = xlPackage.Workbook.Worksheets.Add("DataForProductsFilters");
        //            fpWorksheet.Hidden = eWorkSheetHidden.VeryHidden;

        //            //create Headers and format them 
        //            var manager = new PropertyManager<Order>(properties.Where(p => !p.Ignore));
        //            manager.WriteCaption(worksheet, SetCaptionStyle);

        //            var row = 2;
        //            foreach (var order in itemsToExport)
        //            {
        //                manager.CurrentObject = order;
        //                manager.WriteToXlsx(worksheet, row++, _catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities);

        //                //products
        //                var orederItems = order.OrderItems.ToList();

        //                //a vendor should have access only to his products
        //                if (_workContext.CurrentVendor != null)
        //                    orederItems = orederItems.Where(p => p.Product.VendorId == _workContext.CurrentVendor.Id).ToList();

        //                if (!orederItems.Any())
        //                    continue;

        //                orderItemsManager.WriteCaption(worksheet, SetCaptionStyle, row, 2);
        //                worksheet.Row(row).OutlineLevel = 1;
        //                worksheet.Row(row).Collapsed = true;

        //                foreach (var orederItem in orederItems)
        //                {
        //                    row++;
        //                    orderItemsManager.CurrentObject = orederItem;
        //                    orderItemsManager.WriteToXlsx(worksheet, row, _catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities, 2, fpWorksheet);
        //                    worksheet.Row(row).OutlineLevel = 1;
        //                    worksheet.Row(row).Collapsed = true;
        //                }

        //                row++;
        //            }

        //            xlPackage.Save();
        //        }
        //        return stream.ToArray();
        //    }
        //}

        private string GetCustomUserAttributes(User User)
        {
            var selectedUserAttributes = User.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes, _genericAttributeService);
            return _UserAttributeFormatter.FormatAttributes(selectedUserAttributes, ";");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export Customer list to xml
        /// </summary>
        /// <param name="Customers">Customers</param>
        /// <returns>Result in XML format</returns>
        //public virtual string ExportCustomersToXml(IList<Customer> Customers)
        //{
        //    var sb = new StringBuilder();
        //    var stringWriter = new StringWriter(sb);
        //    var xmlWriter = new XmlTextWriter(stringWriter);
        //    xmlWriter.WriteStartDocument();
        //    xmlWriter.WriteStartElement("Customers");
        //    xmlWriter.WriteAttributeString("Version", InvenioVersion.CurrentVersion);

        //    foreach (var Customer in Customers)
        //    {
        //        xmlWriter.WriteStartElement("Customer");

        //        xmlWriter.WriteString("CustomerId", Customer.Id);
        //        xmlWriter.WriteString("Name", Customer.Name);
        //        xmlWriter.WriteString("Description", Customer.Description);
        //        xmlWriter.WriteString("CustomerTemplateId", Customer.CustomerTemplateId);
        //        xmlWriter.WriteString("MetaKeywords", Customer.MetaKeywords, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("MetaDescription", Customer.MetaDescription, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("MetaTitle", Customer.MetaTitle, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("SEName", Customer.GetSeName(0), IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("PictureId", Customer.PictureId);
        //        xmlWriter.WriteString("PageSize", Customer.PageSize, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("AllowUsersToSelectPageSize", Customer.AllowUsersToSelectPageSize, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("PageSizeOptions", Customer.PageSizeOptions, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("PriceRanges", Customer.PriceRanges, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("Published", Customer.Published, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("Deleted", Customer.Deleted, true);
        //        xmlWriter.WriteString("DisplayOrder", Customer.DisplayOrder);
        //        xmlWriter.WriteString("CreatedOnUtc", Customer.CreatedOnUtc, IgnoreExportCustomerProperty());
        //        xmlWriter.WriteString("UpdatedOnUtc", Customer.UpdatedOnUtc, IgnoreExportCustomerProperty());

        //        xmlWriter.WriteStartElement("Products");
        //        var productCustomers = _CustomerService.GetProductCustomersByCustomerId(Customer.Id, showHidden: true);
        //        if (productCustomers != null)
        //        {
        //            foreach (var productCustomer in productCustomers)
        //            {
        //                var product = productCustomer.Product;
        //                if (product != null && !product.Deleted)
        //                {
        //                    xmlWriter.WriteStartElement("ProductCustomer");
        //                    xmlWriter.WriteString("ProductCustomerId", productCustomer.Id);
        //                    xmlWriter.WriteString("ProductId", productCustomer.ProductId);
        //                    xmlWriter.WriteString("ProductName", product.Name);
        //                    xmlWriter.WriteString("IsFeaturedProduct", productCustomer.IsFeaturedProduct);
        //                    xmlWriter.WriteString("DisplayOrder", productCustomer.DisplayOrder);
        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //        }
        //        xmlWriter.WriteEndElement();

        //        xmlWriter.WriteEndElement();
        //    }

        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndDocument();
        //    xmlWriter.Close();
        //    return stringWriter.ToString();
        //}

        ///// <summary>
        ///// Export Customers to XLSX
        ///// </summary>
        ///// <param name="Customers">Manufactures</param>
        //public virtual byte[] ExportCustomersToXlsx(IEnumerable<Customer> Customers)
        //{
        //    //property array
        //    var properties = new[]
        //    {
        //        new PropertyByName<Customer>("Id", p => p.Id),
        //        new PropertyByName<Customer>("Name", p => p.Name),
        //        new PropertyByName<Customer>("Description", p => p.Description),
        //        new PropertyByName<Customer>("CustomerTemplateId", p => p.CustomerTemplateId),
        //        new PropertyByName<Customer>("MetaKeywords", p => p.MetaKeywords, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("MetaDescription", p => p.MetaDescription, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("MetaTitle", p => p.MetaTitle, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("SeName", p => p.GetSeName(0), IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("Picture", p => GetPictures(p.PictureId)),
        //        new PropertyByName<Customer>("PageSize", p => p.PageSize, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("AllowUsersToSelectPageSize", p => p.AllowUsersToSelectPageSize, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("PageSizeOptions", p => p.PageSizeOptions, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("PriceRanges", p => p.PriceRanges, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("Published", p => p.Published, IgnoreExportCustomerProperty()),
        //        new PropertyByName<Customer>("DisplayOrder", p => p.DisplayOrder)
        //    };

        //    return ExportToXlsx(properties, Customers);
        //}

        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        //public virtual string ExportCategoriesToXml()
        //{
        //    var sb = new StringBuilder();
        //    var stringWriter = new StringWriter(sb);
        //    var xmlWriter = new XmlTextWriter(stringWriter);
        //    xmlWriter.WriteStartDocument();
        //    xmlWriter.WriteStartElement("Categories");
        //    xmlWriter.WriteAttributeString("Version", InvenioVersion.CurrentVersion);
        //    WriteCategories(xmlWriter, 0);
        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndDocument();
        //    xmlWriter.Close();
        //    return stringWriter.ToString();
        //}

        /// <summary>
        /// Export categories to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        //public virtual byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories)
        //{
        //    //property array
        //    var properties = new[]
        //    {
        //        new PropertyByName<Category>("Id", p => p.Id),
        //        new PropertyByName<Category>("Name", p => p.Name),
        //        new PropertyByName<Category>("Description", p => p.Description),
        //        new PropertyByName<Category>("CategoryTemplateId", p => p.CategoryTemplateId),
        //        new PropertyByName<Category>("MetaKeywords", p => p.MetaKeywords, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("MetaDescription", p => p.MetaDescription, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("MetaTitle", p => p.MetaTitle, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("SeName", p => p.GetSeName(0), IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("ParentCategoryId", p => p.ParentCategoryId),
        //        new PropertyByName<Category>("Picture", p => GetPictures(p.PictureId)),
        //        new PropertyByName<Category>("PageSize", p => p.PageSize, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("AllowUsersToSelectPageSize", p => p.AllowUsersToSelectPageSize, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("PageSizeOptions", p => p.PageSizeOptions, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("PriceRanges", p => p.PriceRanges, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("ShowOnHomePage", p => p.ShowOnHomePage, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("IncludeInTopMenu", p => p.IncludeInTopMenu, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("Published", p => p.Published, IgnoreExportCategoryProperty()),
        //        new PropertyByName<Category>("DisplayOrder", p => p.DisplayOrder)
        //    };
        //    return ExportToXlsx(properties, categories);
        //}

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        ///// <returns>Result in XML format</returns>
        //public virtual string ExportProductsToXml(IList<Product> products)
        //{
        //    var sb = new StringBuilder();
        //    var stringWriter = new StringWriter(sb);
        //    var xmlWriter = new XmlTextWriter(stringWriter);
        //    xmlWriter.WriteStartDocument();
        //    xmlWriter.WriteStartElement("Products");
        //    xmlWriter.WriteAttributeString("Version", InvenioVersion.CurrentVersion);

        //    foreach (var product in products)
        //    {
        //        xmlWriter.WriteStartElement("Product");

        //        xmlWriter.WriteString("ProductId", product.Id, IgnoreExportPoductProperty(p => p.Id));
        //        xmlWriter.WriteString("ProductTypeId", product.ProductTypeId, IgnoreExportPoductProperty(p => p.ProductType));
        //        xmlWriter.WriteString("ParentGroupedProductId", product.ParentGroupedProductId, IgnoreExportPoductProperty(p => p.ProductType));
        //        xmlWriter.WriteString("VisibleIndividually", product.VisibleIndividually, IgnoreExportPoductProperty(p => p.VisibleIndividually));
        //        xmlWriter.WriteString("Name", product.Name);
        //        xmlWriter.WriteString("ShortDescription", product.ShortDescription);
        //        xmlWriter.WriteString("FullDescription", product.FullDescription);
        //        xmlWriter.WriteString("AdminComment", product.AdminComment, IgnoreExportPoductProperty(p => p.AdminComment));
        //        //vendor can't change this field
        //        xmlWriter.WriteString("VendorId", product.VendorId, IgnoreExportPoductProperty(p => p.Vendor) || _workContext.CurrentVendor != null);
        //        xmlWriter.WriteString("ProductTemplateId", product.ProductTemplateId, IgnoreExportPoductProperty(p => p.ProductTemplate));
        //        xmlWriter.WriteString("ShowOnHomePage", product.ShowOnHomePage, IgnoreExportPoductProperty(p => p.ShowOnHomePage));
        //        xmlWriter.WriteString("MetaKeywords", product.MetaKeywords, IgnoreExportPoductProperty(p => p.Seo));
        //        xmlWriter.WriteString("MetaDescription", product.MetaDescription, IgnoreExportPoductProperty(p => p.Seo));
        //        xmlWriter.WriteString("MetaTitle", product.MetaTitle, IgnoreExportPoductProperty(p => p.Seo));
        //        xmlWriter.WriteString("SEName", product.GetSeName(0), IgnoreExportPoductProperty(p => p.Seo));
        //        xmlWriter.WriteString("AllowUserReviews", product.AllowUserReviews, IgnoreExportPoductProperty(p => p.AllowUserReviews));
        //        xmlWriter.WriteString("SKU", product.Sku);
        //        xmlWriter.WriteString("CustomerPartNumber", product.CustomerPartNumber, IgnoreExportPoductProperty(p => p.CustomerPartNumber));
        //        xmlWriter.WriteString("Gtin", product.Gtin, IgnoreExportPoductProperty(p => p.GTIN));
        //        xmlWriter.WriteString("IsGiftCard", product.IsGiftCard, IgnoreExportPoductProperty(p => p.IsGiftCard));
        //        xmlWriter.WriteString("GiftCardType", product.GiftCardType, IgnoreExportPoductProperty(p => p.IsGiftCard));
        //        xmlWriter.WriteString("OverriddenGiftCardAmount", product.OverriddenGiftCardAmount, IgnoreExportPoductProperty(p => p.IsGiftCard));
        //        xmlWriter.WriteString("RequireOtherProducts", product.RequireOtherProducts, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart));
        //        xmlWriter.WriteString("RequiredProductIds", product.RequiredProductIds, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart));
        //        xmlWriter.WriteString("AutomaticallyAddRequiredProducts", product.AutomaticallyAddRequiredProducts, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart));
        //        xmlWriter.WriteString("IsDownload", product.IsDownload, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("DownloadId", product.DownloadId, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("UnlimitedDownloads", product.UnlimitedDownloads, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("MaxNumberOfDownloads", product.MaxNumberOfDownloads, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("DownloadExpirationDays", product.DownloadExpirationDays, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("DownloadActivationType", product.DownloadActivationType, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("HasSampleDownload", product.HasSampleDownload, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("SampleDownloadId", product.SampleDownloadId, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("HasUserAgreement", product.HasUserAgreement, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("UserAgreementText", product.UserAgreementText, IgnoreExportPoductProperty(p => p.DownloadableProduct));
        //        xmlWriter.WriteString("IsRecurring", product.IsRecurring, IgnoreExportPoductProperty(p => p.RecurringProduct));
        //        xmlWriter.WriteString("RecurringCycleLength", product.RecurringCycleLength, IgnoreExportPoductProperty(p => p.RecurringProduct));
        //        xmlWriter.WriteString("RecurringCyclePeriodId", product.RecurringCyclePeriodId, IgnoreExportPoductProperty(p => p.RecurringProduct));
        //        xmlWriter.WriteString("RecurringTotalCycles", product.RecurringTotalCycles, IgnoreExportPoductProperty(p => p.RecurringProduct));
        //        xmlWriter.WriteString("IsRental", product.IsRental, IgnoreExportPoductProperty(p => p.IsRental));
        //        xmlWriter.WriteString("RentalPriceLength", product.RentalPriceLength, IgnoreExportPoductProperty(p => p.IsRental));
        //        xmlWriter.WriteString("RentalPricePeriodId", product.RentalPricePeriodId, IgnoreExportPoductProperty(p => p.IsRental));
        //        xmlWriter.WriteString("IsShipEnabled", product.IsShipEnabled);
        //        xmlWriter.WriteString("IsFreeShipping", product.IsFreeShipping, IgnoreExportPoductProperty(p => p.FreeShipping));
        //        xmlWriter.WriteString("ShipSeparately", product.ShipSeparately, IgnoreExportPoductProperty(p => p.ShipSeparately));
        //        xmlWriter.WriteString("AdditionalShippingCharge", product.AdditionalShippingCharge, IgnoreExportPoductProperty(p => p.AdditionalShippingCharge));
        //        xmlWriter.WriteString("DeliveryDateId", product.DeliveryDateId, IgnoreExportPoductProperty(p => p.DeliveryDate));
        //        xmlWriter.WriteString("IsTaxExempt", product.IsTaxExempt);
        //        xmlWriter.WriteString("TaxCategoryId", product.TaxCategoryId);
        //        xmlWriter.WriteString("IsTelecommunicationsOrBroadcastingOrElectronicServices", product.IsTelecommunicationsOrBroadcastingOrElectronicServices, IgnoreExportPoductProperty(p => p.TelecommunicationsBroadcastingElectronicServices));
        //        xmlWriter.WriteString("ManageInventoryMethodId", product.ManageInventoryMethodId);
        //        xmlWriter.WriteString("ProductAvailabilityRangeId", product.ProductAvailabilityRangeId, IgnoreExportPoductProperty(p => p.ProductAvailabilityRange));
        //        xmlWriter.WriteString("UseMultipleWarehouses", product.UseMultipleWarehouses, IgnoreExportPoductProperty(p => p.UseMultipleWarehouses));
        //        xmlWriter.WriteString("WarehouseId", product.WarehouseId, IgnoreExportPoductProperty(p => p.Warehouse));
        //        xmlWriter.WriteString("StockQuantity", product.StockQuantity);
        //        xmlWriter.WriteString("DisplayStockAvailability", product.DisplayStockAvailability, IgnoreExportPoductProperty(p => p.DisplayStockAvailability));
        //        xmlWriter.WriteString("DisplayStockQuantity", product.DisplayStockQuantity, IgnoreExportPoductProperty(p => p.DisplayStockQuantity));
        //        xmlWriter.WriteString("MinStockQuantity", product.MinStockQuantity, IgnoreExportPoductProperty(p => p.MinimumStockQuantity));
        //        xmlWriter.WriteString("LowStockActivityId", product.LowStockActivityId, IgnoreExportPoductProperty(p => p.LowStockActivity));
        //        xmlWriter.WriteString("NotifyAdminForQuantityBelow", product.NotifyAdminForQuantityBelow, IgnoreExportPoductProperty(p => p.NotifyAdminForQuantityBelow));
        //        xmlWriter.WriteString("BackorderModeId", product.BackorderModeId, IgnoreExportPoductProperty(p => p.Backorders));
        //        xmlWriter.WriteString("AllowBackInStockSubscriptions", product.AllowBackInStockSubscriptions, IgnoreExportPoductProperty(p => p.AllowBackInStockSubscriptions));
        //        xmlWriter.WriteString("OrderMinimumQuantity", product.OrderMinimumQuantity, IgnoreExportPoductProperty(p => p.MinimumCartQuantity));
        //        xmlWriter.WriteString("OrderMaximumQuantity", product.OrderMaximumQuantity, IgnoreExportPoductProperty(p => p.MaximumCartQuantity));
        //        xmlWriter.WriteString("AllowedQuantities", product.AllowedQuantities, IgnoreExportPoductProperty(p => p.AllowedQuantities));
        //        xmlWriter.WriteString("AllowAddingOnlyExistingAttributeCombinations", product.AllowAddingOnlyExistingAttributeCombinations, IgnoreExportPoductProperty(p => p.AllowAddingOnlyExistingAttributeCombinations));
        //        xmlWriter.WriteString("NotReturnable", product.NotReturnable, IgnoreExportPoductProperty(p => p.NotReturnable));
        //        xmlWriter.WriteString("DisableBuyButton", product.DisableBuyButton, IgnoreExportPoductProperty(p => p.DisableBuyButton));
        //        xmlWriter.WriteString("DisableWishlistButton", product.DisableWishlistButton, IgnoreExportPoductProperty(p => p.DisableWishlistButton));
        //        xmlWriter.WriteString("AvailableForPreOrder", product.AvailableForPreOrder, IgnoreExportPoductProperty(p => p.AvailableForPreOrder));
        //        xmlWriter.WriteString("PreOrderAvailabilityStartDateTimeUtc", product.PreOrderAvailabilityStartDateTimeUtc, IgnoreExportPoductProperty(p => p.AvailableForPreOrder));
        //        xmlWriter.WriteString("CallForPrice", product.CallForPrice, IgnoreExportPoductProperty(p => p.CallForPrice));
        //        xmlWriter.WriteString("Price", product.Price);
        //        xmlWriter.WriteString("OldPrice", product.OldPrice, IgnoreExportPoductProperty(p => p.OldPrice));
        //        xmlWriter.WriteString("ProductCost", product.ProductCost, IgnoreExportPoductProperty(p => p.ProductCost));
        //        xmlWriter.WriteString("UserEntersPrice", product.UserEntersPrice, IgnoreExportPoductProperty(p => p.UserEntersPrice));
        //        xmlWriter.WriteString("MinimumUserEnteredPrice", product.MinimumUserEnteredPrice, IgnoreExportPoductProperty(p => p.UserEntersPrice));
        //        xmlWriter.WriteString("MaximumUserEnteredPrice", product.MaximumUserEnteredPrice, IgnoreExportPoductProperty(p => p.UserEntersPrice));
        //        xmlWriter.WriteString("BasepriceEnabled", product.BasepriceEnabled, IgnoreExportPoductProperty(p => p.PAngV));
        //        xmlWriter.WriteString("BasepriceAmount", product.BasepriceAmount, IgnoreExportPoductProperty(p => p.PAngV));
        //        xmlWriter.WriteString("BasepriceUnitId", product.BasepriceUnitId, IgnoreExportPoductProperty(p => p.PAngV));
        //        xmlWriter.WriteString("BasepriceBaseAmount", product.BasepriceBaseAmount, IgnoreExportPoductProperty(p => p.PAngV));
        //        xmlWriter.WriteString("BasepriceBaseUnitId", product.BasepriceBaseUnitId, IgnoreExportPoductProperty(p => p.PAngV));
        //        xmlWriter.WriteString("MarkAsNew", product.MarkAsNew, IgnoreExportPoductProperty(p => p.MarkAsNew));
        //        xmlWriter.WriteString("MarkAsNewStartDateTimeUtc", product.MarkAsNewStartDateTimeUtc, IgnoreExportPoductProperty(p => p.MarkAsNewStartDate));
        //        xmlWriter.WriteString("MarkAsNewEndDateTimeUtc", product.MarkAsNewEndDateTimeUtc, IgnoreExportPoductProperty(p => p.MarkAsNewEndDate));
        //        xmlWriter.WriteString("Weight", product.Weight, IgnoreExportPoductProperty(p => p.Weight));
        //        xmlWriter.WriteString("Length", product.Length, IgnoreExportPoductProperty(p => p.Dimensions));
        //        xmlWriter.WriteString("Width", product.Width, IgnoreExportPoductProperty(p => p.Dimensions));
        //        xmlWriter.WriteString("Height", product.Height, IgnoreExportPoductProperty(p => p.Dimensions));
        //        xmlWriter.WriteString("Published", product.Published, IgnoreExportPoductProperty(p => p.Published));
        //        xmlWriter.WriteString("CreatedOnUtc", product.CreatedOnUtc, IgnoreExportPoductProperty(p => p.CreatedOn));
        //        xmlWriter.WriteString("UpdatedOnUtc", product.UpdatedOnUtc, IgnoreExportPoductProperty(p => p.UpdatedOn));

        //        if (!IgnoreExportPoductProperty(p => p.Discounts))
        //        {
        //            xmlWriter.WriteStartElement("ProductDiscounts");
        //            var discounts = product.AppliedDiscounts;
        //            foreach (var discount in discounts)
        //            {
        //                xmlWriter.WriteStartElement("Discount");
        //                xmlWriter.WriteString("DiscountId", discount.Id);
        //                xmlWriter.WriteString("Name", discount.Name);
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();
        //        }

        //        if (!IgnoreExportPoductProperty(p => p.TierPrices))
        //        {
        //            xmlWriter.WriteStartElement("TierPrices");
        //            var tierPrices = product.TierPrices;
        //            foreach (var tierPrice in tierPrices)
        //            {
        //                xmlWriter.WriteStartElement("TierPrice");
        //                xmlWriter.WriteString("TierPriceId", tierPrice.Id);
        //                xmlWriter.WriteString("StoreId", tierPrice.StoreId);
        //                xmlWriter.WriteString("UserRoleId", tierPrice.UserRoleId, defaulValue: "0");
        //                xmlWriter.WriteString("Quantity", tierPrice.Quantity);
        //                xmlWriter.WriteString("Price", tierPrice.Price);
        //                xmlWriter.WriteString("StartDateTimeUtc", tierPrice.StartDateTimeUtc);
        //                xmlWriter.WriteString("EndDateTimeUtc", tierPrice.EndDateTimeUtc);
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();
        //        }

        //        if (!IgnoreExportPoductProperty(p => p.ProductAttributes))
        //        {
        //            xmlWriter.WriteStartElement("ProductAttributes");
        //            var productAttributMappings =
        //                _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
        //            foreach (var productAttributeMapping in productAttributMappings)
        //            {
        //                xmlWriter.WriteStartElement("ProductAttributeMapping");
        //                xmlWriter.WriteString("ProductAttributeMappingId", productAttributeMapping.Id);
        //                xmlWriter.WriteString("ProductAttributeId", productAttributeMapping.ProductAttributeId);
        //                xmlWriter.WriteString("ProductAttributeName", productAttributeMapping.ProductAttribute.Name);
        //                xmlWriter.WriteString("TextPrompt", productAttributeMapping.TextPrompt);
        //                xmlWriter.WriteString("IsRequired", productAttributeMapping.IsRequired);
        //                xmlWriter.WriteString("AttributeControlTypeId", productAttributeMapping.AttributeControlTypeId);
        //                xmlWriter.WriteString("DisplayOrder", productAttributeMapping.DisplayOrder);
        //                //validation rules
        //                if (productAttributeMapping.ValidationRulesAllowed())
        //                {
        //                    if (productAttributeMapping.ValidationMinLength.HasValue)
        //                    {
        //                        xmlWriter.WriteString("ValidationMinLength",
        //                            productAttributeMapping.ValidationMinLength.Value);
        //                    }
        //                    if (productAttributeMapping.ValidationMaxLength.HasValue)
        //                    {
        //                        xmlWriter.WriteString("ValidationMaxLength",
        //                            productAttributeMapping.ValidationMaxLength.Value);
        //                    }
        //                    if (String.IsNullOrEmpty(productAttributeMapping.ValidationFileAllowedExtensions))
        //                    {
        //                        xmlWriter.WriteString("ValidationFileAllowedExtensions",
        //                            productAttributeMapping.ValidationFileAllowedExtensions);
        //                    }
        //                    if (productAttributeMapping.ValidationFileMaximumSize.HasValue)
        //                    {
        //                        xmlWriter.WriteString("ValidationFileMaximumSize",
        //                            productAttributeMapping.ValidationFileMaximumSize.Value);
        //                    }
        //                    xmlWriter.WriteString("DefaultValue", productAttributeMapping.DefaultValue);
        //                }
        //                //conditions
        //                xmlWriter.WriteElementString("ConditionAttributeXml",
        //                    productAttributeMapping.ConditionAttributeXml);

        //                xmlWriter.WriteStartElement("ProductAttributeValues");
        //                var productAttributeValues = productAttributeMapping.ProductAttributeValues;
        //                foreach (var productAttributeValue in productAttributeValues)
        //                {
        //                    xmlWriter.WriteStartElement("ProductAttributeValue");
        //                    xmlWriter.WriteString("ProductAttributeValueId", productAttributeValue.Id);
        //                    xmlWriter.WriteString("Name", productAttributeValue.Name);
        //                    xmlWriter.WriteString("AttributeValueTypeId", productAttributeValue.AttributeValueTypeId);
        //                    xmlWriter.WriteString("AssociatedProductId", productAttributeValue.AssociatedProductId);
        //                    xmlWriter.WriteString("ColorSquaresRgb", productAttributeValue.ColorSquaresRgb);
        //                    xmlWriter.WriteString("ImageSquaresPictureId", productAttributeValue.ImageSquaresPictureId);
        //                    xmlWriter.WriteString("PriceAdjustment", productAttributeValue.PriceAdjustment);
        //                    xmlWriter.WriteString("WeightAdjustment", productAttributeValue.WeightAdjustment);
        //                    xmlWriter.WriteString("Cost", productAttributeValue.Cost);
        //                    xmlWriter.WriteString("UserEntersQty", productAttributeValue.UserEntersQty);
        //                    xmlWriter.WriteString("Quantity", productAttributeValue.Quantity);
        //                    xmlWriter.WriteString("IsPreSelected", productAttributeValue.IsPreSelected);
        //                    xmlWriter.WriteString("DisplayOrder", productAttributeValue.DisplayOrder);
        //                    xmlWriter.WriteString("PictureId", productAttributeValue.PictureId);
        //                    xmlWriter.WriteEndElement();
        //                }
        //                xmlWriter.WriteEndElement();

        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();
        //        }
        //        xmlWriter.WriteStartElement("ProductPictures");
        //        var productPictures = product.ProductPictures;
        //        foreach (var productPicture in productPictures)
        //        {
        //            xmlWriter.WriteStartElement("ProductPicture");
        //            xmlWriter.WriteString("ProductPictureId", productPicture.Id);
        //            xmlWriter.WriteString("PictureId", productPicture.PictureId);
        //            xmlWriter.WriteString("DisplayOrder", productPicture.DisplayOrder);
        //            xmlWriter.WriteEndElement();
        //        }
        //        xmlWriter.WriteEndElement();

        //        xmlWriter.WriteStartElement("ProductCategories");
        //        var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
        //        if (productCategories != null)
        //        {
        //            foreach (var productCategory in productCategories)
        //            {
        //                xmlWriter.WriteStartElement("ProductCategory");
        //                xmlWriter.WriteString("ProductCategoryId", productCategory.Id);
        //                xmlWriter.WriteString("CategoryId", productCategory.CategoryId);
        //                xmlWriter.WriteString("IsFeaturedProduct", productCategory.IsFeaturedProduct);
        //                xmlWriter.WriteString("DisplayOrder", productCategory.DisplayOrder);
        //                xmlWriter.WriteEndElement();
        //            }
        //        }
        //        xmlWriter.WriteEndElement();

        //        if (!IgnoreExportPoductProperty(p => p.Customers))
        //        {
        //            xmlWriter.WriteStartElement("ProductCustomers");
        //            var productCustomers = _CustomerService.GetProductCustomersByProductId(product.Id);
        //            if (productCustomers != null)
        //            {
        //                foreach (var productCustomer in productCustomers)
        //                {
        //                    xmlWriter.WriteStartElement("ProductCustomer");
        //                    xmlWriter.WriteString("ProductCustomerId", productCustomer.Id);
        //                    xmlWriter.WriteString("CustomerId", productCustomer.CustomerId);
        //                    xmlWriter.WriteString("IsFeaturedProduct", productCustomer.IsFeaturedProduct);
        //                    xmlWriter.WriteString("DisplayOrder", productCustomer.DisplayOrder);
        //                    xmlWriter.WriteEndElement();
        //                }
        //            }
        //            xmlWriter.WriteEndElement();
        //        }

        //        if (!IgnoreExportPoductProperty(p => p.SpecificationAttributes))
        //        {
        //            xmlWriter.WriteStartElement("ProductSpecificationAttributes");
        //            var productSpecificationAttributes = product.ProductSpecificationAttributes;
        //            foreach (var productSpecificationAttribute in productSpecificationAttributes)
        //            {
        //                xmlWriter.WriteStartElement("ProductSpecificationAttribute");
        //                xmlWriter.WriteString("ProductSpecificationAttributeId", productSpecificationAttribute.Id);
        //                xmlWriter.WriteString("SpecificationAttributeOptionId", productSpecificationAttribute.SpecificationAttributeOptionId);
        //                xmlWriter.WriteString("CustomValue", productSpecificationAttribute.CustomValue);
        //                xmlWriter.WriteString("AllowFiltering", productSpecificationAttribute.AllowFiltering);
        //                xmlWriter.WriteString("ShowOnProductPage", productSpecificationAttribute.ShowOnProductPage);
        //                xmlWriter.WriteString("DisplayOrder", productSpecificationAttribute.DisplayOrder);
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();
        //        }

        //        if (!IgnoreExportPoductProperty(p => p.ProductTags))
        //        {
        //            xmlWriter.WriteStartElement("ProductTags");
        //            var productTags = product.ProductTags;
        //            foreach (var productTag in productTags)
        //            {
        //                xmlWriter.WriteStartElement("ProductTag");
        //                xmlWriter.WriteString("Id", productTag.Id);
        //                xmlWriter.WriteString("Name", productTag.Name);
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();
        //        }

        //        xmlWriter.WriteEndElement();
        //    }

        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndDocument();
        //    xmlWriter.Close();
        //    return stringWriter.ToString();
        //}

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        ///// <param name="products">Products</param>
        //public virtual byte[] ExportProductsToXlsx(IEnumerable<Product> products)
        //{
        //    var properties = new[]
        //    {
        //        new PropertyByName<Product>("ProductType", p => p.ProductTypeId, IgnoreExportPoductProperty(p => p.ProductType))
        //        {
        //            DropDownElements = ProductType.SimpleProduct.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<Product>("ParentGroupedProductId", p => p.ParentGroupedProductId, IgnoreExportPoductProperty(p => p.ProductType)),
        //        new PropertyByName<Product>("VisibleIndividually", p => p.VisibleIndividually, IgnoreExportPoductProperty(p => p.VisibleIndividually)),
        //        new PropertyByName<Product>("Name", p => p.Name),
        //        new PropertyByName<Product>("ShortDescription", p => p.ShortDescription),
        //        new PropertyByName<Product>("FullDescription", p => p.FullDescription),
        //        //vendor can't change this field
        //        new PropertyByName<Product>("Vendor", p => p.VendorId, IgnoreExportPoductProperty(p => p.Vendor) || _workContext.CurrentVendor != null)
        //        {
        //            DropDownElements = _vendorService.GetAllVendors(showHidden: true).Select(v => v as BaseEntity).ToSelectList(p => (p as Vendor).Return(v => v.Name, String.Empty)),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("ProductTemplate", p => p.ProductTemplateId, IgnoreExportPoductProperty(p => p.ProductTemplate))
        //        {
        //            DropDownElements = _productTemplateService.GetAllProductTemplates().Select(pt => pt as BaseEntity).ToSelectList(p => (p as ProductTemplate).Return(pt => pt.Name, String.Empty)),
        //        },
        //        //vendor can't change this field
        //        new PropertyByName<Product>("ShowOnHomePage", p => p.ShowOnHomePage, IgnoreExportPoductProperty(p => p.ShowOnHomePage) || _workContext.CurrentVendor != null),
        //        new PropertyByName<Product>("MetaKeywords", p => p.MetaKeywords, IgnoreExportPoductProperty(p => p.Seo)),
        //        new PropertyByName<Product>("MetaDescription", p => p.MetaDescription, IgnoreExportPoductProperty(p => p.Seo)),
        //        new PropertyByName<Product>("MetaTitle", p => p.MetaTitle, IgnoreExportPoductProperty(p => p.Seo)),
        //        new PropertyByName<Product>("SeName", p => p.GetSeName(0), IgnoreExportPoductProperty(p => p.Seo)),
        //        new PropertyByName<Product>("AllowUserReviews", p => p.AllowUserReviews, IgnoreExportPoductProperty(p => p.AllowUserReviews)),
        //        new PropertyByName<Product>("Published", p => p.Published, IgnoreExportPoductProperty(p => p.Published)),
        //        new PropertyByName<Product>("SKU", p => p.Sku),
        //        new PropertyByName<Product>("CustomerPartNumber", p => p.CustomerPartNumber, IgnoreExportPoductProperty(p => p.CustomerPartNumber)),
        //        new PropertyByName<Product>("Gtin", p => p.Gtin, IgnoreExportPoductProperty(p => p.GTIN)),
        //        new PropertyByName<Product>("IsGiftCard", p => p.IsGiftCard, IgnoreExportPoductProperty(p => p.IsGiftCard)),
        //        new PropertyByName<Product>("GiftCardType", p => p.GiftCardTypeId, IgnoreExportPoductProperty(p => p.IsGiftCard))
        //        {
        //            DropDownElements = GiftCardType.Virtual.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<Product>("OverriddenGiftCardAmount", p => p.OverriddenGiftCardAmount, IgnoreExportPoductProperty(p => p.IsGiftCard)),
        //        new PropertyByName<Product>("RequireOtherProducts", p => p.RequireOtherProducts, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart)),
        //        new PropertyByName<Product>("RequiredProductIds", p => p.RequiredProductIds, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart)),
        //        new PropertyByName<Product>("AutomaticallyAddRequiredProducts", p => p.AutomaticallyAddRequiredProducts, IgnoreExportPoductProperty(p => p.RequireOtherProductsAddedToTheCart)),
        //        new PropertyByName<Product>("IsDownload", p => p.IsDownload, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("DownloadId", p => p.DownloadId, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("UnlimitedDownloads", p => p.UnlimitedDownloads, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("MaxNumberOfDownloads", p => p.MaxNumberOfDownloads, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("DownloadActivationType", p => p.DownloadActivationTypeId, IgnoreExportPoductProperty(p => p.DownloadableProduct))
        //        {
        //            DropDownElements = DownloadActivationType.Manually.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<Product>("HasSampleDownload", p => p.HasSampleDownload, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("SampleDownloadId", p => p.SampleDownloadId, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("HasUserAgreement", p => p.HasUserAgreement, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("UserAgreementText", p => p.UserAgreementText, IgnoreExportPoductProperty(p => p.DownloadableProduct)),
        //        new PropertyByName<Product>("IsRecurring", p => p.IsRecurring, IgnoreExportPoductProperty(p => p.RecurringProduct)),
        //        new PropertyByName<Product>("RecurringCycleLength", p => p.RecurringCycleLength, IgnoreExportPoductProperty(p => p.RecurringProduct)),
        //        new PropertyByName<Product>("RecurringCyclePeriod", p => p.RecurringCyclePeriodId, IgnoreExportPoductProperty(p => p.RecurringProduct))
        //        {
        //            DropDownElements = RecurringProductCyclePeriod.Days.ToSelectList(useLocalization: false),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("RecurringTotalCycles", p => p.RecurringTotalCycles, IgnoreExportPoductProperty(p => p.RecurringProduct)),
        //        new PropertyByName<Product>("IsRental", p => p.IsRental, IgnoreExportPoductProperty(p => p.IsRental)),
        //        new PropertyByName<Product>("RentalPriceLength", p => p.RentalPriceLength, IgnoreExportPoductProperty(p => p.IsRental)),
        //        new PropertyByName<Product>("RentalPricePeriod", p => p.RentalPricePeriodId, IgnoreExportPoductProperty(p => p.IsRental))
        //        {
        //            DropDownElements = RentalPricePeriod.Days.ToSelectList(useLocalization: false),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("IsShipEnabled", p => p.IsShipEnabled),
        //        new PropertyByName<Product>("IsFreeShipping", p => p.IsFreeShipping, IgnoreExportPoductProperty(p => p.FreeShipping)),
        //        new PropertyByName<Product>("ShipSeparately", p => p.ShipSeparately, IgnoreExportPoductProperty(p => p.ShipSeparately)),
        //        new PropertyByName<Product>("AdditionalShippingCharge", p => p.AdditionalShippingCharge, IgnoreExportPoductProperty(p => p.AdditionalShippingCharge)),
        //        new PropertyByName<Product>("DeliveryDate", p => p.DeliveryDateId, IgnoreExportPoductProperty(p => p.DeliveryDate))
        //        {
        //            DropDownElements = _dateRangeService.GetAllDeliveryDates().Select(dd => dd as BaseEntity).ToSelectList(p => (p as DeliveryDate).Return(dd => dd.Name, String.Empty)),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("IsTaxExempt", p => p.IsTaxExempt),
        //        new PropertyByName<Product>("TaxCategory", p => p.TaxCategoryId)
        //        {
        //            DropDownElements = _taxCategoryService.GetAllTaxCategories().Select(tc => tc as BaseEntity).ToSelectList(p => (p as TaxCategory).Return(tc => tc.Name, String.Empty)),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("IsTelecommunicationsOrBroadcastingOrElectronicServices", p => p.IsTelecommunicationsOrBroadcastingOrElectronicServices, IgnoreExportPoductProperty(p => p.TelecommunicationsBroadcastingElectronicServices)),
        //        new PropertyByName<Product>("ManageInventoryMethod", p => p.ManageInventoryMethodId)
        //        {
        //            DropDownElements = ManageInventoryMethod.DontManageStock.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<Product>("ProductAvailabilityRange", p => p.ProductAvailabilityRangeId, IgnoreExportPoductProperty(p => p.ProductAvailabilityRange))
        //        {
        //            DropDownElements = _dateRangeService.GetAllProductAvailabilityRanges().Select(range => range as BaseEntity).ToSelectList(p => (p as ProductAvailabilityRange).Return(range => range.Name, String.Empty)),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("UseMultipleWarehouses", p => p.UseMultipleWarehouses, IgnoreExportPoductProperty(p => p.UseMultipleWarehouses)),
        //        new PropertyByName<Product>("WarehouseId", p => p.WarehouseId, IgnoreExportPoductProperty(p => p.Warehouse)),
        //        new PropertyByName<Product>("StockQuantity", p => p.StockQuantity),
        //        new PropertyByName<Product>("DisplayStockAvailability", p => p.DisplayStockAvailability, IgnoreExportPoductProperty(p => p.DisplayStockAvailability)),
        //        new PropertyByName<Product>("DisplayStockQuantity", p => p.DisplayStockQuantity, IgnoreExportPoductProperty(p => p.DisplayStockQuantity)),
        //        new PropertyByName<Product>("MinStockQuantity", p => p.MinStockQuantity, IgnoreExportPoductProperty(p => p.MinimumStockQuantity)),
        //        new PropertyByName<Product>("LowStockActivity", p => p.LowStockActivityId, IgnoreExportPoductProperty(p => p.LowStockActivity))
        //        {
        //            DropDownElements = LowStockActivity.Nothing.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<Product>("NotifyAdminForQuantityBelow", p => p.NotifyAdminForQuantityBelow, IgnoreExportPoductProperty(p => p.NotifyAdminForQuantityBelow)),
        //        new PropertyByName<Product>("BackorderMode", p => p.BackorderModeId, IgnoreExportPoductProperty(p => p.Backorders))
        //        {
        //            DropDownElements = BackorderMode.NoBackorders.ToSelectList(useLocalization: false)
        //        },
        //        new PropertyByName<Product>("AllowBackInStockSubscriptions", p => p.AllowBackInStockSubscriptions, IgnoreExportPoductProperty(p => p.AllowBackInStockSubscriptions)),
        //        new PropertyByName<Product>("OrderMinimumQuantity", p => p.OrderMinimumQuantity, IgnoreExportPoductProperty(p => p.MinimumCartQuantity)),
        //        new PropertyByName<Product>("OrderMaximumQuantity", p => p.OrderMaximumQuantity, IgnoreExportPoductProperty(p => p.MaximumCartQuantity)),
        //        new PropertyByName<Product>("AllowedQuantities", p => p.AllowedQuantities, IgnoreExportPoductProperty(p => p.AllowedQuantities)),
        //        new PropertyByName<Product>("AllowAddingOnlyExistingAttributeCombinations", p => p.AllowAddingOnlyExistingAttributeCombinations, IgnoreExportPoductProperty(p => p.AllowAddingOnlyExistingAttributeCombinations)),
        //        new PropertyByName<Product>("NotReturnable", p => p.NotReturnable, IgnoreExportPoductProperty(p => p.NotReturnable)),
        //        new PropertyByName<Product>("DisableBuyButton", p => p.DisableBuyButton, IgnoreExportPoductProperty(p => p.DisableBuyButton)),
        //        new PropertyByName<Product>("DisableWishlistButton", p => p.DisableWishlistButton, IgnoreExportPoductProperty(p => p.DisableWishlistButton)),
        //        new PropertyByName<Product>("AvailableForPreOrder", p => p.AvailableForPreOrder, IgnoreExportPoductProperty(p => p.AvailableForPreOrder)),
        //        new PropertyByName<Product>("PreOrderAvailabilityStartDateTimeUtc", p => p.PreOrderAvailabilityStartDateTimeUtc, IgnoreExportPoductProperty(p => p.AvailableForPreOrder)),
        //        new PropertyByName<Product>("CallForPrice", p => p.CallForPrice, IgnoreExportPoductProperty(p => p.CallForPrice)),
        //        new PropertyByName<Product>("Price", p => p.Price),
        //        new PropertyByName<Product>("OldPrice", p => p.OldPrice, IgnoreExportPoductProperty(p => p.OldPrice)),
        //        new PropertyByName<Product>("ProductCost", p => p.ProductCost, IgnoreExportPoductProperty(p => p.ProductCost)),
        //        new PropertyByName<Product>("UserEntersPrice", p => p.UserEntersPrice, IgnoreExportPoductProperty(p => p.UserEntersPrice)),
        //        new PropertyByName<Product>("MinimumUserEnteredPrice", p => p.MinimumUserEnteredPrice, IgnoreExportPoductProperty(p => p.UserEntersPrice)),
        //        new PropertyByName<Product>("MaximumUserEnteredPrice", p => p.MaximumUserEnteredPrice, IgnoreExportPoductProperty(p => p.UserEntersPrice)),
        //        new PropertyByName<Product>("BasepriceEnabled", p => p.BasepriceEnabled, IgnoreExportPoductProperty(p => p.PAngV)),
        //        new PropertyByName<Product>("BasepriceAmount", p => p.BasepriceAmount, IgnoreExportPoductProperty(p => p.PAngV)),
        //        new PropertyByName<Product>("BasepriceUnit", p => p.BasepriceUnitId, IgnoreExportPoductProperty(p => p.PAngV))
        //        {
        //            DropDownElements = _measureService.GetAllMeasureWeights().Select(mw => mw as BaseEntity).ToSelectList(p => (p as MeasureWeight).Return(mw => mw.Name, String.Empty)),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("BasepriceBaseAmount", p => p.BasepriceBaseAmount, IgnoreExportPoductProperty(p => p.PAngV)),
        //        new PropertyByName<Product>("BasepriceBaseUnit", p => p.BasepriceBaseUnitId, IgnoreExportPoductProperty(p => p.PAngV))
        //        {
        //            DropDownElements = _measureService.GetAllMeasureWeights().Select(mw => mw as BaseEntity).ToSelectList(p => (p as MeasureWeight).Return(mw => mw.Name, String.Empty)),
        //            AllowBlank = true
        //        },
        //        new PropertyByName<Product>("MarkAsNew", p => p.MarkAsNew, IgnoreExportPoductProperty(p => p.MarkAsNew)),
        //        new PropertyByName<Product>("MarkAsNewStartDateTimeUtc", p => p.MarkAsNewStartDateTimeUtc, IgnoreExportPoductProperty(p => p.MarkAsNewStartDate)),
        //        new PropertyByName<Product>("MarkAsNewEndDateTimeUtc", p => p.MarkAsNewEndDateTimeUtc, IgnoreExportPoductProperty(p => p.MarkAsNewEndDate)),
        //        new PropertyByName<Product>("Weight", p => p.Weight, IgnoreExportPoductProperty(p => p.Weight)),
        //        new PropertyByName<Product>("Length", p => p.Length, IgnoreExportPoductProperty(p => p.Dimensions)),
        //        new PropertyByName<Product>("Width", p => p.Width, IgnoreExportPoductProperty(p => p.Dimensions)),
        //        new PropertyByName<Product>("Height", p => p.Height, IgnoreExportPoductProperty(p => p.Dimensions)),
        //        new PropertyByName<Product>("Categories", GetCategories),
        //        new PropertyByName<Product>("Customers", GetCustomers, IgnoreExportPoductProperty(p => p.Customers)),
        //        new PropertyByName<Product>("ProductTags", GetProductTags, IgnoreExportPoductProperty(p => p.ProductTags)),
        //        new PropertyByName<Product>("Picture1", p => GetPictures(p)[0]),
        //        new PropertyByName<Product>("Picture2", p => GetPictures(p)[1]),
        //        new PropertyByName<Product>("Picture3", p => GetPictures(p)[2])
        //    };

        //    var productList = products.ToList();
        //    var productAdvancedMode = _workContext.CurrentUser.GetAttribute<bool>("product-advanced-mode");

        //    if (_catalogSettings.ExportImportProductAttributes)
        //    {
        //        if (productAdvancedMode || _productEditorSettings.ProductAttributes)
        //            return ExportProductsToXlsxWithAttributes(properties, productList);
        //    }

        //    return ExportToXlsx(properties, productList);
        //}

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        //public virtual string ExportOrdersToXml(IList<Order> orders)
        //{
        //    //a vendor should have access only to part of order information
        //    var ignore = _workContext.CurrentVendor != null;

        //    var sb = new StringBuilder();
        //    var stringWriter = new StringWriter(sb);
        //    var xmlWriter = new XmlTextWriter(stringWriter);
        //    xmlWriter.WriteStartDocument();
        //    xmlWriter.WriteStartElement("Orders");
        //    xmlWriter.WriteAttributeString("Version", InvenioVersion.CurrentVersion);

        //    foreach (var order in orders)
        //    {
        //        xmlWriter.WriteStartElement("Order");

        //        xmlWriter.WriteString("OrderId", order.Id);
        //        xmlWriter.WriteString("OrderGuid", order.OrderGuid, ignore);
        //        xmlWriter.WriteString("StoreId", order.StoreId);
        //        xmlWriter.WriteString("UserId", order.UserId, ignore);
        //        xmlWriter.WriteString("OrderStatusId", order.OrderStatusId, ignore);
        //        xmlWriter.WriteString("PaymentStatusId", order.PaymentStatusId, ignore);
        //        xmlWriter.WriteString("ShippingStatusId", order.ShippingStatusId, ignore);
        //        xmlWriter.WriteString("UserLanguageId", order.UserLanguageId, ignore);
        //        xmlWriter.WriteString("UserTaxDisplayTypeId", order.UserTaxDisplayTypeId, ignore);
        //        xmlWriter.WriteString("UserIp", order.UserIp, ignore);
        //        xmlWriter.WriteString("OrderSubtotalInclTax", order.OrderSubtotalInclTax, ignore);
        //        xmlWriter.WriteString("OrderSubtotalExclTax", order.OrderSubtotalExclTax, ignore);
        //        xmlWriter.WriteString("OrderSubTotalDiscountInclTax", order.OrderSubTotalDiscountInclTax, ignore);
        //        xmlWriter.WriteString("OrderSubTotalDiscountExclTax", order.OrderSubTotalDiscountExclTax, ignore);
        //        xmlWriter.WriteString("OrderShippingInclTax", order.OrderShippingInclTax, ignore);
        //        xmlWriter.WriteString("OrderShippingExclTax", order.OrderShippingExclTax, ignore);
        //        xmlWriter.WriteString("PaymentMethodAdditionalFeeInclTax", order.PaymentMethodAdditionalFeeInclTax, ignore);
        //        xmlWriter.WriteString("PaymentMethodAdditionalFeeExclTax", order.PaymentMethodAdditionalFeeExclTax, ignore);
        //        xmlWriter.WriteString("TaxRates", order.TaxRates, ignore);
        //        xmlWriter.WriteString("OrderTax", order.OrderTax, ignore);
        //        xmlWriter.WriteString("OrderTotal", order.OrderTotal, ignore);
        //        xmlWriter.WriteString("RefundedAmount", order.RefundedAmount, ignore);
        //        xmlWriter.WriteString("OrderDiscount", order.OrderDiscount, ignore);
        //        xmlWriter.WriteString("CurrencyRate", order.CurrencyRate);
        //        xmlWriter.WriteString("UserCurrencyCode", order.UserCurrencyCode);
        //        xmlWriter.WriteString("AffiliateId", order.AffiliateId, ignore);
        //        xmlWriter.WriteString("AllowStoringCreditCardNumber", order.AllowStoringCreditCardNumber, ignore);
        //        xmlWriter.WriteString("CardType", order.CardType, ignore);
        //        xmlWriter.WriteString("CardName", order.CardName, ignore);
        //        xmlWriter.WriteString("CardNumber", order.CardNumber, ignore);
        //        xmlWriter.WriteString("MaskedCreditCardNumber", order.MaskedCreditCardNumber, ignore);
        //        xmlWriter.WriteString("CardCvv2", order.CardCvv2, ignore);
        //        xmlWriter.WriteString("CardExpirationMonth", order.CardExpirationMonth, ignore);
        //        xmlWriter.WriteString("CardExpirationYear", order.CardExpirationYear, ignore);
        //        xmlWriter.WriteString("PaymentMethodSystemName", order.PaymentMethodSystemName, ignore);
        //        xmlWriter.WriteString("AuthorizationTransactionId", order.AuthorizationTransactionId, ignore);
        //        xmlWriter.WriteString("AuthorizationTransactionCode", order.AuthorizationTransactionCode, ignore);
        //        xmlWriter.WriteString("AuthorizationTransactionResult", order.AuthorizationTransactionResult, ignore);
        //        xmlWriter.WriteString("CaptureTransactionId", order.CaptureTransactionId, ignore);
        //        xmlWriter.WriteString("CaptureTransactionResult", order.CaptureTransactionResult, ignore);
        //        xmlWriter.WriteString("SubscriptionTransactionId", order.SubscriptionTransactionId, ignore);
        //        xmlWriter.WriteString("PaidDateUtc", order.PaidDateUtc == null ? string.Empty : order.PaidDateUtc.Value.ToString(), ignore);
        //        xmlWriter.WriteString("ShippingMethod", order.ShippingMethod);
        //        xmlWriter.WriteString("ShippingRateComputationMethodSystemName", order.ShippingRateComputationMethodSystemName, ignore);
        //        xmlWriter.WriteString("CustomValuesXml", order.CustomValuesXml, ignore);
        //        xmlWriter.WriteString("VatNumber", order.VatNumber, ignore);
        //        xmlWriter.WriteString("Deleted", order.Deleted, ignore);
        //        xmlWriter.WriteString("CreatedOnUtc", order.CreatedOnUtc);

        //        if (_orderSettings.ExportWithProducts)
        //        {
        //            //products
        //            var orderItems = order.OrderItems;

        //            //a vendor should have access only to his products
        //            if (_workContext.CurrentVendor != null)
        //                orderItems = orderItems.Where(oi => oi.Product.VendorId == _workContext.CurrentVendor.Id).ToList();

        //            if (orderItems.Any())
        //            {
        //                xmlWriter.WriteStartElement("OrderItems");
        //                foreach (var orderItem in orderItems)
        //                {
        //                    xmlWriter.WriteStartElement("OrderItem");
        //                    xmlWriter.WriteString("Id", orderItem.Id);
        //                    xmlWriter.WriteString("OrderItemGuid", orderItem.OrderItemGuid);
        //                    xmlWriter.WriteString("Name", orderItem.Product.Name);
        //                    xmlWriter.WriteString("Sku", orderItem.Product.Sku);
        //                    xmlWriter.WriteString("PriceExclTax", orderItem.UnitPriceExclTax);
        //                    xmlWriter.WriteString("PriceInclTax", orderItem.UnitPriceInclTax);
        //                    xmlWriter.WriteString("Quantity", orderItem.Quantity);
        //                    xmlWriter.WriteString("DiscountExclTax", orderItem.DiscountAmountExclTax);
        //                    xmlWriter.WriteString("DiscountInclTax", orderItem.DiscountAmountInclTax);
        //                    xmlWriter.WriteString("TotalExclTax", orderItem.PriceExclTax);
        //                    xmlWriter.WriteString("TotalInclTax", orderItem.PriceInclTax);
        //                    xmlWriter.WriteEndElement();
        //                }
        //                xmlWriter.WriteEndElement();
        //            }
        //        }

        //        //shipments
        //        var shipments = order.Shipments.OrderBy(x => x.CreatedOnUtc).ToList();
        //        if (shipments.Any())
        //        {
        //            xmlWriter.WriteStartElement("Shipments");
        //            foreach (var shipment in shipments)
        //            {
        //                xmlWriter.WriteStartElement("Shipment");
        //                xmlWriter.WriteElementString("ShipmentId", null, shipment.Id.ToString());
        //                xmlWriter.WriteElementString("TrackingNumber", null, shipment.TrackingNumber);
        //                xmlWriter.WriteElementString("TotalWeight", null, shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value.ToString() : String.Empty);

        //                xmlWriter.WriteElementString("ShippedDateUtc", null, shipment.ShippedDateUtc.HasValue ? 
        //                    shipment.ShippedDateUtc.ToString() : String.Empty);
        //                xmlWriter.WriteElementString("DeliveryDateUtc", null, shipment.DeliveryDateUtc.HasValue ? 
        //                    shipment.DeliveryDateUtc.Value.ToString() : String.Empty);
        //                xmlWriter.WriteElementString("CreatedOnUtc", null, shipment.CreatedOnUtc.ToString());
        //                xmlWriter.WriteEndElement();
        //            }
        //            xmlWriter.WriteEndElement();
        //        }
        //        xmlWriter.WriteEndElement();
        //    }

        //    xmlWriter.WriteEndElement();
        //    xmlWriter.WriteEndDocument();
        //    xmlWriter.Close();
        //    return stringWriter.ToString();
        //}

        ///// <summary>
        ///// Export orders to XLSX
        ///// </summary>
        ///// <param name="orders">Orders</param>
        //public virtual byte[] ExportOrdersToXlsx(IList<Order> orders)
        //{
        //    //a vendor should have access only to part of order information
        //    var ignore = _workContext.CurrentVendor != null;

        //    //property array
        //    var properties = new[]
        //    {
        //        new PropertyByName<Order>("OrderId", p => p.Id),
        //        new PropertyByName<Order>("StoreId", p => p.StoreId),
        //        new PropertyByName<Order>("OrderGuid", p => p.OrderGuid, ignore),
        //        new PropertyByName<Order>("UserId", p => p.UserId, ignore),
        //        new PropertyByName<Order>("OrderStatusId", p => p.OrderStatusId, ignore),
        //        new PropertyByName<Order>("PaymentStatusId", p => p.PaymentStatusId),
        //        new PropertyByName<Order>("ShippingStatusId", p => p.ShippingStatusId, ignore),
        //        new PropertyByName<Order>("OrderSubtotalInclTax", p => p.OrderSubtotalInclTax, ignore),
        //        new PropertyByName<Order>("OrderSubtotalExclTax", p => p.OrderSubtotalExclTax, ignore),
        //        new PropertyByName<Order>("OrderSubTotalDiscountInclTax", p => p.OrderSubTotalDiscountInclTax, ignore),
        //        new PropertyByName<Order>("OrderSubTotalDiscountExclTax", p => p.OrderSubTotalDiscountExclTax, ignore),
        //        new PropertyByName<Order>("OrderShippingInclTax", p => p.OrderShippingInclTax, ignore),
        //        new PropertyByName<Order>("OrderShippingExclTax", p => p.OrderShippingExclTax, ignore),
        //        new PropertyByName<Order>("PaymentMethodAdditionalFeeInclTax", p => p.PaymentMethodAdditionalFeeInclTax, ignore),
        //        new PropertyByName<Order>("PaymentMethodAdditionalFeeExclTax", p => p.PaymentMethodAdditionalFeeExclTax, ignore),
        //        new PropertyByName<Order>("TaxRates", p => p.TaxRates, ignore),
        //        new PropertyByName<Order>("OrderTax", p => p.OrderTax, ignore),
        //        new PropertyByName<Order>("OrderTotal", p => p.OrderTotal, ignore),
        //        new PropertyByName<Order>("RefundedAmount", p => p.RefundedAmount, ignore),
        //        new PropertyByName<Order>("OrderDiscount", p => p.OrderDiscount, ignore),
        //        new PropertyByName<Order>("CurrencyRate", p => p.CurrencyRate),
        //        new PropertyByName<Order>("UserCurrencyCode", p => p.UserCurrencyCode),
        //        new PropertyByName<Order>("AffiliateId", p => p.AffiliateId, ignore),
        //        new PropertyByName<Order>("PaymentMethodSystemName", p => p.PaymentMethodSystemName, ignore),
        //        new PropertyByName<Order>("ShippingPickUpInStore", p => p.PickUpInStore, ignore),
        //        new PropertyByName<Order>("ShippingMethod", p => p.ShippingMethod),
        //        new PropertyByName<Order>("ShippingRateComputationMethodSystemName", p => p.ShippingRateComputationMethodSystemName, ignore),
        //        new PropertyByName<Order>("CustomValuesXml", p => p.CustomValuesXml, ignore),
        //        new PropertyByName<Order>("VatNumber", p => p.VatNumber, ignore),
        //        new PropertyByName<Order>("CreatedOnUtc", p => p.CreatedOnUtc.ToOADate()),
        //        new PropertyByName<Order>("BillingFirstName", p => p.BillingAddress.Return(billingAddress => billingAddress.FirstName, String.Empty)),
        //        new PropertyByName<Order>("BillingLastName", p => p.BillingAddress.Return(billingAddress => billingAddress.LastName, String.Empty)),
        //        new PropertyByName<Order>("BillingEmail", p => p.BillingAddress.Return(billingAddress => billingAddress.Email, String.Empty)),
        //        new PropertyByName<Order>("BillingCompany", p => p.BillingAddress.Return(billingAddress => billingAddress.Company, String.Empty)),
        //        new PropertyByName<Order>("BillingCountry", p => p.BillingAddress.Return(billingAddress => billingAddress.Country, null).Return(country => country.Name, String.Empty)),
        //        new PropertyByName<Order>("BillingStateProvince", p => p.BillingAddress.Return(billingAddress => billingAddress.StateProvince, null).Return(stateProvince => stateProvince.Name, String.Empty)),
        //        new PropertyByName<Order>("BillingCity", p => p.BillingAddress.Return(billingAddress => billingAddress.City, String.Empty)),
        //        new PropertyByName<Order>("BillingAddress1", p => p.BillingAddress.Return(billingAddress => billingAddress.Address1, String.Empty)),
        //        new PropertyByName<Order>("BillingAddress2", p => p.BillingAddress.Return(billingAddress => billingAddress.Address2, String.Empty)),
        //        new PropertyByName<Order>("BillingZipPostalCode", p => p.BillingAddress.Return(billingAddress => billingAddress.ZipPostalCode, String.Empty)),
        //        new PropertyByName<Order>("BillingPhoneNumber", p => p.BillingAddress.Return(billingAddress => billingAddress.PhoneNumber, String.Empty)),
        //        new PropertyByName<Order>("BillingFaxNumber", p => p.BillingAddress.Return(billingAddress => billingAddress.FaxNumber, String.Empty)),
        //        new PropertyByName<Order>("ShippingFirstName", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.FirstName, String.Empty)),
        //        new PropertyByName<Order>("ShippingLastName", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.LastName, String.Empty)),
        //        new PropertyByName<Order>("ShippingEmail", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Email, String.Empty)),
        //        new PropertyByName<Order>("ShippingCompany", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Company, String.Empty)),
        //        new PropertyByName<Order>("ShippingCountry", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Country, null).Return(country => country.Name, String.Empty)),
        //        new PropertyByName<Order>("ShippingStateProvince", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.StateProvince, null).Return(stateProvince => stateProvince.Name, String.Empty)),
        //        new PropertyByName<Order>("ShippingCity", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.City, String.Empty)),
        //        new PropertyByName<Order>("ShippingAddress1", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Address1, String.Empty)),
        //        new PropertyByName<Order>("ShippingAddress2", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.Address2, String.Empty)),
        //        new PropertyByName<Order>("ShippingZipPostalCode", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.ZipPostalCode, String.Empty)),
        //        new PropertyByName<Order>("ShippingPhoneNumber", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.PhoneNumber, String.Empty)),
        //        new PropertyByName<Order>("ShippingFaxNumber", p => p.ShippingAddress.Return(shippingAddress => shippingAddress.FaxNumber, String.Empty))
        //    };

        //    return _orderSettings.ExportWithProducts ? ExportOrderToXlsxWithProducts(properties, orders) : ExportToXlsx(properties, orders);
        //}

        // <summary>
        // Export User list to XLSX
        // </summary>
        // <param name="Users">Users</param>
        public virtual byte[] ExportUsersToXlsx(IList<User> Users)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<User>("UserId", p => p.Id),
                new PropertyByName<User>("UserGuid", p => p.UserGuid),
                new PropertyByName<User>("Email", p => p.Email),
                new PropertyByName<User>("Username", p => p.Username),
                new PropertyByName<User>("Password", p => _UserService.GetCurrentPassword(p.Id).Return(password => password.Password, null)),
                new PropertyByName<User>("PasswordFormatId", p => _UserService.GetCurrentPassword(p.Id).Return(password => password.PasswordFormatId, 0)),
                new PropertyByName<User>("PasswordSalt", p => _UserService.GetCurrentPassword(p.Id).Return(password => password.PasswordSalt, null)),
                new PropertyByName<User>("IsTaxExempt", p => p.IsTaxExempt),
                new PropertyByName<User>("AffiliateId", p => p.AffiliateId),
                new PropertyByName<User>("VendorId", p => p.VendorId),
                new PropertyByName<User>("Active", p => p.Active),
                new PropertyByName<User>("IsGuest", p => p.IsGuest()),
                new PropertyByName<User>("IsRegistered", p => p.IsRegistered()),
                new PropertyByName<User>("IsAdministrator", p => p.IsAdmin()),
                //new PropertyByName<User>("IsForumModerator", p => p.IsForumModerator()),
                new PropertyByName<User>("CreatedOnUtc", p => p.CreatedOnUtc),
                //attributes
                new PropertyByName<User>("FirstName", p => p.GetAttribute<string>(SystemUserAttributeNames.FirstName)),
                new PropertyByName<User>("LastName", p => p.GetAttribute<string>(SystemUserAttributeNames.LastName)),
                new PropertyByName<User>("Gender", p => p.GetAttribute<string>(SystemUserAttributeNames.Gender)),
                new PropertyByName<User>("Company", p => p.GetAttribute<string>(SystemUserAttributeNames.Company)),
                new PropertyByName<User>("StreetAddress", p => p.GetAttribute<string>(SystemUserAttributeNames.StreetAddress)),
                new PropertyByName<User>("StreetAddress2", p => p.GetAttribute<string>(SystemUserAttributeNames.StreetAddress2)),
                new PropertyByName<User>("ZipPostalCode", p => p.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode)),
                new PropertyByName<User>("City", p => p.GetAttribute<string>(SystemUserAttributeNames.City)),
                new PropertyByName<User>("CountryId", p => p.GetAttribute<int>(SystemUserAttributeNames.CountryId)),
                new PropertyByName<User>("StateProvinceId", p => p.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId)),
                new PropertyByName<User>("Phone", p => p.GetAttribute<string>(SystemUserAttributeNames.Phone)),
                new PropertyByName<User>("Fax", p => p.GetAttribute<string>(SystemUserAttributeNames.Fax)),
                new PropertyByName<User>("VatNumber", p => p.GetAttribute<string>(SystemUserAttributeNames.VatNumber)),
                new PropertyByName<User>("VatNumberStatusId", p => p.GetAttribute<int>(SystemUserAttributeNames.VatNumberStatusId)),
                new PropertyByName<User>("TimeZoneId", p => p.GetAttribute<string>(SystemUserAttributeNames.TimeZoneId)),
                new PropertyByName<User>("AvatarPictureId", p => p.GetAttribute<int>(SystemUserAttributeNames.AvatarPictureId)),
                new PropertyByName<User>("ForumPostCount", p => p.GetAttribute<int>(SystemUserAttributeNames.ForumPostCount)),
                new PropertyByName<User>("Signature", p => p.GetAttribute<string>(SystemUserAttributeNames.Signature)),
                new PropertyByName<User>("CustomUserAttributes",  GetCustomUserAttributes)
            };

            return ExportToXlsx(properties, Users);
        }

        /// <summary>
        /// Export User list to xml
        /// </summary>
        /// <param name="Users">Users</param>
        /// <returns>Result in XML format</returns>
        public virtual string ExportUsersToXml(IList<User> Users)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            var xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Users");
            xmlWriter.WriteAttributeString("Version", InvenioVersion.CurrentVersion);

            foreach (var User in Users)
            {
                xmlWriter.WriteStartElement("User");
                xmlWriter.WriteElementString("UserId", null, User.Id.ToString());
                xmlWriter.WriteElementString("UserGuid", null, User.UserGuid.ToString());
                xmlWriter.WriteElementString("Email", null, User.Email);
                xmlWriter.WriteElementString("Username", null, User.Username);

                var UserPassword = _UserService.GetCurrentPassword(User.Id);
                xmlWriter.WriteElementString("Password", null, UserPassword.Return(password => password.Password, null));
                xmlWriter.WriteElementString("PasswordFormatId", null, UserPassword.Return(password => password.PasswordFormatId, 0).ToString());
                xmlWriter.WriteElementString("PasswordSalt", null, UserPassword.Return(password => password.PasswordSalt, null));

                xmlWriter.WriteElementString("IsTaxExempt", null, User.IsTaxExempt.ToString());
                xmlWriter.WriteElementString("AffiliateId", null, User.AffiliateId.ToString());
                xmlWriter.WriteElementString("VendorId", null, User.VendorId.ToString());
                xmlWriter.WriteElementString("Active", null, User.Active.ToString());

                xmlWriter.WriteElementString("IsGuest", null, User.IsGuest().ToString());
                xmlWriter.WriteElementString("IsRegistered", null, User.IsRegistered().ToString());
                xmlWriter.WriteElementString("IsAdministrator", null, User.IsAdmin().ToString());
                //xmlWriter.WriteElementString("IsForumModerator", null, User.IsForumModerator().ToString());
                xmlWriter.WriteElementString("CreatedOnUtc", null, User.CreatedOnUtc.ToString());

                xmlWriter.WriteElementString("FirstName", null, User.GetAttribute<string>(SystemUserAttributeNames.FirstName));
                xmlWriter.WriteElementString("LastName", null, User.GetAttribute<string>(SystemUserAttributeNames.LastName));
                xmlWriter.WriteElementString("Gender", null, User.GetAttribute<string>(SystemUserAttributeNames.Gender));
                xmlWriter.WriteElementString("Company", null, User.GetAttribute<string>(SystemUserAttributeNames.Company));

                xmlWriter.WriteElementString("CountryId", null, User.GetAttribute<int>(SystemUserAttributeNames.CountryId).ToString());
                xmlWriter.WriteElementString("StreetAddress", null, User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress));
                xmlWriter.WriteElementString("StreetAddress2", null, User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress2));
                xmlWriter.WriteElementString("ZipPostalCode", null, User.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode));
                xmlWriter.WriteElementString("City", null, User.GetAttribute<string>(SystemUserAttributeNames.City));
                xmlWriter.WriteElementString("StateProvinceId", null, User.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId).ToString());
                xmlWriter.WriteElementString("Phone", null, User.GetAttribute<string>(SystemUserAttributeNames.Phone));
                xmlWriter.WriteElementString("Fax", null, User.GetAttribute<string>(SystemUserAttributeNames.Fax));
                xmlWriter.WriteElementString("VatNumber", null, User.GetAttribute<string>(SystemUserAttributeNames.VatNumber));
                xmlWriter.WriteElementString("VatNumberStatusId", null, User.GetAttribute<int>(SystemUserAttributeNames.VatNumberStatusId).ToString());
                xmlWriter.WriteElementString("TimeZoneId", null, User.GetAttribute<string>(SystemUserAttributeNames.TimeZoneId));

                //foreach (var store in _storeService.GetAllStores())
                //{
                //    //var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, store.Id);
                //    //bool subscribedToNewsletters = newsletter != null && newsletter.Active;
                //    xmlWriter.WriteElementString(string.Format("Newsletter-in-store-{0}", store.Id), null, subscribedToNewsletters.ToString());
                //}

                xmlWriter.WriteElementString("AvatarPictureId", null, User.GetAttribute<int>(SystemUserAttributeNames.AvatarPictureId).ToString());
                xmlWriter.WriteElementString("ForumPostCount", null, User.GetAttribute<int>(SystemUserAttributeNames.ForumPostCount).ToString());
                xmlWriter.WriteElementString("Signature", null, User.GetAttribute<string>(SystemUserAttributeNames.Signature));

                var selectedUserAttributesString = User.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes, _genericAttributeService);

                if (!string.IsNullOrEmpty(selectedUserAttributesString))
                {
                    var selectedUserAttributes = new StringReader(selectedUserAttributesString);
                    var selectedUserAttributesXmlReader = XmlReader.Create(selectedUserAttributes);
                    xmlWriter.WriteNode(selectedUserAttributesXmlReader, false);
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        //public virtual string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions)
        //{
        //    if (subscriptions == null)
        //        throw new ArgumentNullException("subscriptions");

        //    const string separator = ",";
        //    var sb = new StringBuilder();
        //    foreach (var subscription in subscriptions)
        //    {
        //        sb.Append(subscription.Email);
        //        sb.Append(separator);
        //        sb.Append(subscription.Active);
        //        sb.Append(separator);
        //        sb.Append(subscription.StoreId);
        //        sb.Append(Environment.NewLine); //new line
        //    }
        //    return sb.ToString();
        //}

        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        public virtual string ExportStatesToTxt(IList<StateProvince> states)
        {
            if (states == null)
                throw new ArgumentNullException("states");

            const string separator = ",";
            var sb = new StringBuilder();
            foreach (var state in states)
            {
                sb.Append(state.Country.TwoLetterIsoCode);
                sb.Append(separator);
                sb.Append(state.Name);
                sb.Append(separator);
                sb.Append(state.Abbreviation);
                sb.Append(separator);
                sb.Append(state.Published);
                sb.Append(separator);
                sb.Append(state.DisplayOrder);
                sb.Append(Environment.NewLine); //new line
            }
            return sb.ToString();
        }

        #endregion
    }
}
