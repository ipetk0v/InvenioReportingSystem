using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Invenio.Admin.Validators.Users;
using Invenio.Core.Domain.Catalog;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    [Validator(typeof(UserValidator))]
    public partial class UserModel : BaseNopEntityModel
    {
        public UserModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.SendEmail = new SendEmailModel() { SendImmediately = true };
            this.SendPm = new SendPmModel();

            this.SelectedUserRoleIds= new List<int>();
            this.AvailableUserRoles = new List<SelectListItem>();

            this.SelectedManufacturerIds = new List<int>();
            this.SelectedManufacturerRegionIds = new List<int>();
            this.AvailableManufacturers = new List<SelectListItem>();
            this.AvailableManufacturerRegions = new List<SelectListItem>();

            this.AssociatedExternalAuthRecords = new List<AssociatedExternalAuthModel>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
            this.AvailableVendors = new List<SelectListItem>();
            this.UserAttributes = new List<UserAttributeModel>();
            this.AvailableNewsletterSubscriptionStores = new List<StoreModel>();
            this.RewardPointsAvailableStores = new List<SelectListItem>();
        }
       
        public bool UsernamesEnabled { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.Password")]
        [AllowHtml]
        [DataType(DataType.Password)]
        [NoTrim]
        public string Password { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.Vendor")]
        public int VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.FullName")]
        public string FullName { get; set; }
        
        public bool DateOfBirthEnabled { get; set; }
        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Users.Users.Fields.DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        public bool CompanyEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.StreetAddress")]
        [AllowHtml]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.StreetAddress2")]
        [AllowHtml]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Phone")]
        [AllowHtml]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Fax")]
        [AllowHtml]
        public string Fax { get; set; }

        public List<UserAttributeModel> UserAttributes { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.RegisteredInStore")]
        public string RegisteredInStore { get; set; }



        [NopResourceDisplayName("Admin.Users.Users.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }
        
        [NopResourceDisplayName("Admin.Users.Users.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.Fields.Affiliate")]
        public int AffiliateId { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Affiliate")]
        public string AffiliateName { get; set; }




        //time zone
        [NopResourceDisplayName("Admin.Users.Users.Fields.TimeZoneId")]
        [AllowHtml]
        public string TimeZoneId { get; set; }

        public bool AllowUsersToSetTimeZone { get; set; }

        public IList<SelectListItem> AvailableTimeZones { get; set; }





        //EU VAT
        [NopResourceDisplayName("Admin.Users.Users.Fields.VatNumber")]
        [AllowHtml]
        public string VatNumber { get; set; }

        public string VatNumberStatusNote { get; set; }

        public bool DisplayVatNumber { get; set; }





        //registration date
        [NopResourceDisplayName("Admin.Users.Users.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        //IP adderss
        [NopResourceDisplayName("Admin.Users.Users.Fields.IPAddress")]
        public string LastIpAddress { get; set; }


        [NopResourceDisplayName("Admin.Users.Users.Fields.LastVisitedPage")]
        public string LastVisitedPage { get; set; }


        //User roles
        [NopResourceDisplayName("Admin.Users.Users.Fields.UserRoles")]
        public string UserRoleNames { get; set; }
        public List<SelectListItem> AvailableUserRoles { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.UserRoles")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedUserRoleIds { get; set; }

        //manufacturer
        [NopResourceDisplayName("Admin.Users.Users.Fields.Manufacturers")]
        public string ManufacturerNames { get; set; }
        public List<SelectListItem> AvailableManufacturers { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Manufacturers")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedManufacturerIds { get; set; }

        //manufacturer region
        [NopResourceDisplayName("Admin.Users.Users.Fields.ManufacturerRegions")]
        public string ManufacturerRegionNames { get; set; }
        public List<SelectListItem> AvailableManufacturerRegions { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.ManufacturerRegion")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedManufacturerRegionIds { get; set; }

        //[NopResourceDisplayName("Admin.Users.Users.Fields.ManufacturerRegion")]
        //public int SelectedManufacturerRegionId { get; set; }
        //[NopResourceDisplayName("Admin.Users.Users.ManufacturerRegion.Fields.Regions")]
        //public IList<SelectListItem> AvailableManufacturerRegions { get; set; }

        //newsletter subscriptions (per store)
        [NopResourceDisplayName("Admin.Users.Users.Fields.Newsletter")]
        public List<StoreModel> AvailableNewsletterSubscriptionStores { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.Fields.Newsletter")]
        public int[] SelectedNewsletterSubscriptionStoreIds { get; set; }



        //reward points history
        public bool DisplayRewardPointsHistory { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.AddRewardPointsValue")]
        public int AddRewardPointsValue { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.AddRewardPointsMessage")]
        [AllowHtml]
        public string AddRewardPointsMessage { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.AddRewardPointsStore")]
        public int AddRewardPointsStoreId { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.AddRewardPointsStore")]
        public IList<SelectListItem> RewardPointsAvailableStores { get; set; }



        //send email model
        public SendEmailModel SendEmail { get; set; }
        //send PM model
        public SendPmModel SendPm { get; set; }
        //send the welcome message
        public bool AllowSendingOfWelcomeMessage { get; set; }
        //re-send the activation message
        public bool AllowReSendingOfActivationMessage { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.AssociatedExternalAuth")]
        public IList<AssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; }


        #region Nested classes

        public partial class StoreModel : BaseNopEntityModel
        {
            public string Name { get; set; }
        }

        public partial class AssociatedExternalAuthModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Users.Users.AssociatedExternalAuth.Fields.Email")]
            public string Email { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.AssociatedExternalAuth.Fields.ExternalIdentifier")]
            public string ExternalIdentifier { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.AssociatedExternalAuth.Fields.AuthMethodName")]
            public string AuthMethodName { get; set; }
        }

        public partial class RewardPointsHistoryModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.Store")]
            public string StoreName { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.Points")]
            public int Points { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.PointsBalance")]
            public string PointsBalance { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.Message")]
            [AllowHtml]
            public string Message { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.RewardPoints.Fields.Date")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class SendEmailModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Users.Users.SendEmail.Subject")]
            [AllowHtml]
            public string Subject { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.SendEmail.Body")]
            [AllowHtml]
            public string Body { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.SendEmail.SendImmediately")]
            public bool SendImmediately { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.SendEmail.DontSendBeforeDate")]
            [UIHint("DateTimeNullable")]
            public DateTime? DontSendBeforeDate { get; set; }
        }

        public partial class SendPmModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Users.Users.SendPM.Subject")]
            public string Subject { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.SendPM.Message")]
            public string Message { get; set; }
        }

        public partial class OrderModel : BaseNopEntityModel
        {
            public override int Id { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.Orders.CustomOrderNumber")]
            public string CustomOrderNumber { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.Orders.OrderStatus")]
            public string OrderStatus { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.Orders.OrderStatus")]
            public int OrderStatusId { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.Orders.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.Orders.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.Orders.OrderTotal")]
            public string OrderTotal { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.Orders.Store")]
            public string StoreName { get; set; }

            [NopResourceDisplayName("Admin.Users.Users.Orders.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class ActivityLogModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Users.Users.ActivityLog.ActivityLogType")]
            public string ActivityLogTypeName { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.ActivityLog.Comment")]
            public string Comment { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.ActivityLog.CreatedOn")]
            public DateTime CreatedOn { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.ActivityLog.IpAddress")]
            public string IpAddress { get; set; }
        }

        public partial class BackInStockSubscriptionModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Users.Users.BackInStockSubscriptions.Store")]
            public string StoreName { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.BackInStockSubscriptions.Product")]
            public int ProductId { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.BackInStockSubscriptions.Product")]
            public string ProductName { get; set; }
            [NopResourceDisplayName("Admin.Users.Users.BackInStockSubscriptions.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class UserAttributeModel : BaseNopEntityModel
        {
            public UserAttributeModel()
            {
                Values = new List<UserAttributeValueModel>();
            }

            public string Name { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<UserAttributeValueModel> Values { get; set; }

        }

        public partial class UserAttributeValueModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }

        #endregion
    }
}