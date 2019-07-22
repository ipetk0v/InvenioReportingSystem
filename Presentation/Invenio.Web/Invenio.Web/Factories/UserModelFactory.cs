using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Forums;
using Invenio.Core.Domain.Media;
//using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Security;
//using Invenio.Core.Domain.Tax;
//using Invenio.Core.Domain.Vendors;
using Invenio.Services.Authentication.External;
using Invenio.Services.Common;
using Invenio.Services.Users;
using Invenio.Services.Directory;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Media;
using Invenio.Services.Messages;
//using Invenio.Services.Orders;
//using Invenio.Services.Seo;
using Invenio.Services.Stores;
using Invenio.Web.Framework.Security.Captcha;
using Invenio.Web.Models.Common;
using Invenio.Web.Models.User;
using WebGrease.Css.Extensions;

namespace Invenio.Web.Factories
{
    /// <summary>
    /// Represents the User model factory
    /// </summary>
    public partial class UserModelFactory : IUserModelFactory
    {
        #region Fields

        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly DateTimeSettings _dateTimeSettings;
        //private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUserAttributeParser _UserAttributeParser;
        private readonly IUserAttributeService _UserAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        //private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly UserSettings _UserSettings;
        private readonly AddressSettings _addressSettings;
        //private readonly ForumSettings _forumSettings;
        //private readonly OrderSettings _orderSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        //private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        //private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IDownloadService _downloadService;
        //private readonly IReturnRequestService _returnRequestService;

