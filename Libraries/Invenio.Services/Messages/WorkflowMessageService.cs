﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Invenio.Core;
//using Invenio.Core.Domain.Blogs;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Messages;
//using Invenio.Core.Domain.Forums;
//using Invenio.Core.Domain.Messages;
//using Invenio.Core.Domain.News;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Shipping;
//using Invenio.Core.Domain.Vendors;
using Invenio.Services.Users;
using Invenio.Services.Events;
using Invenio.Services.Localization;
using Invenio.Services.Stores;

namespace Invenio.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly CommonSettings _commonSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly HttpContextBase _httpContext;

        #endregion

        #region Ctor

        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService,
            ILanguageService languageService,
            ITokenizer tokenizer, 
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            IStoreContext storeContext,
            CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IEventPublisher eventPublisher,
            HttpContextBase httpContext)
        {
            this._messageTemplateService = messageTemplateService;
            this._queuedEmailService = queuedEmailService;
            this._languageService = languageService;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._commonSettings = commonSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
            this._httpContext = httpContext;
        }

        #endregion

        #region Utilities
        
        protected virtual MessageTemplate GetActiveMessageTemplate(string messageTemplateName, int storeId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;

        }

        protected virtual int EnsureLanguageIsActive(int languageId, int storeId)
        {
            //load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = _languageService.GetAllLanguages(storeId: storeId).FirstOrDefault();
            }
            if (language == null || !language.Published)
            {
                //load any language
                language = _languageService.GetAllLanguages().FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");
            return language.Id;
        }

        #endregion

        #region Methods

        #region User workflow

        /// <summary>
        /// Sends 'New User' notification message to a store owner
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserRegisteredNotificationMessage(User User, int languageId)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.UserRegisteredNotification, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a welcome message to a User
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserWelcomeMessage(User User, int languageId)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.UserWelcomeMessage, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = User.Email;
            var toName = User.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends an email validation message to a User
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserEmailValidationMessage(User User, int languageId)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.UserEmailValidationMessage, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = User.Email;
            var toName = User.GetFullName();
                
            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends an email re-validation message to a User
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserEmailRevalidationMessage(User User, int languageId)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.UserEmailRevalidationMessage, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            //email to re-validate
            var toEmail = User.EmailToRevalidate;
            var toName = User.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends password recovery message to a User
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserPasswordRecoveryMessage(User User, int languageId)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.UserPasswordRecoveryMessage, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = User.Email;
            var toName = User.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        #endregion

        #region Order workflow

        /// <summary>
        /// Sends an order placed notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderPlacedVendorNotification(Order order, Vendor vendor, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    if (vendor == null)
        //        throw new ArgumentNullException("vendor");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderPlacedVendorNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId, vendor.Id);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = vendor.Email;
        //    var toName = vendor.Name;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderPlacedStoreOwnerNotification(Order order, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderPlacedStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends an order paid notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderPaidStoreOwnerNotification(Order order, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderPaidStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends an order paid notification to a User
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <param name="attachmentFilePath">Attachment file path</param>
        ///// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderPaidUserNotification(Order order, int languageId,
        //    string attachmentFilePath = null, string attachmentFileName = null)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderPaidUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
        //        attachmentFilePath, attachmentFileName);
        //}

        ///// <summary>
        ///// Sends an order paid notification to a vendor
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="vendor">Vendor instance</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderPaidVendorNotification(Order order, Vendor vendor, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    if (vendor == null)
        //        throw new ArgumentNullException("vendor");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderPaidVendorNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId, vendor.Id);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = vendor.Email;
        //    var toName = vendor.Name;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends an order placed notification to a User
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <param name="attachmentFilePath">Attachment file path</param>
        ///// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderPlacedUserNotification(Order order, int languageId,
        //    string attachmentFilePath = null, string attachmentFileName = null)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderPlacedUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName, 
        //        attachmentFilePath, attachmentFileName);
        //}

        ///// <summary>
        ///// Sends a shipment sent notification to a User
        ///// </summary>
        ///// <param name="shipment">Shipment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendShipmentSentUserNotification(Shipment shipment, int languageId)
        //{
        //    if (shipment == null)
        //        throw new ArgumentNullException("shipment");

        //    var order = shipment.Order;
        //    if (order == null)
        //        throw new Exception("Order cannot be loaded");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.ShipmentSentUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
        //    _messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, shipment.Order.User);
            
        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a shipment delivered notification to a User
        ///// </summary>
        ///// <param name="shipment">Shipment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendShipmentDeliveredUserNotification(Shipment shipment, int languageId)
        //{
        //    if (shipment == null)
        //        throw new ArgumentNullException("shipment");

        //    var order = shipment.Order;
        //    if (order == null)
        //        throw new Exception("Order cannot be loaded");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.ShipmentDeliveredUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
        //    _messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, shipment.Order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends an order completed notification to a User
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <param name="attachmentFilePath">Attachment file path</param>
        ///// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderCompletedUserNotification(Order order, int languageId,
        //    string attachmentFilePath = null, string attachmentFileName = null)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderCompletedUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
        //        attachmentFilePath, attachmentFileName);
        //}

        ///// <summary>
        ///// Sends an order cancelled notification to a User
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderCancelledUserNotification(Order order, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderCancelledUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends an order refunded notification to a store owner
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="refundedAmount">Amount refunded</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderRefundedStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddOrderRefundedTokens(tokens, order, refundedAmount);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends an order refunded notification to a User
        ///// </summary>
        ///// <param name="order">Order instance</param>
        ///// <param name="refundedAmount">Amount refunded</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendOrderRefundedUserNotification(Order order, decimal refundedAmount, int languageId)
        //{
        //    if (order == null)
        //        throw new ArgumentNullException("order");

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.OrderRefundedUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
        //    _messageTokenProvider.AddOrderRefundedTokens(tokens, order, refundedAmount);
        //    _messageTokenProvider.AddUserTokens(tokens, order.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a new order note added notification to a User
        ///// </summary>
        ///// <param name="orderNote">Order note</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendNewOrderNoteAddedUserNotification(OrderNote orderNote, int languageId)
        //{
        //    if (orderNote == null)
        //        throw new ArgumentNullException("orderNote");
           
        //    var order = orderNote.Order;

        //    var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewOrderNoteAddedUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderNoteTokens(tokens, orderNote);
        //    _messageTokenProvider.AddOrderTokens(tokens, orderNote.Order, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, orderNote.Order.User);
            
        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = order.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a "Recurring payment cancelled" notification to a store owner
        ///// </summary>
        ///// <param name="recurringPayment">Recurring payment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendRecurringPaymentCancelledStoreOwnerNotification(RecurringPayment recurringPayment, int languageId)
        //{
        //    if (recurringPayment == null)
        //        throw new ArgumentNullException("recurringPayment");

        //    var store = _storeService.GetStoreById(recurringPayment.InitialOrder.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.RecurringPaymentCancelledStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, recurringPayment.InitialOrder, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, recurringPayment.InitialOrder.User);
        //    _messageTokenProvider.AddRecurringPaymentTokens(tokens, recurringPayment);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a "Recurring payment cancelled" notification to a User
        ///// </summary>
        ///// <param name="recurringPayment">Recurring payment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendRecurringPaymentCancelledUserNotification(RecurringPayment recurringPayment, int languageId)
        //{
        //    if (recurringPayment == null)
        //        throw new ArgumentNullException("recurringPayment");

        //    var store = _storeService.GetStoreById(recurringPayment.InitialOrder.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.RecurringPaymentCancelledUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, recurringPayment.InitialOrder, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, recurringPayment.InitialOrder.User);
        //    _messageTokenProvider.AddRecurringPaymentTokens(tokens, recurringPayment);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = recurringPayment.InitialOrder.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}", 
        //        recurringPayment.InitialOrder.BillingAddress.FirstName, recurringPayment.InitialOrder.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a "Recurring payment failed" notification to a User
        ///// </summary>
        ///// <param name="recurringPayment">Recurring payment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendRecurringPaymentFailedUserNotification(RecurringPayment recurringPayment, int languageId)
        //{
        //    if (recurringPayment == null)
        //        throw new ArgumentNullException("recurringPayment");

        //    var store = _storeService.GetStoreById(recurringPayment.InitialOrder.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.RecurringPaymentFailedUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddOrderTokens(tokens, recurringPayment.InitialOrder, languageId);
        //    _messageTokenProvider.AddUserTokens(tokens, recurringPayment.InitialOrder.User);
        //    _messageTokenProvider.AddRecurringPaymentTokens(tokens, recurringPayment);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = recurringPayment.InitialOrder.BillingAddress.Email;
        //    var toName = string.Format("{0} {1}",
        //        recurringPayment.InitialOrder.BillingAddress.FirstName, recurringPayment.InitialOrder.BillingAddress.LastName);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        //#endregion

        //#region Newsletter workflow

        ///// <summary>
        ///// Sends a newsletter subscription activation message
        ///// </summary>
        ///// <param name="subscription">Newsletter subscription</param>
        ///// <param name="languageId">Language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription, int languageId)
        //{
        //    if (subscription == null)
        //        throw new ArgumentNullException("subscription");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);
            
        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, subscription.Email, string.Empty);
        //}

        ///// <summary>
        ///// Sends a newsletter subscription deactivation message
        ///// </summary>
        ///// <param name="subscription">Newsletter subscription</param>
        ///// <param name="languageId">Language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription, int languageId)
        //{
        //    if (subscription == null)
        //        throw new ArgumentNullException("subscription");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, subscription.Email, string.Empty);
        //}

        //#endregion
        
        //#region Send a message to a friend

        ///// <summary>
        ///// Sends "email a friend" message
        ///// </summary>
        ///// <param name="User">User instance</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <param name="product">Product instance</param>
        ///// <param name="UserEmail">User's email</param>
        ///// <param name="friendsEmail">Friend's email</param>
        ///// <param name="personalMessage">Personal message</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendProductEmailAFriendMessage(User User, int languageId,
        //    Product product, string UserEmail, string friendsEmail, string personalMessage)
        //{
        //    if (User == null)
        //        throw new ArgumentNullException("User");

        //    if (product == null)
        //        throw new ArgumentNullException("product");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.EmailAFriendMessage, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddUserTokens(tokens, User);
        //    _messageTokenProvider.AddProductTokens(tokens, product, languageId);
        //    tokens.Add(new Token("EmailAFriend.PersonalMessage", personalMessage, true));
        //    tokens.Add(new Token("EmailAFriend.Email", UserEmail));

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
            
        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, friendsEmail, string.Empty);
        //}

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="UserEmail">User's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendWishlistEmailAFriendMessage(User User, int languageId,
             string UserEmail, string friendsEmail, string personalMessage)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.WishlistToFriendMessage, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);
            tokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
            tokens.Add(new Token("Wishlist.Email", UserEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, friendsEmail, string.Empty);
        }

        #endregion

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        //{
        //    if (returnRequest == null)
        //        throw new ArgumentNullException("returnRequest");

        //    var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddUserTokens(tokens, returnRequest.User);
        //    _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends 'New Return Request' message to a User
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendNewReturnRequestUserNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        //{
        //    if (returnRequest == null)
        //        throw new ArgumentNullException("returnRequest");

        //    var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewReturnRequestUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;
            
        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddUserTokens(tokens, returnRequest.User);
        //    _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = returnRequest.User.IsGuest() ?
        //        orderItem.Order.BillingAddress.Email :
        //        returnRequest.User.Email;
        //    var toName = returnRequest.User.IsGuest() ?
        //        orderItem.Order.BillingAddress.FirstName :
        //        returnRequest.User.GetFullName();

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends 'Return Request status changed' message to a User
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendReturnRequestStatusChangedUserNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        //{
        //    if (returnRequest == null)
        //        throw new ArgumentNullException("returnRequest");

        //    var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.ReturnRequestStatusChangedUserNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddUserTokens(tokens, returnRequest.User);
        //    _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    string toEmail = returnRequest.User.IsGuest() ? 
        //        orderItem.Order.BillingAddress.Email :
        //        returnRequest.User.Email;
        //    var toName = returnRequest.User.IsGuest() ? 
        //        orderItem.Order.BillingAddress.FirstName :
        //        returnRequest.User.GetFullName();

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}
        
        #endregion

        #region Forum Notifications

        /// <summary>
        /// Sends a forum subscription message to a User
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public int SendNewForumTopicMessage(User User, ForumTopic forumTopic, Forum forum, int languageId)
        //{
        //    if (User == null)
        //        throw new ArgumentNullException("User");

        //    var store = _storeContext.CurrentStore;

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewForumTopicMessage, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddForumTopicTokens(tokens, forumTopic);
        //    _messageTokenProvider.AddForumTokens(tokens, forumTopic.Forum);
        //    _messageTokenProvider.AddUserTokens(tokens, User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = User.Email;
        //    var toName = User.GetFullName();

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends a forum subscription message to a User
        /// </summary>
        /// <param name="User">User instance</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public int SendNewForumPostMessage(User User, ForumPost forumPost, ForumTopic forumTopic,
        //    Forum forum, int friendlyForumTopicPageIndex, int languageId)
        //{
        //    if (User == null)
        //        throw new ArgumentNullException("User");

        //    var store = _storeContext.CurrentStore;

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewForumPostMessage, store.Id);
        //    if (messageTemplate == null )
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddForumPostTokens(tokens, forumPost);
        //    _messageTokenProvider.AddForumTopicTokens(tokens, forumPost.ForumTopic, friendlyForumTopicPageIndex, forumPost.Id);
        //    _messageTokenProvider.AddForumTokens(tokens, forumPost.ForumTopic.Forum);
        //    _messageTokenProvider.AddUserTokens(tokens, User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
          
        //    var toEmail = User.Email;
        //    var toName = User.GetFullName();

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public int SendPrivateMessageNotification(PrivateMessage privateMessage, int languageId)
        //{
        //    if (privateMessage == null)
        //        throw new ArgumentNullException("privateMessage");

        //    var store = _storeService.GetStoreById(privateMessage.StoreId) ?? _storeContext.CurrentStore;

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.PrivateMessageNotification, store.Id);
        //    if (messageTemplate == null )
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddPrivateMessageTokens(tokens, privateMessage);
        //    _messageTokenProvider.AddUserTokens(tokens, privateMessage.ToUser);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
           
        //    var toEmail = privateMessage.ToUser.Email;
        //    var toName = privateMessage.ToUser.GetFullName();

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        #endregion

        #region Misc

        /// <summary>
        /// Sends 'New vendor account submitted' message to a store owner
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        //public virtual int SendNewVendorAccountApplyStoreOwnerNotification(User User, Vendor vendor, int languageId)
        //{
        //    if (User == null)
        //        throw new ArgumentNullException("User");

        //    if (vendor == null)
        //        throw new ArgumentNullException("vendor");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewVendorAccountApplyStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddUserTokens(tokens, User);
        //    _messageTokenProvider.AddVendorTokens(tokens, vendor);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends 'Vendor information changed' message to a store owner
        ///// </summary>
        ///// <param name="vendor">Vendor</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendVendorInformationChangeNotification(Vendor vendor, int languageId)
        //{
        //    if (vendor == null)
        //        throw new ArgumentNullException("vendor");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.VendorInformationChangeNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddVendorTokens(tokens, vendor);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a gift card notification
        ///// </summary>
        ///// <param name="giftCard">Gift card</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendGiftCardNotification(GiftCard giftCard, int languageId)
        //{
        //    if (giftCard == null)
        //        throw new ArgumentNullException("giftCard");

        //    var order = giftCard.PurchasedWithOrderItem != null ? giftCard.PurchasedWithOrderItem.Order : null;
        //    var store = order != null ? _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore : _storeContext.CurrentStore;

        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.GiftCardNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddGiftCardTokens(tokens, giftCard);
            
        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = giftCard.RecipientEmail;
        //    var toName = giftCard.RecipientName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}
        
        ///// <summary>
        ///// Sends a product review notification message to a store owner
        ///// </summary>
        ///// <param name="productReview">Product review</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendProductReviewNotificationMessage(ProductReview productReview, int languageId)
        //{
        //    if (productReview == null)
        //        throw new ArgumentNullException("productReview");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.ProductReviewNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddProductReviewTokens(tokens, productReview);
        //    _messageTokenProvider.AddUserTokens(tokens, productReview.User);
            
        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a "quantity below" notification to a store owner
        ///// </summary>
        ///// <param name="product">Product</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendQuantityBelowStoreOwnerNotification(Product product,  int languageId)
        //{
        //    if (product== null)
        //        throw new ArgumentNullException("product");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.QuantityBelowStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddProductTokens(tokens, product, languageId);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a "quantity below" notification to a store owner
        ///// </summary>
        ///// <param name="combination">Attribute combination</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendQuantityBelowStoreOwnerNotification(ProductAttributeCombination combination, int languageId)
        //{
        //    if (combination == null)
        //        throw new ArgumentNullException("combination");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.QuantityBelowAttributeCombinationStoreOwnerNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    var product = combination.Product;

        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddProductTokens(tokens, product, languageId);
        //    _messageTokenProvider.AddAttributeCombinationTokens(tokens, combination, languageId);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends a "new VAT submitted" notification to a store owner
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewVatSubmittedStoreOwnerNotification(User User,
            string vatName, string vatAddress, int languageId)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewVatSubmittedStoreOwnerNotification, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddUserTokens(tokens, User);
            tokens.Add(new Token("VatValidationResult.Name", vatName));
            tokens.Add(new Token("VatValidationResult.Address", vatAddress));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        ///// <summary>
        ///// Sends a blog comment notification message to a store owner
        ///// </summary>
        ///// <param name="blogComment">Blog comment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendBlogCommentNotificationMessage(BlogComment blogComment, int languageId)
        //{
        //    if (blogComment == null)
        //        throw new ArgumentNullException("blogComment");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.BlogCommentNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddBlogCommentTokens(tokens, blogComment);
        //    _messageTokenProvider.AddUserTokens(tokens, blogComment.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a news comment notification message to a store owner
        ///// </summary>
        ///// <param name="newsComment">News comment</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendNewsCommentNotificationMessage(NewsComment newsComment, int languageId)
        //{
        //    if (newsComment == null)
        //        throw new ArgumentNullException("newsComment");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.NewsCommentNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddNewsCommentTokens(tokens, newsComment);
        //    _messageTokenProvider.AddUserTokens(tokens, newsComment.User);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = emailAccount.Email;
        //    var toName = emailAccount.DisplayName;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        ///// <summary>
        ///// Sends a 'Back in stock' notification message to a User
        ///// </summary>
        ///// <param name="subscription">Subscription</param>
        ///// <param name="languageId">Message language identifier</param>
        ///// <returns>Queued email identifier</returns>
        //public virtual int SendBackInStockNotification(BackInStockSubscription subscription, int languageId)
        //{
        //    if (subscription == null)
        //        throw new ArgumentNullException("subscription");

        //    var store = _storeService.GetStoreById(subscription.StoreId) ?? _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.BackInStockNotification, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    _messageTokenProvider.AddUserTokens(tokens, subscription.User);
        //    _messageTokenProvider.AddBackInStockTokens(tokens, subscription);

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var User = subscription.User;
        //    var toEmail = User.Email;
        //    var toName = User.GetFullName();

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        //}

        /// <summary>
        /// Sends "contact us" message
        /// </summary>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendContactUsMessage(int languageId, string senderEmail,
            string senderName, string subject, string body)
        {
            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.ContactUsMessage, store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            string fromEmail;
            string fromName;
            //required for some SMTP servers
            if (_commonSettings.UseSystemEmailForContactUsForm)
            {
                fromEmail = emailAccount.Email;
                fromName = emailAccount.DisplayName;
                body = string.Format("<strong>From</strong>: {0} - {1}<br /><br />{2}",
                    _httpContext.Server.HtmlEncode(senderName), _httpContext.Server.HtmlEncode(senderEmail), body);
            }
            else
            {
                fromEmail = senderEmail;
                fromName = senderName;
            }

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            tokens.Add(new Token("ContactUs.SenderEmail", senderEmail));
            tokens.Add(new Token("ContactUs.SenderName", senderName));
            tokens.Add(new Token("ContactUs.Body", body, true));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                fromEmail: fromEmail,
                fromName: fromName,
                subject: subject,
                replyToEmailAddress: senderEmail,
                replyToName: senderName);
        }

        /// <summary>
        /// Sends "contact vendor" message
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>Queued email identifier</returns>
        //public virtual int SendContactVendorMessage(Vendor vendor, int languageId, string senderEmail,
        //    string senderName, string subject, string body)
        //{
        //    if (vendor == null)
        //        throw new ArgumentNullException("vendor");

        //    var store = _storeContext.CurrentStore;
        //    languageId = EnsureLanguageIsActive(languageId, store.Id);

        //    var messageTemplate = GetActiveMessageTemplate(MessageTemplateSystemNames.ContactVendorMessage, store.Id);
        //    if (messageTemplate == null)
        //        return 0;

        //    //email account
        //    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

        //    string fromEmail;
        //    string fromName;
        //    //required for some SMTP servers
        //    if (_commonSettings.UseSystemEmailForContactUsForm)
        //    {
        //        fromEmail = emailAccount.Email;
        //        fromName = emailAccount.DisplayName;
        //        body = string.Format("<strong>From</strong>: {0} - {1}<br /><br />{2}",
        //            _httpContext.Server.HtmlEncode(senderName), _httpContext.Server.HtmlEncode(senderEmail), body);
        //    }
        //    else
        //    {
        //        fromEmail = senderEmail;
        //        fromName = senderName;
        //    }

        //    //tokens
        //    var tokens = new List<Token>();
        //    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
        //    tokens.Add(new Token("ContactUs.SenderEmail", senderEmail));
        //    tokens.Add(new Token("ContactUs.SenderName", senderName));
        //    tokens.Add(new Token("ContactUs.Body", body, true));

        //    //event notification
        //    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

        //    var toEmail = vendor.Email;
        //    var toName = vendor.Name;

        //    return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
        //        fromEmail: fromEmail,
        //        fromName: fromName,
        //        subject: subject,
        //        replyToEmailAddress: senderEmail,
        //        replyToName: senderName);
        //}

        /// <summary>
        /// Sends a test email
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <param name="sendToEmail">Send to email</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendTestEmail(int messageTemplateId, string sendToEmail, List<Token> tokens, int languageId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, sendToEmail, null);
        }

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            //retrieve localized message template data
            var bcc = messageTemplate.GetLocalized(mt => mt.BccEmailAddresses, languageId);
            if (String.IsNullOrEmpty(subject))
                subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }      

        #endregion

        #endregion
    }
}
