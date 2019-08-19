using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Settings
{
    public partial class SupplierUserSettingsModel : BaseNopModel
    {
        public SupplierUserSettingsModel()
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
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.UsernamesEnabled")]
            public bool UsernamesEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AllowUsersToChangeUsernames")]
            public bool AllowUsersToChangeUsernames { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CheckUsernameAvailabilityEnabled")]
            public bool CheckUsernameAvailabilityEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.UserRegistrationType")]
            public int UserRegistrationType { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AllowUsersToUploadAvatars")]
            public bool AllowUsersToUploadAvatars { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DefaultAvatarEnabled")]
            public bool DefaultAvatarEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.ShowUsersLocation")]
            public bool ShowUsersLocation { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.ShowUsersJoinDate")]
            public bool ShowUsersJoinDate { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AllowViewingProfiles")]
            public bool AllowViewingProfiles { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.NotifyNewUserRegistration")]
            public bool NotifyNewUserRegistration { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.RequireRegistrationForDownloadableProducts")]
            public bool RequireRegistrationForDownloadableProducts { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.HideDownloadableProductsTab")]
            public bool HideDownloadableProductsTab { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.HideBackInStockSubscriptionsTab")]
            public bool HideBackInStockSubscriptionsTab { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.UserNameFormat")]
            public int UserNameFormat { get; set; }
            
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.PasswordMinLength")]
            public int PasswordMinLength { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.UnduplicatedPasswordsNumber")]
            public int UnduplicatedPasswordsNumber { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.PasswordRecoveryLinkDaysValid")]
            public int PasswordRecoveryLinkDaysValid { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DefaultPasswordFormat")]
            public int DefaultPasswordFormat { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.PasswordLifetime")]
            public int PasswordLifetime { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.FailedPasswordAllowedAttempts")]
            public int FailedPasswordAllowedAttempts { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.FailedPasswordLockoutMinutes")]
            public int FailedPasswordLockoutMinutes { get; set; }
            
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.NewsletterEnabled")]
            public bool NewsletterEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.NewsletterTickedByDefault")]
            public bool NewsletterTickedByDefault { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.HideNewsletterBlock")]
            public bool HideNewsletterBlock { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.NewsletterBlockAllowToUnsubscribe")]
            public bool NewsletterBlockAllowToUnsubscribe { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StoreLastVisitedPage")]
            public bool StoreLastVisitedPage { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.EnteringEmailTwice")]
            public bool EnteringEmailTwice { get; set; }


            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.GenderEnabled")]
            public bool GenderEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DateOfBirthEnabled")]
            public bool DateOfBirthEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DateOfBirthRequired")]
            public bool DateOfBirthRequired { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DateOfBirthMinimumAge")]
            [UIHint("Int32Nullable")]
            public int? DateOfBirthMinimumAge { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CityEnabled")]
            public bool CityEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CityRequired")]
            public bool CityRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CountryEnabled")]
            public bool CountryEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.CountryRequired")]
            public bool CountryRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.StateProvinceRequired")]
            public bool StateProvinceRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.FaxRequired")]
            public bool FaxRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AcceptPrivacyPolicyEnabled")]
            public bool AcceptPrivacyPolicyEnabled { get; set; }
        }

        public partial class AddressSettingsModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.CityEnabled")]
            public bool CityEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.CityRequired")]
            public bool CityRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.CountryEnabled")]
            public bool CountryEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AddressFormFields.FaxRequired")]
            public bool FaxRequired { get; set; }
        }

        public partial class DateTimeSettingsModel : BaseNopModel
        {
            public DateTimeSettingsModel()
            {
                AvailableTimeZones = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.AllowUsersToSetTimeZone")]
            public bool AllowUsersToSetTimeZone { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DefaultStoreTimeZone")]
            public string DefaultStoreTimeZoneId { get; set; }

            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.DefaultStoreTimeZone")]
            public IList<SelectListItem> AvailableTimeZones { get; set; }
        }

        public partial class ExternalAuthenticationSettingsModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Configuration.Settings.SupplierUser.ExternalAuthenticationAutoRegisterEnabled")]
            public bool AutoRegisterEnabled { get; set; }
        }
        #endregion
    }
}