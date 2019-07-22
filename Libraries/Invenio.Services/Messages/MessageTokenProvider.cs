using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Invenio.Core;
using Invenio.Core.Domain;
//using Invenio.Core.Domain.Blogs;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
//using Invenio.Core.Domain.Forums;
using Invenio.Core.Domain.Messages;
//using Invenio.Core.Domain.News;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Payments;
//using Invenio.Core.Domain.Shipping;
using Invenio.Core.Domain.Stores;
//using Invenio.Core.Domain.Tax;
//using Invenio.Core.Domain.Vendors;
using Invenio.Core.Html;
using Invenio.Core.Infrastructure;
//using  Invenio.Services.Catalog;
using Invenio.Services.Common;
using Invenio.Services.Users;
using Invenio.Services.Directory;
using Invenio.Services.Events;
//using  Invenio.Services.Forums;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Media;
//using  Invenio.Services.Orders;
//using  Invenio.Services.Payments;
//using  Invenio.Services.Seo;
//using  Invenio.Services.Shipping;
//using  Invenio.Services.Shipping.Tracking;
using Invenio.Services.Stores;

namespace Invenio.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        //private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        //private readonly IOrderService _orderService;
        //private readonly IPaymentService _paymentService;
        //private readonly IProductAttributeParser _productAttributeParser;
        //private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IUserAttributeFormatter _UserAttributeFormatter;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;

        private readonly MessageTemplatesSettings _templatesSettings;
        //private readonly CatalogSettings _catalogSettings;
        //private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        //private readonly ShippingSettings _shippingSettings;
        //private readonly PaymentSettings _paymentSettings;

        private readonly IEventPublisher _eventPublisher;
        //private readonly StoreInformationSettings _storeInformationSettings;

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            //IPriceFormatter priceFormatter, 
            ICurrencyService currencyService,
            IWorkContext workContext,
            IDownloadService downloadService,
            //IOrderService orderService,
            //IPaymentService paymentService,
            IStoreService storeService,
            IStoreContext storeContext,
            //IProductAttributeParser productAttributeParser,
            //IAddressAttributeFormatter addressAttributeFormatter,
            IUserAttributeFormatter UserAttributeFormatter,
            MessageTemplatesSettings templatesSettings,
            //CatalogSettings catalogSettings,
            //TaxSettings taxSettings,
            CurrencySettings currencySettings,
            //ShippingSettings shippingSettings,
            //PaymentSettings paymentSettings,
            IEventPublisher eventPublisher
            /*StoreInformationSettings storeInformationSettings*/)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            //this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._downloadService = downloadService;
            //this._orderService = orderService;
            //this._paymentService = paymentService;
            //this._productAttributeParser = productAttributeParser;
            //this._addressAttributeFormatter = addressAttributeFormatter;
            this._UserAttributeFormatter = UserAttributeFormatter;
            this._storeService = storeService;
            this._storeContext = storeContext;

            this._templatesSettings = templatesSettings;
            //this._catalogSettings = catalogSettings;
            //this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            //this._shippingSettings = shippingSettings;
            //this._paymentSettings = paymentSettings;
            this._eventPublisher = eventPublisher;
            //this._storeInformationSettings = storeInformationSettings;
        }

        #endregion

        #region Allowed tokens

        private Dictionary<string, IEnumerable<string>> _allowedTokens;
        /// <summary>
        /// Get all available tokens by token groups
        /// </summary>
        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //store tokens
                _allowedTokens.Add(TokenGroupNames.StoreTokens, new[]
                {
                    "%Store.Name%",
                    "%Store.URL%",
                    "%Store.Email%",
                    "%Store.CompanyName%",
                    "%Store.CompanyAddress%",
                    "%Store.CompanyPhoneNumber%",
                    "%Store.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%",
                    "%GooglePlus.URL%"
                });

                //User tokens
                _allowedTokens.Add(TokenGroupNames.UserTokens, new[]
                {
                    "%User.Email%",
                    "%User.Username%",
                    "%User.FullName%",
                    "%User.FirstName%",
                    "%User.LastName%",
                    "%User.VatNumber%",
                    "%User.VatNumberStatus%",
                    "%User.CustomAttributes%",
                    "%User.PasswordRecoveryURL%",
                    "%User.AccountActivationURL%",
                    "%User.EmailRevalidationURL%",
                    "%Wishlist.URLForUser%"
                });

                //order tokens
                _allowedTokens.Add(TokenGroupNames.OrderTokens, new[]
                {
                    "%Order.OrderNumber%",
                    "%Order.UserFullName%",
                    "%Order.UserEmail%",
                    "%Order.BillingFirstName%",
                    "%Order.BillingLastName%",
                    "%Order.BillingPhoneNumber%",
                    "%Order.BillingEmail%",
                    "%Order.BillingFaxNumber%",
                    "%Order.BillingCompany%",
                    "%Order.BillingAddress1%",
                    "%Order.BillingAddress2%",
                    "%Order.BillingCity%",
                    "%Order.BillingStateProvince%",
                    "%Order.BillingZipPostalCode%",
                    "%Order.BillingCountry%",
                    "%Order.BillingCustomAttributes%",
                    "%Order.Shippable%",
                    "%Order.ShippingMethod%",
                    "%Order.ShippingFirstName%",
                    "%Order.ShippingLastName%",
                    "%Order.ShippingPhoneNumber%",
                    "%Order.ShippingEmail%",
                    "%Order.ShippingFaxNumber%",
                    "%Order.ShippingCompany%",
                    "%Order.ShippingAddress1%",
                    "%Order.ShippingAddress2%",
                    "%Order.ShippingCity%",
                    "%Order.ShippingStateProvince%",
                    "%Order.ShippingZipPostalCode%",
                    "%Order.ShippingCountry%",
                    "%Order.ShippingCustomAttributes%",
                    "%Order.PaymentMethod%",
                    "%Order.VatNumber%",
                    "%Order.CustomValues%",
                    "%Order.Product(s)%",
                    "%Order.CreatedOn%",
                    "%Order.OrderURLForUser%"
                });

                //shipment tokens
                _allowedTokens.Add(TokenGroupNames.ShipmentTokens, new[]
                {
                    "%Shipment.ShipmentNumber%",
                    "%Shipment.TrackingNumber%",
                    "%Shipment.TrackingNumberURL%",
                    "%Shipment.Product(s)%",
                    "%Shipment.URLForUser%"
                });

                //refunded order tokens
                _allowedTokens.Add(TokenGroupNames.RefundedOrderTokens, new[]
                {
                    "%Order.AmountRefunded%"
                });

                //order note tokens
                _allowedTokens.Add(TokenGroupNames.OrderNoteTokens, new[]
                {
                    "%Order.NewNoteText%",
                    "%Order.OrderNoteAttachmentUrl%"
                });

                //recurring payment tokens
                _allowedTokens.Add(TokenGroupNames.RecurringPaymentTokens, new[]
                {
                    "%RecurringPayment.ID%",
                    "%RecurringPayment.CancelAfterFailedPayment%",
                    "%RecurringPayment.RecurringPaymentType%"
                });

                //newsletter subscription tokens
                _allowedTokens.Add(TokenGroupNames.SubscriptionTokens, new[]
                {
                    "%NewsLetterSubscription.Email%",
                    "%NewsLetterSubscription.ActivationUrl%",
                    "%NewsLetterSubscription.DeactivationUrl%"
                });

                //product tokens
                _allowedTokens.Add(TokenGroupNames.ProductTokens, new[]
                {
                    "%Product.ID%",
                    "%Product.Name%",
                    "%Product.ShortDescription%",
                    "%Product.ProductURLForUser%",
                    "%Product.SKU%",
                    "%Product.StockQuantity%"
                });

                //return request tokens
                _allowedTokens.Add(TokenGroupNames.ReturnRequestTokens, new[]
                {
                    "%ReturnRequest.CustomNumber%",
                    "%ReturnRequest.OrderId%",
                    "%ReturnRequest.Product.Quantity%",
                    "%ReturnRequest.Product.Name%",
                    "%ReturnRequest.Reason%",
                    "%ReturnRequest.RequestedAction%",
                    "%ReturnRequest.UserComment%",
                    "%ReturnRequest.StaffNotes%",
                    "%ReturnRequest.Status%"
                });

                //forum tokens
                _allowedTokens.Add(TokenGroupNames.ForumTokens, new[]
                {
                    "%Forums.ForumURL%",
                    "%Forums.ForumName%"
                });

                //forum topic tokens
                _allowedTokens.Add(TokenGroupNames.ForumTopicTokens, new[]
                {
                    "%Forums.TopicURL%",
                    "%Forums.TopicName%"
                });

                //forum post tokens
                _allowedTokens.Add(TokenGroupNames.ForumPostTokens, new[]
                {
                    "%Forums.PostAuthor%",
                    "%Forums.PostBody%"
                });

                //private message tokens
                _allowedTokens.Add(TokenGroupNames.PrivateMessageTokens, new[]
                {
                    "%PrivateMessage.Subject%",
                    "%PrivateMessage.Text%"
                });

                //vendor tokens
                _allowedTokens.Add(TokenGroupNames.VendorTokens, new[]
                {
                    "%Vendor.Name%",
                    "%Vendor.Email%"
                });

                //gift card tokens
                _allowedTokens.Add(TokenGroupNames.GiftCardTokens, new[]
                {
                    "%GiftCard.SenderName%",
                    "%GiftCard.SenderEmail%",
                    "%GiftCard.RecipientName%",
                    "%GiftCard.RecipientEmail%",
                    "%GiftCard.Amount%",
                    "%GiftCard.CouponCode%",
                    "%GiftCard.Message%"
                });

                //product review tokens
                _allowedTokens.Add(TokenGroupNames.ProductReviewTokens, new[]
                {
                    "%ProductReview.ProductName%"
                });

                //attribute combination tokens
                _allowedTokens.Add(TokenGroupNames.AttributeCombinationTokens, new[]
                {
                    "%AttributeCombination.Formatted%",
                    "%AttributeCombination.SKU%",
                    "%AttributeCombination.StockQuantity%"
                });

                //blog comment tokens
                _allowedTokens.Add(TokenGroupNames.BlogCommentTokens, new[]
                {
                    "%BlogComment.BlogPostTitle%"
                });

                //news comment tokens
                _allowedTokens.Add(TokenGroupNames.NewsCommentTokens, new[]
                {
                    "%NewsComment.NewsTitle%"
                });

                //product back in stock tokens
                _allowedTokens.Add(TokenGroupNames.ProductBackInStockTokens, new[]
                {
                    "%BackInStockSubscription.ProductName%",
                    "%BackInStockSubscription.ProductUrl%"
                });

                //email a friend tokens
                _allowedTokens.Add(TokenGroupNames.EmailAFriendTokens, new[]
                {
                    "%EmailAFriend.PersonalMessage%",
                    "%EmailAFriend.Email%"
                });

                //wishlist to friend tokens
                _allowedTokens.Add(TokenGroupNames.WishlistToFriendTokens, new[]
                {
                    "%Wishlist.PersonalMessage%",
                    "%Wishlist.Email%"
                });

                //VAT validation tokens
                _allowedTokens.Add(TokenGroupNames.VatValidation, new[]
                {
                    "%VatValidationResult.Name%",
                    "%VatValidationResult.Address%"
                });

                //contact us tokens
                _allowedTokens.Add(TokenGroupNames.ContactUs, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                //contact vendor tokens
                _allowedTokens.Add(TokenGroupNames.ContactVendor, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                return _allowedTokens;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="vendorId">Vendor identifier (used to limit products by vendor</param>
        ///// <returns>HTML table of products</returns>
        //protected virtual string ProductListToHtmlTable(Order order, int languageId, int vendorId)
        //{
        //    string result;

        //    var language = _languageService.GetLanguageById(languageId);

        //    var sb = new StringBuilder();
        //    sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

        //    #region Products
        //    sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
        //    sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
        //    sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price", languageId)));
        //    sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
        //    sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Total", languageId)));
        //    sb.AppendLine("</tr>");

        //    var table = order.OrderItems.ToList();
        //    for (int i = 0; i <= table.Count - 1; i++)
        //    {
        //        var orderItem = table[i];
        //        var product = orderItem.Product;
        //        if (product == null)
        //            continue;

        //        if (vendorId > 0 && product.VendorId != vendorId)
        //            continue;

        //        sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
        //        //product name
        //        string productName = product.GetLocalized(x => x.Name, languageId);

        //        sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + HttpUtility.HtmlEncode(productName));
        //        //add download link
        //        if (_downloadService.IsDownloadAllowed(orderItem))
        //        {
        //            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //            string downloadUrl = string.Format("{0}download/getdownload/{1}", GetStoreUrl(order.StoreId), orderItem.OrderItemGuid);
        //            string downloadLink = string.Format("<a class=\"link\" href=\"{0}\">{1}</a>", downloadUrl, _localizationService.GetResource("Messages.Order.Product(s).Download", languageId));
        //            sb.AppendLine("<br />");
        //            sb.AppendLine(downloadLink);
        //        }
        //        //add download link
        //        if (_downloadService.IsLicenseDownloadAllowed(orderItem))
        //        {
        //            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //            string licenseUrl = string.Format("{0}download/getlicense/{1}", GetStoreUrl(order.StoreId), orderItem.OrderItemGuid);
        //            string licenseLink = string.Format("<a class=\"link\" href=\"{0}\">{1}</a>", licenseUrl, _localizationService.GetResource("Messages.Order.Product(s).License", languageId));
        //            sb.AppendLine("<br />");
        //            sb.AppendLine(licenseLink);
        //        }
        //        //attributes
        //        if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
        //        {
        //            sb.AppendLine("<br />");
        //            sb.AppendLine(orderItem.AttributeDescription);
        //        }
        //        //rental info
        //        if (orderItem.Product.IsRental)
        //        {
        //            var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : string.Empty;
        //            var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : string.Empty;
        //            var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
        //                rentalStartDate, rentalEndDate);
        //            sb.AppendLine("<br />");
        //            sb.AppendLine(rentalInfo);
        //        }
        //        //sku
        //        if (_catalogSettings.ShowSkuOnProductDetailsPage)
        //        {
        //            var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
        //            if (!String.IsNullOrEmpty(sku))
        //            {
        //                sb.AppendLine("<br />");
        //                sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(sku)));
        //            }
        //        }
        //        sb.AppendLine("</td>");

        //        string unitPriceStr;
        //        if (order.UserTaxDisplayType == TaxDisplayType.IncludingTax)
        //        {
        //            //including tax
        //            var unitPriceInclTaxInUserCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
        //            unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInUserCurrency, true, order.UserCurrencyCode, language, true);
        //        }
        //        else
        //        {
        //            //excluding tax
        //            var unitPriceExclTaxInUserCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
        //            unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInUserCurrency, true, order.UserCurrencyCode, language, false);
        //        }
        //        sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", unitPriceStr));

        //        sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", orderItem.Quantity));

        //        string priceStr; 
        //        if (order.UserTaxDisplayType == TaxDisplayType.IncludingTax)
        //        {
        //            //including tax
        //            var priceInclTaxInUserCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
        //            priceStr = _priceFormatter.FormatPrice(priceInclTaxInUserCurrency, true, order.UserCurrencyCode, language, true);
        //        }
        //        else
        //        {
        //            //excluding tax
        //            var priceExclTaxInUserCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
        //            priceStr = _priceFormatter.FormatPrice(priceExclTaxInUserCurrency, true, order.UserCurrencyCode, language, false);
        //        }
        //        sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", priceStr));

        //        sb.AppendLine("</tr>");
        //    }
        //    #endregion

        //    if (vendorId == 0)
        //    {
        //        //we render checkout attributes and totals only for store owners (hide for vendors)

        //        #region Checkout Attributes

        //        if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription))
        //        {
        //            sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
        //            sb.AppendLine(order.CheckoutAttributeDescription);
        //            sb.AppendLine("</td></tr>");
        //        }

        //        #endregion

        //        #region Totals

        //        //subtotal
        //        string cusSubTotal;
        //        bool displaySubTotalDiscount = false;
        //        string cusSubTotalDiscount = string.Empty;
        //        if (order.UserTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
        //        {
        //            //including tax

        //            //subtotal
        //            var orderSubtotalInclTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
        //            cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInUserCurrency, true, order.UserCurrencyCode, language, true);
        //            //discount (applied to order subtotal)
        //            var orderSubTotalDiscountInclTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
        //            if (orderSubTotalDiscountInclTaxInUserCurrency > decimal.Zero)
        //            {
        //                cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInUserCurrency, true, order.UserCurrencyCode, language, true);
        //                displaySubTotalDiscount = true;
        //            }
        //        }
        //        else
        //        {
        //            //exсluding tax

        //            //subtotal
        //            var orderSubtotalExclTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
        //            cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInUserCurrency, true, order.UserCurrencyCode, language, false);
        //            //discount (applied to order subtotal)
        //            var orderSubTotalDiscountExclTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
        //            if (orderSubTotalDiscountExclTaxInUserCurrency > decimal.Zero)
        //            {
        //                cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInUserCurrency, true, order.UserCurrencyCode, language, false);
        //                displaySubTotalDiscount = true;
        //            }
        //        }

        //        //shipping, payment method fee
        //        string cusShipTotal;
        //        string cusPaymentMethodAdditionalFee;
        //        var taxRates = new SortedDictionary<decimal, decimal>();
        //        string cusTaxTotal = string.Empty;
        //        string cusDiscount = string.Empty;
        //        string cusTotal; 
        //        if (order.UserTaxDisplayType == TaxDisplayType.IncludingTax)
        //        {
        //            //including tax

        //            //shipping
        //            var orderShippingInclTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
        //            cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInUserCurrency, true, order.UserCurrencyCode, language, true);
        //            //payment method additional fee
        //            var paymentMethodAdditionalFeeInclTaxInUserCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
        //            cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInUserCurrency, true, order.UserCurrencyCode, language, true);
        //        }
        //        else
        //        {
        //            //excluding tax

        //            //shipping
        //            var orderShippingExclTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
        //            cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInUserCurrency, true, order.UserCurrencyCode, language, false);
        //            //payment method additional fee
        //            var paymentMethodAdditionalFeeExclTaxInUserCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
        //            cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInUserCurrency, true, order.UserCurrencyCode, language, false);
        //        }

        //        //shipping
        //        bool displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

        //        //payment method fee
        //        bool displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

        //        //tax
        //        bool displayTax = true;
        //        bool displayTaxRates = true;
        //        if (_taxSettings.HideTaxInOrderSummary && order.UserTaxDisplayType == TaxDisplayType.IncludingTax)
        //        {
        //            displayTax = false;
        //            displayTaxRates = false;
        //        }
        //        else
        //        {
        //            if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
        //            {
        //                displayTax = false;
        //                displayTaxRates = false;
        //            }
        //            else
        //            {
        //                taxRates = new SortedDictionary<decimal, decimal>();
        //                foreach (var tr in order.TaxRatesDictionary)
        //                    taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

        //                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
        //                displayTax = !displayTaxRates;

        //                var orderTaxInUserCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
        //                string taxStr = _priceFormatter.FormatPrice(orderTaxInUserCurrency, true, order.UserCurrencyCode, false, language);
        //                cusTaxTotal = taxStr;
        //            }
        //        }

        //        //discount
        //        bool displayDiscount = false;
        //        if (order.OrderDiscount > decimal.Zero)
        //        {
        //            var orderDiscountInUserCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
        //            cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInUserCurrency, true, order.UserCurrencyCode, false, language);
        //            displayDiscount = true;
        //        }

        //        //total
        //        var orderTotalInUserCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
        //        cusTotal = _priceFormatter.FormatPrice(orderTotalInUserCurrency, true, order.UserCurrencyCode, false, language);




        //        //subtotal
        //        sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));

        //        //discount (applied to order subtotal)
        //        if (displaySubTotalDiscount)
        //        {
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
        //        }


        //        //shipping
        //        if (displayShipping)
        //        {
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
        //        }

        //        //payment method fee
        //        if (displayPaymentMethodFee)
        //        {
        //            string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
        //        }

        //        //tax
        //        if (displayTax)
        //        {
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
        //        }
        //        if (displayTaxRates)
        //        {
        //            foreach (var item in taxRates)
        //            {
        //                string taxRate = String.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
        //                string taxValue = _priceFormatter.FormatPrice(item.Value, true, order.UserCurrencyCode, false, language);
        //                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, taxRate, taxValue));
        //            }
        //        }

        //        //discount
        //        if (displayDiscount)
        //        {
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
        //        }

        //        //gift cards
        //        var gcuhC = order.GiftCardUsageHistory;
        //        foreach (var gcuh in gcuhC)
        //        {
        //            string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), HttpUtility.HtmlEncode(gcuh.GiftCard.GiftCardCouponCode));
        //            string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.UserCurrencyCode, false, language);
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, giftCardText, giftCardAmount));
        //        }

        //        //reward points
        //        if (order.RedeemedRewardPointsEntry != null)
        //        {
        //            string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -order.RedeemedRewardPointsEntry.Points);
        //            string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.UserCurrencyCode, false, language);
        //            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, rpTitle, rpAmount));
        //        }

        //        //total
        //        sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), cusTotal));
        //        #endregion

        //    }

        //    sb.AppendLine("</table>");
        //    result = sb.ToString();
        //    return result;
        //}

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Language identifier</param>
        ///// <returns>HTML table of products</returns>
        //protected virtual string ProductListToHtmlTable(Shipment shipment, int languageId)
        //{
        //    string result;

        //    var sb = new StringBuilder();
        //    sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

        //    #region Products
        //    sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
        //    sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
        //    sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
        //    sb.AppendLine("</tr>");

        //    var table = shipment.ShipmentItems.ToList();
        //    for (int i = 0; i <= table.Count - 1; i++)
        //    {
        //        var si = table[i];
        //        var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
        //        if (orderItem == null)
        //            continue;

        //        var product = orderItem.Product;
        //        if (product == null)
        //            continue;

        //        sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
        //        //product name
        //        string productName = product.GetLocalized(x => x.Name, languageId);

        //        sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + HttpUtility.HtmlEncode(productName));
        //        //attributes
        //        if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
        //        {
        //            sb.AppendLine("<br />");
        //            sb.AppendLine(orderItem.AttributeDescription);
        //        }
        //        //rental info
        //        if (orderItem.Product.IsRental)
        //        {
        //            var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : string.Empty;
        //            var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : string.Empty;
        //            var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
        //                rentalStartDate, rentalEndDate);
        //            sb.AppendLine("<br />");
        //            sb.AppendLine(rentalInfo);
        //        }
        //        //sku
        //        if (_catalogSettings.ShowSkuOnProductDetailsPage)
        //        {
        //            var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
        //            if (!String.IsNullOrEmpty(sku))
        //            {
        //                sb.AppendLine("<br />");
        //                sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(sku)));
        //            }
        //        }
        //        sb.AppendLine("</td>");

        //        sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", si.Quantity));

        //        sb.AppendLine("</tr>");
        //    }
        //    #endregion

        //    sb.AppendLine("</table>");
        //    result = sb.ToString();
        //    return result;
        //}

        /// <summary>
        /// Get store URL
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <returns></returns>
        protected virtual string GetStoreUrl(int storeId = 0)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
                throw new Exception("No store could be loaded");

            return store.Url;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name = "tokens" > List of already added tokens</param>
        /// <param name = "store" > Store </ param >
        /// <param name="emailAccount">Email account</param>
        public virtual void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            tokens.Add(new Token("Store.Name", store.GetLocalized(x => x.Name)));
            tokens.Add(new Token("Store.URL", store.Url, true));
            tokens.Add(new Token("Store.Email", emailAccount.Email));
            tokens.Add(new Token("Store.CompanyName", store.CompanyName));
            tokens.Add(new Token("Store.CompanyAddress", store.CompanyAddress));
            tokens.Add(new Token("Store.CompanyPhoneNumber", store.CompanyPhoneNumber));
            tokens.Add(new Token("Store.CompanyVat", store.CompanyVat));

            //tokens.Add(new Token("Facebook.URL", _storeInformationSettings.FacebookLink));
            //tokens.Add(new Token("Twitter.URL", _storeInformationSettings.TwitterLink));
            //tokens.Add(new Token("YouTube.URL", _storeInformationSettings.YoutubeLink));
            //tokens.Add(new Token("GooglePlus.URL", _storeInformationSettings.GooglePlusLink));

            //event notification
            _eventPublisher.EntityTokensAdded(store, tokens);
        }

        /// <summary>
        /// Add order tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order"></param>
        /// <param name="languageId">Language identifier</param>
        ///// <param name="vendorId">Vendor identifier</param>
        //public virtual void AddOrderTokens(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
        //{
        //    tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));

        //    tokens.Add(new Token("Order.UserFullName", string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName)));
        //    tokens.Add(new Token("Order.UserEmail", order.BillingAddress.Email));


        //    tokens.Add(new Token("Order.BillingFirstName", order.BillingAddress.FirstName));
        //    tokens.Add(new Token("Order.BillingLastName", order.BillingAddress.LastName));
        //    tokens.Add(new Token("Order.BillingPhoneNumber", order.BillingAddress.PhoneNumber));
        //    tokens.Add(new Token("Order.BillingEmail", order.BillingAddress.Email));
        //    tokens.Add(new Token("Order.BillingFaxNumber", order.BillingAddress.FaxNumber));
        //    tokens.Add(new Token("Order.BillingCompany", order.BillingAddress.Company));
        //    tokens.Add(new Token("Order.BillingAddress1", order.BillingAddress.Address1));
        //    tokens.Add(new Token("Order.BillingAddress2", order.BillingAddress.Address2));
        //    tokens.Add(new Token("Order.BillingCity", order.BillingAddress.City));
        //    tokens.Add(new Token("Order.BillingStateProvince", order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.GetLocalized(x => x.Name) : string.Empty));
        //    tokens.Add(new Token("Order.BillingZipPostalCode", order.BillingAddress.ZipPostalCode));
        //    tokens.Add(new Token("Order.BillingCountry", order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name) : string.Empty));
        //    tokens.Add(new Token("Order.BillingCustomAttributes", _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes), true));

        //    tokens.Add(new Token("Order.Shippable", !string.IsNullOrEmpty(order.ShippingMethod)));
        //    tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
        //    tokens.Add(new Token("Order.ShippingFirstName", order.ShippingAddress != null ? order.ShippingAddress.FirstName : string.Empty));
        //    tokens.Add(new Token("Order.ShippingLastName", order.ShippingAddress != null ? order.ShippingAddress.LastName : string.Empty));
        //    tokens.Add(new Token("Order.ShippingPhoneNumber", order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : string.Empty));
        //    tokens.Add(new Token("Order.ShippingEmail", order.ShippingAddress != null ? order.ShippingAddress.Email : string.Empty));
        //    tokens.Add(new Token("Order.ShippingFaxNumber", order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : string.Empty));
        //    tokens.Add(new Token("Order.ShippingCompany", order.ShippingAddress != null ? order.ShippingAddress.Company : string.Empty));
        //    tokens.Add(new Token("Order.ShippingAddress1", order.ShippingAddress != null ? order.ShippingAddress.Address1 : string.Empty));
        //    tokens.Add(new Token("Order.ShippingAddress2", order.ShippingAddress != null ? order.ShippingAddress.Address2 : string.Empty));
        //    tokens.Add(new Token("Order.ShippingCity", order.ShippingAddress != null ? order.ShippingAddress.City : string.Empty));
        //    tokens.Add(new Token("Order.ShippingStateProvince", order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name) : string.Empty));
        //    tokens.Add(new Token("Order.ShippingZipPostalCode", order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : string.Empty));
        //    tokens.Add(new Token("Order.ShippingCountry", order.ShippingAddress != null && order.ShippingAddress.Country != null ? order.ShippingAddress.Country.GetLocalized(x => x.Name) : string.Empty));
        //    tokens.Add(new Token("Order.ShippingCustomAttributes", _addressAttributeFormatter.FormatAttributes(order.ShippingAddress != null ? order.ShippingAddress.CustomAttributes : string.Empty), true));

        //    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
        //    var paymentMethodName = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : order.PaymentMethodSystemName;
        //    tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
        //    tokens.Add(new Token("Order.VatNumber", order.VatNumber));
        //    var sbCustomValues = new StringBuilder();
        //    var customValues = order.DeserializeCustomValues();
        //    if (customValues != null)
        //    {
        //        foreach (var item in customValues)
        //        {
        //            sbCustomValues.AppendFormat("{0}: {1}", HttpUtility.HtmlEncode(item.Key), HttpUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
        //            sbCustomValues.Append("<br />");
        //        }
        //    }
        //    tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));



        //    tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, languageId, vendorId), true));

        //    var language = _languageService.GetLanguageById(languageId);
        //    if (language != null && !String.IsNullOrEmpty(language.LanguageCulture))
        //    {
        //        DateTime createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelper.GetUserTimeZone(order.User));
        //        tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
        //    }
        //    else
        //    {
        //        tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
        //    }

        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    tokens.Add(new Token("Order.OrderURLForUser", string.Format("{0}orderdetails/{1}", GetStoreUrl(order.StoreId), order.Id), true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(order, tokens);
        //}

        /// <summary>
        /// Add refunded order tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">Order</param>
        /// <param name="refundedAmount">Refunded amount of order</param>
        //public virtual void AddOrderRefundedTokens(IList<Token> tokens, Order order, decimal refundedAmount)
        //{
        //    //should we convert it to User currency?
        //    //most probably, no. It can cause some rounding or legal issues
        //    //furthermore, exchange rate could be changed
        //    //so let's display it the primary store currency

        //    var primaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
        //    var refundedAmountStr = _priceFormatter.FormatPrice(refundedAmount, true, primaryStoreCurrencyCode, false, _workContext.WorkingLanguage);

        //    tokens.Add(new Token("Order.AmountRefunded", refundedAmountStr));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(order, tokens);
        //}

        ///// <summary>
        ///// Add shipment tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="shipment">Shipment item</param>
        ///// <param name="languageId">Language identifier</param>
        //public virtual void AddShipmentTokens(IList<Token> tokens, Shipment shipment, int languageId)
        //{
        //    tokens.Add(new Token("Shipment.ShipmentNumber", shipment.Id));
        //    tokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));
        //    var trackingNumberUrl = string.Empty;
        //    if (!String.IsNullOrEmpty(shipment.TrackingNumber))
        //    {
        //        //we cannot inject IShippingService into constructor because it'll cause circular references.
        //        //that's why we resolve it here this way
        //        var shipmentTracker = shipment.GetShipmentTracker(EngineContext.Current.Resolve<IShippingService>(), _shippingSettings);
        //        if (shipmentTracker != null)
        //            trackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
        //    }
        //    tokens.Add(new Token("Shipment.TrackingNumberURL", trackingNumberUrl, true));
        //    tokens.Add(new Token("Shipment.Product(s)", ProductListToHtmlTable(shipment, languageId), true));
        //    tokens.Add(new Token("Shipment.URLForUser", string.Format("{0}orderdetails/shipment/{1}", GetStoreUrl(shipment.Order.StoreId), shipment.Id), true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(shipment, tokens);
        //}

        ///// <summary>
        ///// Add order note tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="orderNote">Order note</param>
        //public virtual void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote)
        //{
        //    tokens.Add(new Token("Order.NewNoteText", orderNote.FormatOrderNoteText(), true));
        //    tokens.Add(new Token("Order.OrderNoteAttachmentUrl", string.Format("{0}download/ordernotefile/{1}", GetStoreUrl(orderNote.Order.StoreId), orderNote.Id), true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(orderNote, tokens);
        //}

        ///// <summary>
        ///// Add recurring payment tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="recurringPayment">Recurring payment</param>
        //public virtual void AddRecurringPaymentTokens(IList<Token> tokens, RecurringPayment recurringPayment)
        //{
        //    tokens.Add(new Token("RecurringPayment.ID", recurringPayment.Id));
        //    tokens.Add(new Token("RecurringPayment.CancelAfterFailedPayment", 
        //        recurringPayment.LastPaymentFailed && _paymentSettings.CancelRecurringPaymentsAfterFailedPayment));
        //    if (recurringPayment.InitialOrder != null)
        //        tokens.Add(new Token("RecurringPayment.RecurringPaymentType", _paymentService.GetRecurringPaymentType(recurringPayment.InitialOrder.PaymentMethodSystemName).ToString()));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(recurringPayment, tokens);
        //}

        ///// <summary>
        ///// Add return request tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="returnRequest">Return request</param>
        ///// <param name="orderItem">Order item</param>
        //public virtual void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem)
        //{
        //    tokens.Add(new Token("ReturnRequest.CustomNumber", returnRequest.CustomNumber));
        //    tokens.Add(new Token("ReturnRequest.OrderId", orderItem.OrderId));
        //    tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity));
        //    tokens.Add(new Token("ReturnRequest.Product.Name", orderItem.Product.Name));
        //    tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
        //    tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
        //    tokens.Add(new Token("ReturnRequest.UserComment", HtmlHelper.FormatText(returnRequest.UserComments, false, true, false, false, false, false), true));
        //    tokens.Add(new Token("ReturnRequest.StaffNotes", HtmlHelper.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
        //    tokens.Add(new Token("ReturnRequest.Status", returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext)));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(returnRequest, tokens);
        //}

        ///// <summary>
        ///// Add gift card tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="giftCard">Gift card</param>
        //public virtual void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard)
        //{
        //    tokens.Add(new Token("GiftCard.SenderName", giftCard.SenderName));
        //    tokens.Add(new Token("GiftCard.SenderEmail",giftCard.SenderEmail));
        //    tokens.Add(new Token("GiftCard.RecipientName", giftCard.RecipientName));
        //    tokens.Add(new Token("GiftCard.RecipientEmail", giftCard.RecipientEmail));
        //    tokens.Add(new Token("GiftCard.Amount", _priceFormatter.FormatPrice(giftCard.Amount, true, false)));
        //    tokens.Add(new Token("GiftCard.CouponCode", giftCard.GiftCardCouponCode));

        //    var giftCardMesage = !String.IsNullOrWhiteSpace(giftCard.Message) ? 
        //        HtmlHelper.FormatText(giftCard.Message, false, true, false, false, false, false) : string.Empty;

        //    tokens.Add(new Token("GiftCard.Message", giftCardMesage, true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(giftCard, tokens);
        //}

        /// <summary>
        /// Add User tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="User">User</param>
        public virtual void AddUserTokens(IList<Token> tokens, User User)
        {
            tokens.Add(new Token("User.Email", User.Email));
            tokens.Add(new Token("User.Username", User.Username));
            tokens.Add(new Token("User.FullName", User.GetFullName()));
            tokens.Add(new Token("User.FirstName", User.GetAttribute<string>(SystemUserAttributeNames.FirstName)));
            tokens.Add(new Token("User.LastName", User.GetAttribute<string>(SystemUserAttributeNames.LastName)));
            tokens.Add(new Token("User.VatNumber", User.GetAttribute<string>(SystemUserAttributeNames.VatNumber)));
            //tokens.Add(new Token("User.VatNumberStatus", ((VatNumberStatus)User.GetAttribute<int>(SystemUserAttributeNames.VatNumberStatusId)).ToString()));

            var customAttributesXml = User.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes);
            tokens.Add(new Token("User.CustomAttributes", _UserAttributeFormatter.FormatAttributes(customAttributesXml), true));


            //note: we do not use SEO friendly URLS because we can get errors caused by having .(dot) in the URL (from the email address)
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", GetStoreUrl(), User.GetAttribute<string>(SystemUserAttributeNames.PasswordRecoveryToken), HttpUtility.UrlEncode(User.Email));
            var accountActivationUrl = string.Format("{0}User/activation?token={1}&email={2}", GetStoreUrl(), User.GetAttribute<string>(SystemUserAttributeNames.AccountActivationToken), HttpUtility.UrlEncode(User.Email));
            var emailRevalidationUrl = string.Format("{0}User/revalidateemail?token={1}&email={2}", GetStoreUrl(), User.GetAttribute<string>(SystemUserAttributeNames.EmailRevalidationToken), HttpUtility.UrlEncode(User.Email));
            var wishlistUrl = string.Format("{0}wishlist/{1}", GetStoreUrl(), User.UserGuid);

            tokens.Add(new Token("User.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("User.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("User.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForUser", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(User, tokens);
        }

        ///// <summary>
        ///// Add vendor tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="vendor">Vendor</param>
        //public virtual void AddVendorTokens(IList<Token> tokens, Vendor vendor)
        //{
        //    tokens.Add(new Token("Vendor.Name", vendor.Name));
        //    tokens.Add(new Token("Vendor.Email", vendor.Email));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(vendor, tokens);
        //}

        ///// <summary>
        ///// Add newsletter subscription tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="subscription">Newsletter subscription</param>
        //public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        //{
        //    tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));


        //    const string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";

        //    var activationUrl = String.Format(urlFormat, GetStoreUrl(), subscription.NewsLetterSubscriptionGuid, "true");
        //    tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

        //    var deActivationUrl = String.Format(urlFormat, GetStoreUrl(), subscription.NewsLetterSubscriptionGuid, "false");
        //    tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(subscription, tokens);
        //}

        ///// <summary>
        ///// Add product review tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="productReview">Product review</param>
        //public virtual void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview)
        //{
        //    tokens.Add(new Token("ProductReview.ProductName", productReview.Product.Name));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(productReview, tokens);
        //}

        ///// <summary>
        ///// Add blog comment tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="blogComment">Blog post comment</param>
        //public virtual void AddBlogCommentTokens(IList<Token> tokens, BlogComment blogComment)
        //{
        //    tokens.Add(new Token("BlogComment.BlogPostTitle", blogComment.BlogPost.Title));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(blogComment, tokens);
        //}

        ///// <summary>
        ///// Add news comment tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="newsComment">News comment</param>
        //public virtual void AddNewsCommentTokens(IList<Token> tokens, NewsComment newsComment)
        //{
        //    tokens.Add(new Token("NewsComment.NewsTitle", newsComment.NewsItem.Title));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(newsComment, tokens);
        //}

        ///// <summary>
        ///// Add product tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="product">Product</param>
        ///// <param name="languageId">Language identifier</param>
        //public virtual void AddProductTokens(IList<Token> tokens, Product product, int languageId)
        //{
        //    tokens.Add(new Token("Product.ID", product.Id));
        //    tokens.Add(new Token("Product.Name", product.GetLocalized(x => x.Name, languageId)));
        //    tokens.Add(new Token("Product.ShortDescription", product.GetLocalized(x => x.ShortDescription, languageId), true));
        //    tokens.Add(new Token("Product.SKU", product.Sku));
        //    tokens.Add(new Token("Product.StockQuantity", product.GetTotalStockQuantity()));

        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    var productUrl = string.Format("{0}{1}", GetStoreUrl(), product.GetSeName());
        //    tokens.Add(new Token("Product.ProductURLForUser", productUrl, true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(product, tokens);
        //}

        ///// <summary>
        ///// Add product attribute combination tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="combination">Product attribute combination</param>
        ///// <param name="languageId">Language identifier</param>
        //public virtual void AddAttributeCombinationTokens(IList<Token> tokens, ProductAttributeCombination combination,  int languageId)
        //{
        //    //attributes
        //    //we cannot inject IProductAttributeFormatter into constructor because it'll cause circular references.
        //    //that's why we resolve it here this way
        //    var productAttributeFormatter = EngineContext.Current.Resolve<IProductAttributeFormatter>();
        //    string attributes = productAttributeFormatter.FormatAttributes(combination.Product, 
        //        combination.AttributesXml, 
        //        _workContext.CurrentUser, 
        //        renderPrices: false);



        //    tokens.Add(new Token("AttributeCombination.Formatted", attributes, true));
        //    tokens.Add(new Token("AttributeCombination.SKU", combination.Product.FormatSku(combination.AttributesXml, _productAttributeParser)));
        //    tokens.Add(new Token("AttributeCombination.StockQuantity", combination.StockQuantity));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(combination, tokens);
        //}

        ///// <summary>
        ///// Add forum topic tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="forumTopic">Forum topic</param>
        ///// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        ///// <param name="appendedPostIdentifierAnchor">Forum post identifier</param>
        //public virtual void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic, 
        //    int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null)
        //{
        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    string topicUrl;
        //    if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
        //        topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", GetStoreUrl(), forumTopic.Id, forumTopic.GetSeName(), friendlyForumTopicPageIndex.Value);
        //    else
        //        topicUrl = string.Format("{0}boards/topic/{1}/{2}", GetStoreUrl(), forumTopic.Id, forumTopic.GetSeName());
        //    if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
        //        topicUrl = string.Format("{0}#{1}", topicUrl, appendedPostIdentifierAnchor.Value);
        //    tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
        //    tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(forumTopic, tokens);
        //}

        ///// <summary>
        ///// Add forum post tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="forumPost">Forum post</param>
        //public virtual void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost)
        //{
        //    tokens.Add(new Token("Forums.PostAuthor", forumPost.User.FormatUserName()));
        //    tokens.Add(new Token("Forums.PostBody", forumPost.FormatPostText(), true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(forumPost, tokens);
        //}

        ///// <summary>
        ///// Add forum tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="forum">Forum</param>
        //public virtual void AddForumTokens(IList<Token> tokens, Forum forum)
        //{
        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    var forumUrl = string.Format("{0}boards/forum/{1}/{2}", GetStoreUrl(), forum.Id, forum.GetSeName());
        //    tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
        //    tokens.Add(new Token("Forums.ForumName", forum.Name));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(forum, tokens);
        //}

        ///// <summary>
        ///// Add private message tokens
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="privateMessage">Private message</param>
        //public virtual void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage)
        //{
        //    tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
        //    tokens.Add(new Token("PrivateMessage.Text",  privateMessage.FormatPrivateMessageText(), true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(privateMessage, tokens);
        //}

        ///// <summary>
        ///// Add tokens of BackInStock subscription
        ///// </summary>
        ///// <param name="tokens">List of already added tokens</param>
        ///// <param name="subscription">BackInStock subscription</param>
        //public virtual void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription)
        //{
        //    tokens.Add(new Token("BackInStockSubscription.ProductName", subscription.Product.Name));
        //    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
        //    var productUrl = string.Format("{0}{1}", GetStoreUrl(subscription.StoreId), subscription.Product.GetSeName());
        //    tokens.Add(new Token("BackInStockSubscription.ProductUrl", productUrl, true));

        //    //event notification
        //    _eventPublisher.EntityTokensAdded(subscription, tokens);
        //}

        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>Collection of allowed (supported) message tokens for campaigns</returns>
        public virtual IEnumerable<string> GetListOfCampaignAllowedTokens()
        {
            var additionTokens = new CampaignAdditionTokensAddedEvent();
            _eventPublisher.Publish(additionTokens);

            var allowedTokens = GetListOfAllowedTokens(new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens }).ToList();
            allowedTokens.AddRange(additionTokens.AdditionTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        public virtual IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null)
        {
            var additionTokens = new AdditionTokensAddedEvent();
            _eventPublisher.Publish(additionTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            allowedTokens.AddRange(additionTokens.AdditionTokens);

            return allowedTokens.Distinct();
        }

        #endregion
    }
}