        private readonly MediaSettings _mediaSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        //private readonly CatalogSettings _catalogSettings;
        //private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public UserModelFactory(IAddressModelFactory addressModelFactory, 
            IDateTimeHelper dateTimeHelper,
            DateTimeSettings dateTimeSettings, 
            //TaxSettings taxSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUserAttributeParser UserAttributeParser,
            IUserAttributeService UserAttributeService,
            IGenericAttributeService genericAttributeService,
            //RewardPointsSettings rewardPointsSettings,
            UserSettings UserSettings,
            AddressSettings addressSettings, 
            //ForumSettings forumSettings,
            //OrderSettings orderSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            //IOrderService orderService,
            IPictureService pictureService, 
            //INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOpenAuthenticationService openAuthenticationService,
            IDownloadService downloadService,
            //IReturnRequestService returnRequestService,
            MediaSettings mediaSettings,
            CaptchaSettings captchaSettings,
            SecuritySettings securitySettings, 
            ExternalAuthenticationSettings externalAuthenticationSettings
            //CatalogSettings catalogSettings, 
            //VendorSettings vendorSettings
            )
        {
            this._addressModelFactory = addressModelFactory;
            this._dateTimeHelper = dateTimeHelper;
            this._dateTimeSettings = dateTimeSettings;
            //this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._UserAttributeParser = UserAttributeParser;
            this._UserAttributeService = UserAttributeService;
            this._genericAttributeService = genericAttributeService;
            //this._rewardPointsSettings = rewardPointsSettings;
            this._UserSettings = UserSettings;
            this._addressSettings = addressSettings;
            //this._forumSettings = forumSettings;
            //this._orderSettings = orderSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            //this._orderService = orderService;
            this._pictureService = pictureService;
            //this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._openAuthenticationService = openAuthenticationService;
            this._downloadService = downloadService;
            //this._returnRequestService = returnRequestService;
            this._mediaSettings = mediaSettings;
            this._captchaSettings = captchaSettings;
            this._securitySettings = securitySettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            //this._catalogSettings = catalogSettings;
            //this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the custom User attribute models
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="overrideAttributesXml">Overridden User attributes in XML format; pass null to use CustomUserAttributes of User</param>
        /// <returns>List of the User attribute model</returns>
        public virtual IList<UserAttributeModel> PrepareCustomUserAttributes(User User, string overrideAttributesXml = "")
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var result = new List<UserAttributeModel>();

            var UserAttributes = _UserAttributeService.GetAllUserAttributes();
            foreach (var attribute in UserAttributes)
            {
                var attributeModel = new UserAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _UserAttributeService.GetUserAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new UserAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                //set already selected attributes
                var selectedAttributesXml = !String.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml : 
                    User.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes, _genericAttributeService);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedAttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = _UserAttributeParser.ParseUserAttributeValues(selectedAttributesXml);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedAttributesXml))
                            {
                                var enteredText = _UserAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                result.Add(attributeModel);
            }


            return result;
        }

        /// <summary>
        /// Prepare the User info model
        /// </summary>
        /// <param name="model">User info model</param>
        /// <param name="User">User</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomUserAttributesXml">Overridden User attributes in XML format; pass null to use CustomUserAttributes of User</param>
        /// <returns>User info model</returns>
        public virtual UserInfoModel PrepareUserInfoModel(UserInfoModel model, User User, 
            bool excludeProperties, string overrideCustomUserAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (User == null)
                throw new ArgumentNullException("User");

            model.AllowUsersToSetTimeZone = _dateTimeSettings.AllowUsersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = User.GetAttribute<string>(SystemUserAttributeNames.VatNumber);
                model.FirstName = User.GetAttribute<string>(SystemUserAttributeNames.FirstName);
                model.LastName = User.GetAttribute<string>(SystemUserAttributeNames.LastName);
                model.Gender = User.GetAttribute<string>(SystemUserAttributeNames.Gender);
                var dateOfBirth = User.GetAttribute<DateTime?>(SystemUserAttributeNames.DateOfBirth);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = User.GetAttribute<string>(SystemUserAttributeNames.Company);
                model.StreetAddress = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress);
                model.StreetAddress2 = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress2);
                model.ZipPostalCode = User.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode);
                model.City = User.GetAttribute<string>(SystemUserAttributeNames.City);
                model.CountryId = User.GetAttribute<int>(SystemUserAttributeNames.CountryId);
                model.StateProvinceId = User.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId);
                model.Phone = User.GetAttribute<string>(SystemUserAttributeNames.Phone);
                model.Fax = User.GetAttribute<string>(SystemUserAttributeNames.Fax);

                //newsletter
                //var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, _storeContext.CurrentStore.Id);
                //model.Newsletter = newsletter != null && newsletter.Active;

                model.Signature = User.GetAttribute<string>(SystemUserAttributeNames.Signature);

                model.Email = User.Email;
                model.Username = User.Username;
            }
            else
            {
                if (_UserSettings.UsernamesEnabled && !_UserSettings.AllowUsersToChangeUsernames)
                    model.Username = User.Username;
            }

            if (_UserSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
                model.EmailToRevalidate = User.EmailToRevalidate;

            //countries and states
            if (_UserSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_UserSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = "0"
                        });
                    }

                }
            }
            //model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //model.VatNumberStatusNote = ((VatNumberStatus)User.GetAttribute<int>(SystemUserAttributeNames.VatNumberStatusId))
                //.GetLocalizedEnum(_localizationService, _workContext);
            model.GenderEnabled = _UserSettings.GenderEnabled;
            model.DateOfBirthEnabled = _UserSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _UserSettings.DateOfBirthRequired;
            model.CompanyEnabled = _UserSettings.CompanyEnabled;
            model.CompanyRequired = _UserSettings.CompanyRequired;
            model.StreetAddressEnabled = _UserSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _UserSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _UserSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _UserSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _UserSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _UserSettings.ZipPostalCodeRequired;
            model.CityEnabled = _UserSettings.CityEnabled;
            model.CityRequired = _UserSettings.CityRequired;
            model.CountryEnabled = _UserSettings.CountryEnabled;
            model.CountryRequired = _UserSettings.CountryRequired;
            model.StateProvinceEnabled = _UserSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _UserSettings.StateProvinceRequired;
            model.PhoneEnabled = _UserSettings.PhoneEnabled;
            model.PhoneRequired = _UserSettings.PhoneRequired;
            model.FaxEnabled = _UserSettings.FaxEnabled;
            model.FaxRequired = _UserSettings.FaxRequired;
            model.NewsletterEnabled = _UserSettings.NewsletterEnabled;
            model.UsernamesEnabled = _UserSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _UserSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _UserSettings.CheckUsernameAvailabilityEnabled;
            //model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;

            //external authentication
            model.NumberOfExternalAuthenticationProviders = _openAuthenticationService
                .LoadActiveExternalAuthenticationMethods(_workContext.CurrentUser, _storeContext.CurrentStore.Id).Count;
            foreach (var ear in _openAuthenticationService.GetExternalIdentifiersFor(User))
            {
                var authMethod = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(ear.ProviderSystemName);
                if (authMethod == null || !authMethod.IsMethodActive(_externalAuthenticationSettings))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new UserInfoModel.AssociatedExternalAuthModel
                {
                    Id = ear.Id,
                    Email = ear.Email,
                    ExternalIdentifier = ear.ExternalIdentifier,
                    //AuthMethodName = authMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id)
                });
            }

            //custom User attributes
            var customAttributes = PrepareCustomUserAttributes(User, overrideCustomUserAttributesXml);
            customAttributes.ForEach(model.UserAttributes.Add);

            return model;
        }

        /// <summary>
        /// Prepare the User register model
        /// </summary>
        /// <param name="model">User register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomUserAttributesXml">Overridden User attributes in XML format; pass null to use CustomUserAttributes of User</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>User register model</returns>
        public virtual RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties, 
            string overrideCustomUserAttributesXml = "", bool setDefaultValues = false)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AllowUsersToSetTimeZone = _dateTimeSettings.AllowUsersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });
            
            //model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.GenderEnabled = _UserSettings.GenderEnabled;
            model.DateOfBirthEnabled = _UserSettings.DateOfBirthEnabled;
            model.DateOfBirthRequired = _UserSettings.DateOfBirthRequired;
            model.CompanyEnabled = _UserSettings.CompanyEnabled;
            model.CompanyRequired = _UserSettings.CompanyRequired;
            model.StreetAddressEnabled = _UserSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _UserSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _UserSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _UserSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _UserSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _UserSettings.ZipPostalCodeRequired;
            model.CityEnabled = _UserSettings.CityEnabled;
            model.CityRequired = _UserSettings.CityRequired;
            model.CountryEnabled = _UserSettings.CountryEnabled;
            model.CountryRequired = _UserSettings.CountryRequired;
            model.StateProvinceEnabled = _UserSettings.StateProvinceEnabled;
            model.StateProvinceRequired = _UserSettings.StateProvinceRequired;
            model.PhoneEnabled = _UserSettings.PhoneEnabled;
            model.PhoneRequired = _UserSettings.PhoneRequired;
            model.FaxEnabled = _UserSettings.FaxEnabled;
            model.FaxRequired = _UserSettings.FaxRequired;
            model.NewsletterEnabled = _UserSettings.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = _UserSettings.AcceptPrivacyPolicyEnabled;
            model.UsernamesEnabled = _UserSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _UserSettings.CheckUsernameAvailabilityEnabled;
            model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;
            model.EnteringEmailTwice = _UserSettings.EnteringEmailTwice;
            if (setDefaultValues)
            {
                //enable newsletter by default
                model.Newsletter = _UserSettings.NewsletterTickedByDefault;
            }

            //countries and states
            if (_UserSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });

                foreach (var c in _countryService.GetAllCountries(_workContext.WorkingLanguage.Id))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_UserSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, _workContext.WorkingLanguage.Id).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"), 
                            Value = "0"
                        });
                    }

                }
            }

            //custom User attributes
            var customAttributes = PrepareCustomUserAttributes(_workContext.CurrentUser, overrideCustomUserAttributesXml);
            customAttributes.ForEach(model.UserAttributes.Add);

            return model;
        }

        /// <summary>
        /// Prepare the login model
        /// </summary>
        /// <param name="checkoutAsGuest">Whether to checkout as guest is enabled</param>
        /// <returns>Login model</returns>
        public virtual LoginModel PrepareLoginModel(bool? checkoutAsGuest)
        {
            var model = new LoginModel();
            model.UsernamesEnabled = _UserSettings.UsernamesEnabled;
            model.CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault();
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage;
            return model;
        }

        /// <summary>
        /// Prepare the password recovery model
        /// </summary>
        /// <returns>Password recovery model</returns>
        public virtual PasswordRecoveryModel PreparePasswordRecoveryModel()
        {
            var model = new PasswordRecoveryModel();
            return model;
        }

        /// <summary>
        /// Prepare the password recovery confirm model
        /// </summary>
        /// <returns>Password recovery confirm model</returns>
        public virtual PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel()
        {
            var model = new PasswordRecoveryConfirmModel();
            return model;
        }

        /// <summary>
        /// Prepare the register result model
        /// </summary>
        /// <param name="resultId">Value of UserRegistrationType enum</param>
        /// <returns>Register result model</returns>
        public virtual RegisterResultModel PrepareRegisterResultModel(int resultId)
        {
            var resultText = "";
            switch ((UserRegistrationType)resultId)
            {
                case UserRegistrationType.Disabled:
                    resultText = _localizationService.GetResource("Account.Register.Result.Disabled");
                    break;
                case UserRegistrationType.Standard:
                    resultText = _localizationService.GetResource("Account.Register.Result.Standard");
                    break;
                case UserRegistrationType.AdminApproval:
                    resultText = _localizationService.GetResource("Account.Register.Result.AdminApproval");
                    break;
                case UserRegistrationType.EmailValidation:
                    resultText = _localizationService.GetResource("Account.Register.Result.EmailValidation");
                    break;
                default:
                    break;
            }
            var model = new RegisterResultModel
            {
                Result = resultText
            };
            return model;
        }

        /// <summary>
        /// Prepare the User navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>User navigation model</returns>
        public virtual UserNavigationModel PrepareUserNavigationModel(int selectedTabId = 0)
        {
            var model = new UserNavigationModel();

            model.UserNavigationItems.Add(new UserNavigationItemModel
            {
                RouteName = "UserInfo",
                Title = _localizationService.GetResource("Account.UserInfo"),
                Tab = UserNavigationEnum.Info,
                ItemClass = "User-info"
            });

            model.UserNavigationItems.Add(new UserNavigationItemModel
            {
                RouteName = "UserAddresses",
                Title = _localizationService.GetResource("Account.UserAddresses"),
                Tab = UserNavigationEnum.Addresses,
                ItemClass = "User-addresses"
            });

            model.UserNavigationItems.Add(new UserNavigationItemModel
            {
                RouteName = "UserOrders",
                Title = _localizationService.GetResource("Account.UserOrders"),
                Tab = UserNavigationEnum.Orders,
                ItemClass = "User-orders"
            });

            //if (_orderSettings.ReturnRequestsEnabled &&
            //    _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id,
            //        _workContext.CurrentUser.Id, pageIndex: 0, pageSize: 1).Any())
            //{
            //    model.UserNavigationItems.Add(new UserNavigationItemModel
            //    {
            //        RouteName = "UserReturnRequests",
            //        Title = _localizationService.GetResource("Account.UserReturnRequests"),
            //        Tab = UserNavigationEnum.ReturnRequests,
            //        ItemClass = "return-requests"
            //    });
            //}

            if (!_UserSettings.HideDownloadableProductsTab)
            {
                model.UserNavigationItems.Add(new UserNavigationItemModel
                {
                    RouteName = "UserDownloadableProducts",
                    Title = _localizationService.GetResource("Account.DownloadableProducts"),
                    Tab = UserNavigationEnum.DownloadableProducts,
                    ItemClass = "downloadable-products"
                });
            }

            if (!_UserSettings.HideBackInStockSubscriptionsTab)
            {
                model.UserNavigationItems.Add(new UserNavigationItemModel
                {
                    RouteName = "UserBackInStockSubscriptions",
                    Title = _localizationService.GetResource("Account.BackInStockSubscriptions"),
                    Tab = UserNavigationEnum.BackInStockSubscriptions,
                    ItemClass = "back-in-stock-subscriptions"
                });
            }

            //if (_rewardPointsSettings.Enabled)
            //{
            //    model.UserNavigationItems.Add(new UserNavigationItemModel
            //    {
            //        RouteName = "UserRewardPoints",
            //        Title = _localizationService.GetResource("Account.RewardPoints"),
            //        Tab = UserNavigationEnum.RewardPoints,
            //        ItemClass = "reward-points"
            //    });
            //}

            model.UserNavigationItems.Add(new UserNavigationItemModel
            {
                RouteName = "UserChangePassword",
                Title = _localizationService.GetResource("Account.ChangePassword"),
                Tab = UserNavigationEnum.ChangePassword,
                ItemClass = "change-password"
            });

            if (_UserSettings.AllowUsersToUploadAvatars)
            {
                model.UserNavigationItems.Add(new UserNavigationItemModel
                {
                    RouteName = "UserAvatar",
                    Title = _localizationService.GetResource("Account.Avatar"),
                    Tab = UserNavigationEnum.Avatar,
                    ItemClass = "User-avatar"
                });
            }

            //if (_forumSettings.ForumsEnabled && _forumSettings.AllowUsersToManageSubscriptions)
            //{
            //    model.UserNavigationItems.Add(new UserNavigationItemModel
            //    {
            //        RouteName = "UserForumSubscriptions",
            //        Title = _localizationService.GetResource("Account.ForumSubscriptions"),
            //        Tab = UserNavigationEnum.ForumSubscriptions,
            //        ItemClass = "forum-subscriptions"
            //    });
            //}
            //if (_catalogSettings.ShowProductReviewsTabOnAccountPage)
            //{
            //    model.UserNavigationItems.Add(new UserNavigationItemModel
            //    {
            //        RouteName = "UserProductReviews",
            //        Title = _localizationService.GetResource("Account.UserProductReviews"),
            //        Tab = UserNavigationEnum.ProductReviews,
            //        ItemClass = "User-reviews"
            //    });
            //}
            //if (_vendorSettings.AllowVendorsToEditInfo && _workContext.CurrentVendor != null)
            //{
            //    model.UserNavigationItems.Add(new UserNavigationItemModel
            //    {
            //        RouteName = "UserVendorInfo",
            //        Title = _localizationService.GetResource("Account.VendorInfo"),
            //        Tab = UserNavigationEnum.VendorInfo,
            //        ItemClass = "User-vendor-info"
            //    });
            //}

            model.SelectedTab = (UserNavigationEnum)selectedTabId;

            return model;
        }

        /// <summary>
        /// Prepare the User address list model
        /// </summary>
        /// <returns>User address list model</returns>
        public virtual UserAddressListModel PrepareUserAddressListModel()
        {
            var addresses = _workContext.CurrentUser.Addresses
                //enabled for the current store
                .Where(a => a.Country == null/* || _storeMappingService.Authorize(a.Country)*/)
                .ToList();

            var model = new UserAddressListModel();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressModelFactory.PrepareAddressModel(addressModel,
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));
                model.Addresses.Add(addressModel);
            }
            return model;
        }

        /// <summary>
        /// Prepare the User downloadable products model
        /// </summary>
        /// <returns>User downloadable products model</returns>
        //public virtual UserDownloadableProductsModel PrepareUserDownloadableProductsModel()
        //{
        //    var model = new UserDownloadableProductsModel();
        //    var items = _orderService.GetDownloadableOrderItems(_workContext.CurrentUser.Id);
        //    foreach (var item in items)
        //    {
        //        var itemModel = new UserDownloadableProductsModel.DownloadableProductsModel
        //        {
        //            OrderItemGuid = item.OrderItemGuid,
        //            OrderId = item.OrderId,
        //            CustomOrderNumber = item.Order.CustomOrderNumber,
        //            CreatedOn = _dateTimeHelper.ConvertToUserTime(item.Order.CreatedOnUtc, DateTimeKind.Utc),
        //            ProductName = item.Product.GetLocalized(x => x.Name),
        //            ProductSeName = item.Product.GetSeName(),
        //            ProductAttributes = item.AttributeDescription,
        //            ProductId = item.ProductId
        //        };
        //        model.Items.Add(itemModel);

        //        if (_downloadService.IsDownloadAllowed(item))
        //            itemModel.DownloadId = item.Product.DownloadId;

        //        if (_downloadService.IsLicenseDownloadAllowed(item))
        //            itemModel.LicenseId = item.LicenseDownloadId.HasValue ? item.LicenseDownloadId.Value : 0;
        //    }

        //    return model;
        //}

        /// <summary>
        /// Prepare the user agreement model
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product</param>
        /// <returns>User agreement model</returns>
        //public virtual UserAgreementModel PrepareUserAgreementModel(OrderItem orderItem,Product product)
        //{
        //    if (orderItem == null)
        //        throw new ArgumentNullException("orderItem");

        //    if (product == null)
        //        throw new ArgumentNullException("product");

        //    var model = new UserAgreementModel();
        //    model.UserAgreementText = product.UserAgreementText;
        //    model.OrderItemGuid = orderItem.OrderItemGuid;

        //    return model;
        //}

        /// <summary>
        /// Prepare the change password model
        /// </summary>
        /// <returns>Change password model</returns>
        public virtual ChangePasswordModel PrepareChangePasswordModel()
        {
            var model = new ChangePasswordModel();
            return model;
        }

        /// <summary>
        /// Prepare the User avatar model
        /// </summary>
        /// <param name="model">User avatar model</param>
        /// <returns>User avatar model</returns>
        public virtual UserAvatarModel PrepareUserAvatarModel(UserAvatarModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvatarUrl = _pictureService.GetPictureUrl(
                _workContext.CurrentUser.GetAttribute<int>(SystemUserAttributeNames.AvatarPictureId),
                _mediaSettings.AvatarPictureSize,
                false);

            return model;
        }

        #endregion
    }
}
