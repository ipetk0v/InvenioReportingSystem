﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Settings
{
    public partial class CustomerUserSettingsModel : BaseNopModel
    {
        public CustomerUserSettingsModel()
        {
            UserSettings = new UserSettingsModel();
            AddressSettings = new AddressSettingsModel();
            DateTimeSettings = new DateTimeSettingsModel();
            ExternalAuthenticationSettings = new ExternalAuthenticationSettingsModel();
        }
        public UserSettingsModel UserSettings { get; set; }
        public AddressSettingsModel AddressSettings { get; set; }
        public DateTimeSettingsModel DateTimeSettings { get; set; }
        public ExternalAuthenticationSettingsModel ExternalAuthenticationSettings { get; set; }

        #region Nested classes

        public partial class UserSettingsModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UsernamesEnabled")]
            public bool UsernamesEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowUsersToChangeUsernames")]
            public bool AllowUsersToChangeUsernames { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CheckUsernameAvailabilityEnabled")]
            public bool CheckUsernameAvailabilityEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UserRegistrationType")]
            public int UserRegistrationType { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowUsersToUploadAvatars")]
            public bool AllowUsersToUploadAvatars { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultAvatarEnabled")]
            public bool DefaultAvatarEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ShowUsersLocation")]
            public bool ShowUsersLocation { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ShowUsersJoinDate")]
            public bool ShowUsersJoinDate { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowViewingProfiles")]
            public bool AllowViewingProfiles { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NotifyNewUserRegistration")]
            public bool NotifyNewUserRegistration { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.RequireRegistrationForDownloadableProducts")]
            public bool RequireRegistrationForDownloadableProducts { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideDownloadableProductsTab")]
            public bool HideDownloadableProductsTab { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideBackInStockSubscriptionsTab")]
            public bool HideBackInStockSubscriptionsTab { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UserNameFormat")]
            public int UserNameFormat { get; set; }
            
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PasswordMinLength")]
            public int PasswordMinLength { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UnduplicatedPasswordsNumber")]
            public int UnduplicatedPasswordsNumber { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PasswordRecoveryLinkDaysValid")]
            public int PasswordRecoveryLinkDaysValid { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultPasswordFormat")]
            public int DefaultPasswordFormat { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PasswordLifetime")]
            public int PasswordLifetime { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FailedPasswordAllowedAttempts")]
            public int FailedPasswordAllowedAttempts { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FailedPasswordLockoutMinutes")]
            public int FailedPasswordLockoutMinutes { get; set; }
            
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NewsletterEnabled")]
            public bool NewsletterEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NewsletterTickedByDefault")]
            public bool NewsletterTickedByDefault { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideNewsletterBlock")]
            public bool HideNewsletterBlock { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NewsletterBlockAllowToUnsubscribe")]
            public bool NewsletterBlockAllowToUnsubscribe { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StoreLastVisitedPage")]
            public bool StoreLastVisitedPage { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.EnteringEmailTwice")]
            public bool EnteringEmailTwice { get; set; }


            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.GenderEnabled")]
            public bool GenderEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DateOfBirthEnabled")]
            public bool DateOfBirthEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DateOfBirthRequired")]
            public bool DateOfBirthRequired { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DateOfBirthMinimumAge")]
            [UIHint("Int32Nullable")]
            public int? DateOfBirthMinimumAge { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CityEnabled")]
            public bool CityEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CityRequired")]
            public bool CityRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CountryEnabled")]
            public bool CountryEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CountryRequired")]
            public bool CountryRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StateProvinceRequired")]
            public bool StateProvinceRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxRequired")]
            public bool FaxRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AcceptPrivacyPolicyEnabled")]
            public bool AcceptPrivacyPolicyEnabled { get; set; }
        }

        public partial class AddressSettingsModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityEnabled")]
            public bool CityEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired")]
            public bool CityRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryEnabled")]
            public bool CountryEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired")]
            public bool FaxRequired { get; set; }
        }

        public partial class DateTimeSettingsModel : BaseNopModel
        {
            public DateTimeSettingsModel()
            {
                AvailableTimeZones = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowUsersToSetTimeZone")]
            public bool AllowUsersToSetTimeZone { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultStoreTimeZone")]
            public string DefaultStoreTimeZoneId { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultStoreTimeZone")]
            public IList<SelectListItem> AvailableTimeZones { get; set; }
        }

        public partial class ExternalAuthenticationSettingsModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ExternalAuthenticationAutoRegisterEnabled")]
            public bool AutoRegisterEnabled { get; set; }
        }
        #endregion
    }
}