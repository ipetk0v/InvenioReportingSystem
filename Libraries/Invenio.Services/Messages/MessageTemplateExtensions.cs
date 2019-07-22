using System.Collections.Generic;
using Invenio.Core.Domain.Messages;

namespace Invenio.Services.Messages
{
    /// <summary>
    /// Represents message template  extensions
    /// </summary>
    public static class MessageTemplateExtensions
    {
        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public static IEnumerable<string> GetTokenGroups(this MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            switch (messageTemplate.Name)
            {
                case MessageTemplateSystemNames.UserRegisteredNotification:
                case MessageTemplateSystemNames.UserWelcomeMessage:
                case MessageTemplateSystemNames.UserEmailValidationMessage:
                case MessageTemplateSystemNames.UserEmailRevalidationMessage:
                case MessageTemplateSystemNames.UserPasswordRecoveryMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.OrderPlacedVendorNotification:
                case MessageTemplateSystemNames.OrderPlacedStoreOwnerNotification:
                case MessageTemplateSystemNames.OrderPaidStoreOwnerNotification:
                case MessageTemplateSystemNames.OrderPaidUserNotification:
                case MessageTemplateSystemNames.OrderPaidVendorNotification:
                case MessageTemplateSystemNames.OrderPlacedUserNotification:
                case MessageTemplateSystemNames.OrderCompletedUserNotification:
                case MessageTemplateSystemNames.OrderCancelledUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.ShipmentSentUserNotification:
                case MessageTemplateSystemNames.ShipmentDeliveredUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ShipmentTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.OrderRefundedStoreOwnerNotification:
                case MessageTemplateSystemNames.OrderRefundedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.RefundedOrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewOrderNoteAddedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderNoteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.RecurringPaymentCancelledStoreOwnerNotification:
                case MessageTemplateSystemNames.RecurringPaymentCancelledUserNotification:
                case MessageTemplateSystemNames.RecurringPaymentFailedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens, TokenGroupNames.RecurringPaymentTokens };

                case MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage:
                case MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens };

                case MessageTemplateSystemNames.EmailAFriendMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.ProductTokens, TokenGroupNames.EmailAFriendTokens };

                case MessageTemplateSystemNames.WishlistToFriendMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.WishlistToFriendTokens };

                case MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification:
                case MessageTemplateSystemNames.NewReturnRequestUserNotification:
                case MessageTemplateSystemNames.ReturnRequestStatusChangedUserNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.ReturnRequestTokens };

                case MessageTemplateSystemNames.NewForumTopicMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewForumPostMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumPostTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.PrivateMessageNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.PrivateMessageTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewVendorAccountApplyStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.VendorTokens };

                case MessageTemplateSystemNames.VendorInformationChangeNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.VendorTokens };

                case MessageTemplateSystemNames.GiftCardNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.GiftCardTokens};

                case MessageTemplateSystemNames.ProductReviewNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductReviewTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.QuantityBelowStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductTokens };

                case MessageTemplateSystemNames.QuantityBelowAttributeCombinationStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductTokens, TokenGroupNames.AttributeCombinationTokens };

                case MessageTemplateSystemNames.NewVatSubmittedStoreOwnerNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.VatValidation };

                case MessageTemplateSystemNames.BlogCommentNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.BlogCommentTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewsCommentNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.NewsCommentTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.BackInStockNotification:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.UserTokens, TokenGroupNames.ProductBackInStockTokens };

                case MessageTemplateSystemNames.ContactUsMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactUs };

                case MessageTemplateSystemNames.ContactVendorMessage:
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactVendor };

                default:
                    return new string[] { };
            }
        }
    }
}