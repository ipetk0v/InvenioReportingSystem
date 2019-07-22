using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Domain.Users;
using Invenio.Core.Infrastructure;
using Invenio.Services.Common;
using Invenio.Services.Users.Cache;
using Invenio.Services.Localization;

namespace Invenio.Services.Users
{
    public static class UserExtensions
    {
        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="User">User</param>
        /// <returns>User full name</returns>
        public static string GetFullName(this User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");
            var firstName = User.GetAttribute<string>(SystemUserAttributeNames.FirstName);
            var lastName = User.GetAttribute<string>(SystemUserAttributeNames.LastName);

            string fullName = "";
            if (!String.IsNullOrWhiteSpace(firstName) && !String.IsNullOrWhiteSpace(lastName))
                fullName = string.Format("{0} {1}", firstName, lastName);
            else
            {
                if (!String.IsNullOrWhiteSpace(firstName))
                    fullName = firstName;

                if (!String.IsNullOrWhiteSpace(lastName))
                    fullName = lastName;
            }
            return fullName;
        }
        /// <summary>
        /// Formats the User name
        /// </summary>
        /// <param name="User">Source</param>
        /// <param name="stripTooLong">Strip too long User name</param>
        /// <param name="maxLength">Maximum User name length</param>
        /// <returns>Formatted text</returns>
        public static string FormatUserName(this User User, bool stripTooLong = false, int maxLength = 0)
        {
            if (User == null)
                return string.Empty;

            if (User.IsGuest())
            {
                return EngineContext.Current.Resolve<ILocalizationService>().GetResource("User.Guest");
            }

            string result = string.Empty;
            switch (EngineContext.Current.Resolve<UserSettings>().UserNameFormat)
            {
                case UserNameFormat.ShowEmails:
                    result = User.Email;
                    break;
                case UserNameFormat.ShowUsernames:
                    result = User.Username;
                    break;
                case UserNameFormat.ShowFullNames:
                    result = User.GetFullName();
                    break;
                case UserNameFormat.ShowFirstName:
                    result = User.GetAttribute<string>(SystemUserAttributeNames.FirstName);
                    break;
                default:
                    break;
            }

            if (stripTooLong && maxLength > 0)
            {
                result = CommonHelper.EnsureMaximumLength(result, maxLength);
            }

            return result;
        }

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="User">User</param>
        /// <returns>Coupon codes</returns>
        public static string[] ParseAppliedDiscountCouponCodes(this User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            var existingCouponCodes = User.GetAttribute<string>(SystemUserAttributeNames.DiscountCouponCode,
                genericAttributeService);

            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }
        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static void ApplyDiscountCouponCode(this User User, string couponCode)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string result = string.Empty;
            try
            {
                var existingCouponCodes = User.GetAttribute<string>(SystemUserAttributeNames.DiscountCouponCode,
                    genericAttributeService);

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("DiscountCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//DiscountCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//DiscountCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.DiscountCouponCode, result);
        }
        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static void RemoveDiscountCouponCode(this User User, string couponCode)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            //get applied coupon codes
            var existingCouponCodes = User.ParseAppliedDiscountCouponCodes();

            //clear them
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            genericAttributeService.SaveAttribute<string>(User, SystemUserAttributeNames.DiscountCouponCode, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase))
                    User.ApplyDiscountCouponCode(existingCouponCode);
        }


        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <param name="User">User</param>
        /// <returns>Coupon codes</returns>
        public static string[] ParseAppliedGiftCardCouponCodes(this User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            var existingCouponCodes = User.GetAttribute<string>(SystemUserAttributeNames.GiftCardCouponCodes,
                genericAttributeService);

            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(existingCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }
        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public static void ApplyGiftCardCouponCode(this User User, string couponCode)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string result = string.Empty;
            try
            {
                var existingCouponCodes = User.GetAttribute<string>(SystemUserAttributeNames.GiftCardCouponCodes,
                    genericAttributeService);

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(existingCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("GiftCardCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(existingCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//GiftCardCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string couponCodeAttribute = node1.Attributes["Code"].InnerText.Trim();
                        if (couponCodeAttribute.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.GiftCardCouponCodes, result);
        }
        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public static void RemoveGiftCardCouponCode(this User User, string couponCode)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            //get applied coupon codes
            var existingCouponCodes = User.ParseAppliedGiftCardCouponCodes();

            //clear them
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            genericAttributeService.SaveAttribute<string>(User, SystemUserAttributeNames.GiftCardCouponCodes, null);

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase))
                    User.ApplyGiftCardCouponCode(existingCouponCode);
        }

        /// <summary>
        /// Check whether password recovery token is valid
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="token">Token to validate</param>
        /// <returns>Result</returns>
        public static bool IsPasswordRecoveryTokenValid(this User User, string token)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var cPrt = User.GetAttribute<string>(SystemUserAttributeNames.PasswordRecoveryToken);
            if (String.IsNullOrEmpty(cPrt))
                return false;

            if (!cPrt.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
        /// <summary>
        /// Check whether password recovery link is expired
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="UserSettings">User settings</param>
        /// <returns>Result</returns>
        public static bool IsPasswordRecoveryLinkExpired(this User User, UserSettings UserSettings)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            if (UserSettings == null)
                throw new ArgumentNullException("UserSettings");

            if (UserSettings.PasswordRecoveryLinkDaysValid == 0)
                return false;
            
            var geneatedDate = User.GetAttribute<DateTime?>(SystemUserAttributeNames.PasswordRecoveryTokenDateGenerated);
            if (!geneatedDate.HasValue)
                return false;

            var daysPassed = (DateTime.UtcNow - geneatedDate.Value).TotalDays;
            if (daysPassed > UserSettings.PasswordRecoveryLinkDaysValid)
                return true;

            return false;
        }

        /// <summary>
        /// Get User role identifiers
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>User role identifiers</returns>
        public static int[] GetUserRoleIds(this User User, bool showHidden = false)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var UserRolesIds = User.UserRoles
               .Where(cr => showHidden || cr.Active)
               .Select(cr => cr.Id)
               .ToArray();

            return UserRolesIds;
        }

        /// <summary>
        /// Check whether User password is expired 
        /// </summary>
        /// <param name="User">User</param>
        /// <returns>True if password is expired; otherwise false</returns>
        public static bool PasswordIsExpired(this User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            //the guests don't have a password
            if (User.IsGuest())
                return false;

            //password lifetime is disabled for user
            if (!User.UserRoles.Any(role => role.Active && role.EnablePasswordLifetime))
                return false;

            //setting disabled for all
            var UserSettings = EngineContext.Current.Resolve<UserSettings>();
            if (UserSettings.PasswordLifetime == 0)
                return false;

            //cache result between HTTP requests 
            var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            var cacheKey = string.Format(UserCacheEventConsumer.User_PASSWORD_LIFETIME, User.Id);
            //get current password usage time
            var currentLifetime = cacheManager.Get(cacheKey, () =>
            {
                var UserPassword = EngineContext.Current.Resolve<IUserService>().GetCurrentPassword(User.Id);
                //password is not found, so return max value to force User to change password
                if (UserPassword == null)
                    return int.MaxValue;

                return (DateTime.UtcNow - UserPassword.CreatedOnUtc).Days;
            });

            return currentLifetime >= UserSettings.PasswordLifetime;
        }
    }
}
