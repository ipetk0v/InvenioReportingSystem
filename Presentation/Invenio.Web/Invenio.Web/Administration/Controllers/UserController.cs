using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Invenio.Admin.Extensions;
using Invenio.Admin.Helpers;
using Invenio.Admin.Models.Common;
using Invenio.Admin.Models.Users;
//using Invenio.Admin.Models.ShoppingCart;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
//using Invenio.Core.Domain.Forums;
using Invenio.Core.Domain.Messages;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Payments;
//using Invenio.Core.Domain.Shipping;
//using Invenio.Core.Domain.Tax;
using Invenio.Services;
//using Invenio.Services.Affiliates;
using Invenio.Services.Authentication.External;
using Invenio.Services.Catalog;
using Invenio.Services.Common;
using Invenio.Services.Users;
using Invenio.Services.Directory;
using Invenio.Services.ExportImport;
//using Invenio.Services.Forums;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Messages;
//using Invenio.Services.Orders;
using Invenio.Services.Security;
using Invenio.Services.Stores;
//using Invenio.Services.Tax;
//using Invenio.Services.Vendors;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Controllers
{
    public partial class UserController : BaseAdminController
    {
        #region Fields

        private readonly IUserService _UserService;
        //private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUserRegistrationService _UserRegistrationService;
        private readonly IUserReportService _UserReportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly DateTimeSettings _dateTimeSettings;
        //private readonly TaxSettings _taxSettings;
        //private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly UserSettings _UserSettings;
        //private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        //private readonly IVendorService _vendorService;
        private readonly IStoreContext _storeContext;
        //private readonly IPriceFormatter _priceFormatter;
        //private readonly IOrderService _orderService;
        private readonly IExportManager _exportManager;
        private readonly IUserActivityService _UserActivityService;
        //private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        //private readonly IPriceCalculationService _priceCalculationService;
        //private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IPermissionService _permissionService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        //private readonly ForumSettings _forumSettings;
        //private readonly IForumService _forumService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly AddressSettings _addressSettings;
        private readonly IStoreService _storeService;
        private readonly IUserAttributeParser _UserAttributeParser;
        private readonly IUserAttributeService _UserAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        //private readonly IAffiliateService _affiliateService;
        private readonly IWorkflowMessageService _workflowMessageService;
        //private readonly IRewardPointService _rewardPointService;
        private readonly ICacheManager _cacheManager;
        private readonly IManufacturerService _manufacturerService;

        #endregion

        #region Constructors

        public UserController(IUserService UserService,
            //INewsLetterSubscriptionService newsLetterSubscriptionService,
            IGenericAttributeService genericAttributeService,
            IUserRegistrationService UserRegistrationService,
            IUserReportService UserReportService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            DateTimeSettings dateTimeSettings,
            //TaxSettings taxSettings, 
            //RewardPointsSettings rewardPointsSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            UserSettings UserSettings,
            //ITaxService taxService, 
            IWorkContext workContext,
            //IVendorService vendorService,
            IStoreContext storeContext,
            //IPriceFormatter priceFormatter,
            //IOrderService orderService, 
            IExportManager exportManager,
            IUserActivityService UserActivityService,
            //IBackInStockSubscriptionService backInStockSubscriptionService,
            //IPriceCalculationService priceCalculationService,
            //IProductAttributeFormatter productAttributeFormatter,
            IPermissionService permissionService,
            IQueuedEmailService queuedEmailService,
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            //ForumSettings forumSettings,
            //IForumService forumService, 
            IOpenAuthenticationService openAuthenticationService,
            AddressSettings addressSettings,
            IStoreService storeService,
            IUserAttributeParser UserAttributeParser,
            IUserAttributeService UserAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            //IAffiliateService affiliateService,
            IWorkflowMessageService workflowMessageService,
            //IRewardPointService rewardPointService,
            ICacheManager cacheManager,
            IManufacturerService manufacturerService)
        {
            this._UserService = UserService;
            //this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._genericAttributeService = genericAttributeService;
            this._UserRegistrationService = UserRegistrationService;
            this._UserReportService = UserReportService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._dateTimeSettings = dateTimeSettings;
            //this._taxSettings = taxSettings;
            //this._rewardPointsSettings = rewardPointsSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._UserSettings = UserSettings;
            //this._taxService = taxService;
            this._workContext = workContext;
            //this._vendorService = vendorService;
            this._storeContext = storeContext;
            //this._priceFormatter = priceFormatter;
            //this._orderService = orderService;
            this._exportManager = exportManager;
            this._UserActivityService = UserActivityService;
            //this._backInStockSubscriptionService = backInStockSubscriptionService;
            //this._priceCalculationService = priceCalculationService;
            //this._productAttributeFormatter = productAttributeFormatter;
            this._permissionService = permissionService;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountSettings = emailAccountSettings;
            this._emailAccountService = emailAccountService;
            //this._forumSettings = forumSettings;
            //this._forumService = forumService;
            this._openAuthenticationService = openAuthenticationService;
            this._addressSettings = addressSettings;
            this._storeService = storeService;
            this._UserAttributeParser = UserAttributeParser;
            this._UserAttributeService = UserAttributeService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            //this._affiliateService = affiliateService;
            this._workflowMessageService = workflowMessageService;
            //this._rewardPointService = rewardPointService;
            this._cacheManager = cacheManager;
            this._manufacturerService = manufacturerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual string GetUserRolesNames(IList<UserRole> UserRoles, string separator = ",")
        {
            var sb = new StringBuilder();
            for (int i = 0; i < UserRoles.Count; i++)
            {
                sb.Append(UserRoles[i].Name);
                if (i != UserRoles.Count - 1)
                {
                    sb.Append(separator);
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        [NonAction]
        protected virtual IList<RegisteredUserReportLineModel> GetReportRegisteredUsersModel()
        {
            var report = new List<RegisteredUserReportLineModel>();
            report.Add(new RegisteredUserReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.7days"),
                Users = _UserReportService.GetRegisteredUsersReport(7)
            });

            report.Add(new RegisteredUserReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.14days"),
                Users = _UserReportService.GetRegisteredUsersReport(14)
            });
            report.Add(new RegisteredUserReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.month"),
                Users = _UserReportService.GetRegisteredUsersReport(30)
            });
            report.Add(new RegisteredUserReportLineModel
            {
                Period = _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.year"),
                Users = _UserReportService.GetRegisteredUsersReport(365)
            });

            return report;
        }

        [NonAction]
        protected virtual IList<UserModel.AssociatedExternalAuthModel> GetAssociatedExternalAuthRecords(User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            var result = new List<UserModel.AssociatedExternalAuthModel>();
            foreach (var record in _openAuthenticationService.GetExternalIdentifiersFor(User))
            {
                var method = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(record.ProviderSystemName);
                if (method == null)
                    continue;

                result.Add(new UserModel.AssociatedExternalAuthModel
                {
                    Id = record.Id,
                    Email = record.Email,
                    ExternalIdentifier = record.ExternalIdentifier,
                    //AuthMethodName = method.PluginDescriptor.FriendlyName
                });
            }

            return result;
        }

        [NonAction]
        protected virtual UserModel PrepareUserModelForList(User User)
        {
            return new UserModel
            {
                Id = User.Id,
                Email = User.IsRegistered() ? User.Email : _localizationService.GetResource("Admin.Users.Guest"),
                Username = User.Username,
                FullName = User.GetFullName(),
                Company = User.GetAttribute<string>(SystemUserAttributeNames.Company),
                Phone = User.GetAttribute<string>(SystemUserAttributeNames.Phone),
                ZipPostalCode = User.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode),
                UserRoleNames = GetUserRolesNames(User.UserRoles.ToList()),
                Active = User.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(User.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(User.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        [NonAction]
        protected virtual string ValidateUserRoles(IList<UserRole> UserRoles)
        {
            if (UserRoles == null)
                throw new ArgumentNullException("UserRoles");

            //ensure a User is not added to both 'Guests' and 'Registered' User roles
            //ensure that a User is in at least one required role ('Guests' and 'Registered')
            bool isInGuestsRole = UserRoles.FirstOrDefault(cr => cr.SystemName == SystemUserRoleNames.Guests) != null;
            bool isInRegisteredRole = UserRoles.FirstOrDefault(cr => cr.SystemName == SystemUserRoleNames.Registered) != null;
            if (isInGuestsRole && isInRegisteredRole)
                return _localizationService.GetResource("Admin.Users.Users.GuestsAndRegisteredRolesError");
            if (!isInGuestsRole && !isInRegisteredRole)
                return _localizationService.GetResource("Admin.Users.Users.AddUserToGuestsOrRegisteredRoleError");

            //no errors
            return "";
        }

        //[NonAction]
        //protected virtual void PrepareVendorsModel(UserModel model)
        //{
        //    if (model == null)
        //        throw new ArgumentNullException("model");

        //    model.AvailableVendors.Add(new SelectListItem
        //    {
        //        Text = _localizationService.GetResource("Admin.Users.Users.Fields.Vendor.None"),
        //        Value = "0"
        //    });
        //    var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
        //    foreach (var v in vendors)
        //        model.AvailableVendors.Add(v);
        //}

        [NonAction]
        protected virtual void PrepareUserAttributeModel(UserModel model, User User)
        {
            var UserAttributes = _UserAttributeService.GetAllUserAttributes();
            foreach (var attribute in UserAttributes)
            {
                var attributeModel = new UserModel.UserAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _UserAttributeService.GetUserAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new UserModel.UserAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }


                //set already selected attributes
                if (User != null)
                {
                    var selectedUserAttributes = User.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes, _genericAttributeService);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                            {
                                if (!String.IsNullOrEmpty(selectedUserAttributes))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = _UserAttributeParser.ParseUserAttributeValues(selectedUserAttributes);
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
                                if (!String.IsNullOrEmpty(selectedUserAttributes))
                                {
                                    var enteredText = _UserAttributeParser.ParseValues(selectedUserAttributes, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                        case AttributeControlType.FileUpload:
                        default:
                            //not supported attribute control types
                            break;
                    }
                }

                model.UserAttributes.Add(attributeModel);
            }
        }

        [NonAction]
        protected virtual string ParseCustomUserAttributes(FormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var UserAttributes = _UserAttributeService.GetAllUserAttributes();
            foreach (var attribute in UserAttributes)
            {
                string controlId = string.Format("User_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _UserAttributeParser.AddUserAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _UserAttributeParser.AddUserAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _UserAttributeService.GetUserAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _UserAttributeParser.AddUserAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _UserAttributeParser.AddUserAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported User attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        [NonAction]
        protected virtual void PrepareUserModel(UserModel model, User User, bool excludeProperties)
        {
            var allStores = _storeService.GetAllStores();
            if (User != null)
            {
                model.Id = User.Id;
                if (!excludeProperties)
                {
                    model.Email = User.Email;
                    model.Username = User.Username;
                    model.VendorId = User.VendorId;
                    model.AdminComment = User.AdminComment;
                    model.IsTaxExempt = User.IsTaxExempt;
                    model.Active = User.Active;

                    if (User.RegisteredInStoreId == 0 || allStores.All(s => s.Id != User.RegisteredInStoreId))
                        model.RegisteredInStore = string.Empty;
                    else
                        model.RegisteredInStore = allStores.First(s => s.Id == User.RegisteredInStoreId).Name;

                    //var affiliate = _affiliateService.GetAffiliateById(User.AffiliateId);
                    //if (affiliate != null)
                    //{
                    //    model.AffiliateId = affiliate.Id;
                    //    model.AffiliateName = affiliate.GetFullName();
                    //}

                    model.TimeZoneId = User.GetAttribute<string>(SystemUserAttributeNames.TimeZoneId);
                    model.VatNumber = User.GetAttribute<string>(SystemUserAttributeNames.VatNumber);
                    //model.VatNumberStatusNote = ((VatNumberStatus)User.GetAttribute<int>(SystemUserAttributeNames.VatNumberStatusId))
                    //    .GetLocalizedEnum(_localizationService, _workContext);
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(User.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(User.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = User.LastIpAddress;
                    model.LastVisitedPage = User.GetAttribute<string>(SystemUserAttributeNames.LastVisitedPage);

                    model.SelectedUserRoleIds = User.UserRoles.Select(cr => cr.Id).ToList();

                    model.SelectedManufacturerIds = User.Manufacturers.Select(x => x.Id).ToList();

                    model.SelectedManufacturerRegionIds = User.ManufacturerRegions.Select(x => x.Id).ToList();

                    //newsletter subscriptions
                    if (!String.IsNullOrEmpty(User.Email))
                    {
                        var newsletterSubscriptionStoreIds = new List<int>();
                        foreach (var store in allStores)
                        {
                            //var newsletterSubscription = _newsLetterSubscriptionService
                            //    .GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, store.Id);
                            //if (newsletterSubscription != null)
                            //    newsletterSubscriptionStoreIds.Add(store.Id);
                            model.SelectedNewsletterSubscriptionStoreIds = newsletterSubscriptionStoreIds.ToArray();
                        }
                    }

                    //form fields
                    model.FirstName = User.GetAttribute<string>(SystemUserAttributeNames.FirstName);
                    model.LastName = User.GetAttribute<string>(SystemUserAttributeNames.LastName);
                    model.Gender = User.GetAttribute<string>(SystemUserAttributeNames.Gender);
                    model.DateOfBirth = User.GetAttribute<DateTime?>(SystemUserAttributeNames.DateOfBirth);
                    model.Company = User.GetAttribute<string>(SystemUserAttributeNames.Company);
                    model.StreetAddress = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress);
                    model.StreetAddress2 = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress2);
                    model.ZipPostalCode = User.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode);
                    model.City = User.GetAttribute<string>(SystemUserAttributeNames.City);
                    model.CountryId = User.GetAttribute<int>(SystemUserAttributeNames.CountryId);
                    model.StateProvinceId = User.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId);
                    model.Phone = User.GetAttribute<string>(SystemUserAttributeNames.Phone);
                    model.Fax = User.GetAttribute<string>(SystemUserAttributeNames.Fax);
                }
            }

            model.UsernamesEnabled = _UserSettings.UsernamesEnabled;
            model.AllowUsersToSetTimeZone = _dateTimeSettings.AllowUsersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (tzi.Id == model.TimeZoneId) });
            if (User != null)
            {
                //model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            }
            else
            {
                model.DisplayVatNumber = false;
            }

            //vendors
            //PrepareVendorsModel(model);
            //User attributes
            PrepareUserAttributeModel(model, User);

            model.GenderEnabled = _UserSettings.GenderEnabled;
            model.DateOfBirthEnabled = _UserSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _UserSettings.CompanyEnabled;
            model.StreetAddressEnabled = _UserSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _UserSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _UserSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _UserSettings.CityEnabled;
            model.CountryEnabled = _UserSettings.CountryEnabled;
            model.StateProvinceEnabled = _UserSettings.StateProvinceEnabled;
            model.PhoneEnabled = _UserSettings.PhoneEnabled;
            model.FaxEnabled = _UserSettings.FaxEnabled;

            //countries and states
            if (_UserSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries(showHidden: true))
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_UserSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Admin.Address.OtherNonUS" : "Admin.Address.SelectState"),
                            Value = "0"
                        });
                    }
                }
            }

            //newsletter subscriptions
            model.AvailableNewsletterSubscriptionStores = allStores
                .Select(s => new UserModel.StoreModel() { Id = s.Id, Name = s.Name })
                .ToList();

            //User roles
            var allRoles = _UserService.GetAllUserRoles(true);
            var adminRole = allRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered);
            //precheck Registered Role as a default role while creating a new User through admin
            if (User == null && adminRole != null)
            {
                model.SelectedUserRoleIds.Add(adminRole.Id);
            }
            foreach (var role in allRoles)
            {
                model.AvailableUserRoles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = model.SelectedUserRoleIds.Contains(role.Id)
                });
            }

            //manufacturer - trqbva da ima filtyr sprqmo rolqta
            foreach (var man in _manufacturerService.GetAllManufacturers())
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Text = man.Name,
                    Value = man.Id.ToString(),
                    Selected = model.SelectedManufacturerIds.Contains(model.Id)
                });
            }

            //manufacturer region
            var stateProvinces = _manufacturerService.GetAllManufacturers().Select(x => x.Country.StateProvinces).Distinct();
            foreach (var stateProvince in stateProvinces)
            {
                foreach (var region in stateProvince)
                {
                    model.AvailableManufacturerRegions.Add(new SelectListItem
                    {
                        Text = region.Name,
                        Value = region.Id.ToString(),
                        Selected = model.SelectedManufacturerRegionIds.Contains(model.Id)
                    });
                }
            }

            //reward points history
            if (User != null)
            {
                //model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
                model.AddRewardPointsValue = 0;
                model.AddRewardPointsMessage = "Some comment here...";

                //stores
                foreach (var store in allStores)
                {
                    model.RewardPointsAvailableStores.Add(new SelectListItem
                    {
                        Text = store.Name,
                        Value = store.Id.ToString(),
                        Selected = (store.Id == _storeContext.CurrentStore.Id)
                    });
                }
            }
            else
            {
                model.DisplayRewardPointsHistory = false;
            }
            //external authentication records
            if (User != null)
            {
                model.AssociatedExternalAuthRecords = GetAssociatedExternalAuthRecords(User);
            }
            //sending of the welcome message:
            //1. "admin approval" registration method
            //2. already created User
            //3. registered
            model.AllowSendingOfWelcomeMessage = _UserSettings.UserRegistrationType == UserRegistrationType.AdminApproval &&
                User != null &&
                User.IsRegistered();
            //sending of the activation message
            //1. "email validation" registration method
            //2. already created User
            //3. registered
            //4. not active
            model.AllowReSendingOfActivationMessage = _UserSettings.UserRegistrationType == UserRegistrationType.EmailValidation &&
                User != null &&
                User.IsRegistered() &&
                !User.Active;
        }

        [NonAction]
        protected virtual void PrepareAddressModel(UserAddressModel model, Address address, User User, bool excludeProperties)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            model.UserId = User.Id;
            if (address != null)
            {
                if (!excludeProperties)
                {
                    model.Address = address.ToModel();
                }
            }

            if (model.Address == null)
                model.Address = new AddressModel();

            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.CountryRequired = _addressSettings.CountryEnabled; //country is required when enabled
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;
            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            //User attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);
        }

        [NonAction]
        private bool SecondAdminAccountExists(User User)
        {
            var Users = _UserService.GetAllUsers(UserRoleIds: new[] { _UserService.GetUserRoleBySystemName(SystemUserRoleNames.Administrators).Id });

            return Users.Any(c => c.Active && c.Id != User.Id);
        }
        #endregion

        #region Users

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            //load registered Users by default
            var defaultRoleIds = new List<int> { _UserService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id };
            var model = new UserListModel
            {
                UsernamesEnabled = _UserSettings.UsernamesEnabled,
                DateOfBirthEnabled = _UserSettings.DateOfBirthEnabled,
                CompanyEnabled = _UserSettings.CompanyEnabled,
                PhoneEnabled = _UserSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _UserSettings.ZipPostalCodeEnabled,
                SearchUserRoleIds = defaultRoleIds,
            };
            var allRoles = _UserService.GetAllUserRoles(true);
            foreach (var role in allRoles)
            {
                model.AvailableUserRoles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = defaultRoleIds.Any(x => x == role.Id)
                });
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult UserList(DataSourceRequest command, UserListModel model,
            [ModelBinder(typeof(CommaSeparatedModelBinder))] int[] searchUserRoleIds)
        {
            //we use own own binder for searchUserRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var searchDayOfBirth = 0;
            int searchMonthOfBirth = 0;
            if (!String.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!String.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var Users = _UserService.GetAllUsers(
                UserRoleIds: searchUserRoleIds,
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                dayOfBirth: searchDayOfBirth,
                monthOfBirth: searchMonthOfBirth,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                ipAddress: model.SearchIpAddress,
                loadOnlyWithShoppingCart: false,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = Users.Select(PrepareUserModelForList),
                Total = Users.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var model = new UserModel();
            PrepareUserModel(model, null, false);
            //default value
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Create(UserModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                var cust2 = _UserService.GetUserByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }
            if (!String.IsNullOrWhiteSpace(model.Username) & _UserSettings.UsernamesEnabled)
            {
                var cust2 = _UserService.GetUserByUsername(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "Username is already registered");
            }

            //validate User roles
            var allUserRoles = _UserService.GetAllUserRoles(true);
            var newUserRoles = new List<UserRole>();
            foreach (var UserRole in allUserRoles)
                if (model.SelectedUserRoleIds.Contains(UserRole.Id))
                    newUserRoles.Add(UserRole);
            var UserRolesError = ValidateUserRoles(newUserRoles);
            if (!String.IsNullOrEmpty(UserRolesError))
            {
                ModelState.AddModelError("", UserRolesError);
                ErrorNotification(UserRolesError, false);
            }

            // Ensure that valid email address is entered if Registered role is checked to avoid registered Users with empty email address
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null && !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Admin.Users.Users.ValidEmailRequiredRegisteredRole"));
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.ValidEmailRequiredRegisteredRole"), false);
            }

            //custom User attributes
            var UserAttributesXml = ParseCustomUserAttributes(form);
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null)
            {
                var UserAttributeWarnings = _UserAttributeParser.GetAttributeWarnings(UserAttributesXml);
                foreach (var error in UserAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ErrorNotification(_localizationService.GetResource("Account.Login.Fields.Password.Required"));
            }

            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Password))
            {
                var User = new User
                {
                    UserGuid = Guid.NewGuid(),
                    Email = model.Email,
                    Username = model.Username,
                    VendorId = model.VendorId,
                    AdminComment = model.AdminComment,
                    IsTaxExempt = model.IsTaxExempt,
                    Active = model.Active,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    RegisteredInStoreId = _storeContext.CurrentStore.Id
                };
                _UserService.InsertUser(User);

                //form fields
                if (_dateTimeSettings.AllowUsersToSetTimeZone)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.TimeZoneId, model.TimeZoneId);
                if (_UserSettings.GenderEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Gender, model.Gender);
                _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.FirstName, model.FirstName);
                _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.LastName, model.LastName);
                if (_UserSettings.DateOfBirthEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.DateOfBirth, model.DateOfBirth);
                if (_UserSettings.CompanyEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Company, model.Company);
                if (_UserSettings.StreetAddressEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StreetAddress, model.StreetAddress);
                if (_UserSettings.StreetAddress2Enabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StreetAddress2, model.StreetAddress2);
                if (_UserSettings.ZipPostalCodeEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.ZipPostalCode, model.ZipPostalCode);
                if (_UserSettings.CityEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.City, model.City);
                if (_UserSettings.CountryEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.CountryId, model.CountryId);
                if (_UserSettings.CountryEnabled && _UserSettings.StateProvinceEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StateProvinceId, model.StateProvinceId);
                if (_UserSettings.PhoneEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Phone, model.Phone);
                if (_UserSettings.FaxEnabled)
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Fax, model.Fax);

                //custom User attributes
                _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.CustomUserAttributes, UserAttributesXml);


                //newsletter subscriptions
                //if (!String.IsNullOrEmpty(User.Email))
                //{
                //    var allStores = _storeService.GetAllStores();
                //    foreach (var store in allStores)
                //    {
                //        //var newsletterSubscription = _newsLetterSubscriptionService
                //        //    .GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, store.Id);
                //        if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                //            model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                //        {
                //            //subscribed
                //            //if (newsletterSubscription == null)
                //            //{
                //            //    _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                //            //    {
                //            //        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                //            //        Email = User.Email,
                //            //        Active = true,
                //            //        StoreId = store.Id,
                //            //        CreatedOnUtc = DateTime.UtcNow
                //            //    });
                //            //}
                //        }
                //        else
                //        {
                //            //not subscribed
                //            if (newsletterSubscription != null)
                //            {
                //                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                //            }
                //        }
                //    }
                //}

                //password
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _UserSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = _UserRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                            ErrorNotification(changePassError);
                    }
                }

                //User roles
                foreach (var userRole in newUserRoles)
                {
                    //ensure that the current User cannot add to "Administrators" system role if he's not an admin himself
                    if (userRole.SystemName == SystemUserRoleNames.Administrators &&
                        !_workContext.CurrentUser.IsAdmin())
                        continue;

                    User.UserRoles.Add(userRole);
                }
                _UserService.UpdateUser(User);

                foreach (var manufacturerId in model.SelectedManufacturerIds)
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);

                    if (manufacturer != null)
                    {
                        User.Manufacturers.Add(manufacturer);
                    }
                }
                _UserService.UpdateUser(User);

                foreach (var stateProvinceId in model.SelectedManufacturerRegionIds)
                {
                    var region = _stateProvinceService.GetStateProvinceById(stateProvinceId);

                    if (region != null)
                    {
                        User.ManufacturerRegions.Add(region);
                    }
                }
                _UserService.UpdateUser(User);

                //ensure that a User with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to other functionality in admin area
                if (User.IsAdmin() && User.VendorId > 0)
                {
                    User.VendorId = 0;
                    _UserService.UpdateUser(User);
                    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminCouldNotbeVendor"));
                }

                //ensure that a User in the Vendors role has a vendor account associated.
                //otherwise, he will have access to ALL products
                //if (User.IsVendor() && User.VendorId == 0)
                //{
                //    var vendorRole = User
                //        .UserRoles
                //        .FirstOrDefault(x => x.SystemName == SystemUserRoleNames.Vendors);
                //    User.UserRoles.Remove(vendorRole);
                //    _UserService.UpdateUser(User);
                //    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.CannotBeInVendoRoleWithoutVendorAssociated"));
                //}

                //activity log
                _UserActivityService.InsertActivity("AddNewUser", _localizationService.GetResource("ActivityLog.AddNewUser"), User.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = User.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareUserModel(model, null, true);
            return View(model);
        }

        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(id);
            if (User == null || User.Deleted)
                //No User found with the specified id
                return RedirectToAction("List");

            var model = new UserModel();
            PrepareUserModel(model, User, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Edit(UserModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null || User.Deleted)
                //No User found with the specified id
                return RedirectToAction("List");

            //validate User roles
            var allUserRoles = _UserService.GetAllUserRoles(true);
            var newUserRoles = new List<UserRole>();
            foreach (var UserRole in allUserRoles)
                if (model.SelectedUserRoleIds.Contains(UserRole.Id))
                    newUserRoles.Add(UserRole);
            var UserRolesError = ValidateUserRoles(newUserRoles);
            if (!String.IsNullOrEmpty(UserRolesError))
            {
                ModelState.AddModelError("", UserRolesError);
                ErrorNotification(UserRolesError, false);
            }

            // Ensure that valid email address is entered if Registered role is checked to avoid registered Users with empty email address
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null && !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Admin.Users.Users.ValidEmailRequiredRegisteredRole"));
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.ValidEmailRequiredRegisteredRole"), false);
            }

            //custom User attributes
            var UserAttributesXml = ParseCustomUserAttributes(form);
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null)
            {
                var UserAttributeWarnings = _UserAttributeParser.GetAttributeWarnings(UserAttributesXml);
                foreach (var error in UserAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    User.AdminComment = model.AdminComment;
                    User.IsTaxExempt = model.IsTaxExempt;

                    //prevent deactivation of the last active administrator
                    if (!User.IsAdmin() || model.Active || SecondAdminAccountExists(User))
                        User.Active = model.Active;
                    else
                        ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminAccountShouldExists.Deactivate"));

                    //email
                    if (!String.IsNullOrWhiteSpace(model.Email))
                    {
                        _UserRegistrationService.SetEmail(User, model.Email, false);
                    }
                    else
                    {
                        User.Email = model.Email;
                    }

                    //username
                    if (_UserSettings.UsernamesEnabled)
                    {
                        if (!String.IsNullOrWhiteSpace(model.Username))
                        {
                            _UserRegistrationService.SetUsername(User, model.Username);
                        }
                        else
                        {
                            User.Username = model.Username;
                        }
                    }

                    //VAT number
                    //if (_taxSettings.EuVatEnabled)
                    //{
                    //    var prevVatNumber = User.GetAttribute<string>(SystemUserAttributeNames.VatNumber);

                    //    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.VatNumber, model.VatNumber);
                    //    //set VAT number status
                    //    if (!String.IsNullOrEmpty(model.VatNumber))
                    //    {
                    //        if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                    //        {
                    //            _genericAttributeService.SaveAttribute(User, 
                    //                SystemUserAttributeNames.VatNumberStatusId, 
                    //                (int)_taxService.GetVatNumberStatus(model.VatNumber));
                    //        }
                    //    }
                    //    else
                    //    {
                    //        _genericAttributeService.SaveAttribute(User,
                    //            SystemUserAttributeNames.VatNumberStatusId, 
                    //            (int)VatNumberStatus.Empty);
                    //    }
                    //}

                    //vendor
                    //User.VendorId = model.VendorId;

                    //form fields
                    if (_dateTimeSettings.AllowUsersToSetTimeZone)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.TimeZoneId, model.TimeZoneId);
                    if (_UserSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Gender, model.Gender);
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.LastName, model.LastName);
                    if (_UserSettings.DateOfBirthEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.DateOfBirth, model.DateOfBirth);
                    if (_UserSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Company, model.Company);
                    if (_UserSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StreetAddress, model.StreetAddress);
                    if (_UserSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_UserSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_UserSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.City, model.City);
                    if (_UserSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.CountryId, model.CountryId);
                    if (_UserSettings.CountryEnabled && _UserSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_UserSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Phone, model.Phone);
                    if (_UserSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Fax, model.Fax);

                    //custom User attributes
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.CustomUserAttributes, UserAttributesXml);

                    //newsletter subscriptions
                    //if (!String.IsNullOrEmpty(User.Email))
                    //{
                    //    var allStores = _storeService.GetAllStores();
                    //    foreach (var store in allStores)
                    //    {
                    //        var newsletterSubscription = _newsLetterSubscriptionService
                    //            .GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, store.Id);
                    //        if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                    //            model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    //        {
                    //            //subscribed
                    //            if (newsletterSubscription == null)
                    //            {
                    //                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                    //                {
                    //                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    //                    Email = User.Email,
                    //                    Active = true,
                    //                    StoreId = store.Id,
                    //                    CreatedOnUtc = DateTime.UtcNow
                    //                });
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //not subscribed
                    //            if (newsletterSubscription != null)
                    //            {
                    //                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletterSubscription);
                    //            }
                    //        }
                    //    }
                    //}

                    if (model.SelectedManufacturerIds.Any())
                    {
                        User.Manufacturers.Clear();
                        foreach (var manufacturerId in model.SelectedManufacturerIds)
                        {
                            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);

                            if (manufacturer != null)
                            {
                                User.Manufacturers.Add(manufacturer);
                            }
                        }
                        _UserService.UpdateUser(User);
                    }
                    else
                    {
                        User.Manufacturers.Clear();
                        _UserService.UpdateUser(User);
                    }

                    if (model.SelectedManufacturerRegionIds.Any())
                    {
                        User.ManufacturerRegions.Clear();
                        foreach (var regionId in model.SelectedManufacturerRegionIds)
                        {
                            var region = _stateProvinceService.GetStateProvinceById(regionId);

                            if (region != null)
                            {
                                User.ManufacturerRegions.Add(region);
                            }
                        }
                        _UserService.UpdateUser(User);
                    }
                    else
                    {
                        User.ManufacturerRegions.Clear();
                        _UserService.UpdateUser(User);
                    }

                    //User roles
                    foreach (var UserRole in allUserRoles)
                    {
                        //ensure that the current User cannot add/remove to/from "Administrators" system role
                        //if he's not an admin himself
                        if (UserRole.SystemName == SystemUserRoleNames.Administrators &&
                            !_workContext.CurrentUser.IsAdmin())
                            continue;

                        if (model.SelectedUserRoleIds.Contains(UserRole.Id))
                        {
                            //new role
                            if (User.UserRoles.Count(cr => cr.Id == UserRole.Id) == 0)
                                User.UserRoles.Add(UserRole);
                        }
                        else
                        {
                            //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                            if (UserRole.SystemName == SystemUserRoleNames.Administrators && !SecondAdminAccountExists(User))
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminAccountShouldExists.DeleteRole"));
                                continue;
                            }

                            //remove role
                            if (User.UserRoles.Count(cr => cr.Id == UserRole.Id) > 0)
                                User.UserRoles.Remove(UserRole);
                        }
                    }
                    _UserService.UpdateUser(User);


                    //ensure that a User with a vendor associated is not in "Administrators" role
                    //otherwise, he won't have access to the other functionality in admin area
                    if (User.IsAdmin() && User.VendorId > 0)
                    {
                        User.VendorId = 0;
                        _UserService.UpdateUser(User);
                        ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminCouldNotbeVendor"));
                    }

                    //ensure that a User in the Vendors role has a vendor account associated.
                    //otherwise, he will have access to ALL products
                    //if (User.IsVendor() && User.VendorId == 0)
                    //{
                    //    var vendorRole = User
                    //        .UserRoles
                    //        .FirstOrDefault(x => x.SystemName == SystemUserRoleNames.Vendors);
                    //    User.UserRoles.Remove(vendorRole);
                    //    _UserService.UpdateUser(User);
                    //    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.CannotBeInVendoRoleWithoutVendorAssociated"));
                    //}


                    //activity log
                    _UserActivityService.InsertActivity("EditUser", _localizationService.GetResource("ActivityLog.EditUser"), User.Id);

                    SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        SaveSelectedTabName();

                        return RedirectToAction("Edit", new { id = User.Id });
                    }
                    return RedirectToAction("List");
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }


            //If we got this far, something failed, redisplay form
            PrepareUserModel(model, User, true);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual ActionResult ChangePassword(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //ensure that the current User cannot change passwords of "Administrators" if he's not an admin himself
            if (User.IsAdmin() && !_workContext.CurrentUser.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.OnlyAdminCanChangePassword"));
                return RedirectToAction("Edit", new { id = User.Id });
            }

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                    false, _UserSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _UserRegistrationService.ChangePassword(changePassRequest);
                if (changePassResult.Success)
                    SuccessNotification(_localizationService.GetResource("Admin.Users.Users.PasswordChanged"));
                else
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
            }

            return RedirectToAction("Edit", new { id = User.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public virtual ActionResult MarkVatNumberAsValid(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //_genericAttributeService.SaveAttribute(User, 
            //    SystemUserAttributeNames.VatNumberStatusId,
            //    (int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", new { id = User.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public virtual ActionResult MarkVatNumberAsInvalid(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //_genericAttributeService.SaveAttribute(User,
            //    SystemUserAttributeNames.VatNumberStatusId,
            //    (int)VatNumberStatus.Invalid);

            return RedirectToAction("Edit", new { id = User.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("remove-affiliate")]
        public virtual ActionResult RemoveAffiliate(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            User.AffiliateId = 0;
            _UserService.UpdateUser(User);

            return RedirectToAction("Edit", new { id = User.Id });
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (User.IsAdmin() && !SecondAdminAccountExists(User))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminAccountShouldExists.DeleteAdministrator"));
                    return RedirectToAction("Edit", new { id = User.Id });
                }

                //ensure that the current User cannot delete "Administrators" if he's not an admin himself
                if (User.IsAdmin() && !_workContext.CurrentUser.IsAdmin())
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.OnlyAdminCanDeleteAdmin"));
                    return RedirectToAction("Edit", new { id = User.Id });
                }

                //delete
                _UserService.DeleteUser(User);

                //remove newsletter subscription (if exists)
                //foreach (var store in _storeService.GetAllStores())
                //{
                //    var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, store.Id);
                //    if (subscription != null)
                //        _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
                //}

                //activity log
                _UserActivityService.InsertActivity("DeleteUser", _localizationService.GetResource("ActivityLog.DeleteUser"), User.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = User.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public virtual ActionResult Impersonate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AllowUserImpersonation))
                return AccessDeniedView();

            var User = _UserService.GetUserById(id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!_workContext.CurrentUser.IsAdmin() && User.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.NonAdminNotImpersonateAsAdminError"));
                return RedirectToAction("Edit", User.Id);
            }

            //activity log
            _UserActivityService.InsertActivity("Impersonation.Started", _localizationService.GetResource("ActivityLog.Impersonation.Started.StoreOwner"), User.Email, User.Id);
            _UserActivityService.InsertActivity(User, "Impersonation.Started", _localizationService.GetResource("ActivityLog.Impersonation.Started.User"), _workContext.CurrentUser.Email, _workContext.CurrentUser.Id);

            //ensure login is not required
            User.RequireReLogin = false;
            _UserService.UpdateUser(User);
            _genericAttributeService.SaveAttribute<int?>(_workContext.CurrentUser, SystemUserAttributeNames.ImpersonatedUserId, User.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public virtual ActionResult SendWelcomeMessage(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            _workflowMessageService.SendUserWelcomeMessage(User, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Users.Users.SendWelcomeMessage.Success"));

            return RedirectToAction("Edit", new { id = User.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public virtual ActionResult ReSendActivationMessage(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //email validation message
            _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
            _workflowMessageService.SendUserEmailValidationMessage(User, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Users.Users.ReSendActivationMessage.Success"));

            return RedirectToAction("Edit", new { id = User.Id });
        }

        public virtual ActionResult SendEmail(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.Id);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            try
            {
                if (String.IsNullOrWhiteSpace(User.Email))
                    throw new InvenioException("User email is empty");
                if (!CommonHelper.IsValidEmail(User.Email))
                    throw new InvenioException("User email is not valid");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new InvenioException("Email subject is empty");
                if (String.IsNullOrWhiteSpace(model.SendEmail.Body))
                    throw new InvenioException("Email body is empty");

                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                if (emailAccount == null)
                    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
                if (emailAccount == null)
                    throw new InvenioException("Email account can't be loaded");
                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.High,
                    EmailAccountId = emailAccount.Id,
                    FromName = emailAccount.DisplayName,
                    From = emailAccount.Email,
                    ToName = User.GetFullName(),
                    To = User.Email,
                    Subject = model.SendEmail.Subject,
                    Body = model.SendEmail.Body,
                    CreatedOnUtc = DateTime.UtcNow,
                    DontSendBeforeDateUtc = (model.SendEmail.SendImmediately || !model.SendEmail.DontSendBeforeDate.HasValue) ?
                        null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SendEmail.DontSendBeforeDate.Value)
                };
                _queuedEmailService.InsertQueuedEmail(email);
                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.SendEmail.Queued"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
            }

            return RedirectToAction("Edit", new { id = User.Id });
        }

        //public virtual ActionResult SendPm(UserModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedView();

        //    var User = _UserService.GetUserById(model.Id);
        //    if (User == null)
        //        //No User found with the specified id
        //        return RedirectToAction("List");

        //    try
        //    {
        //        if (!_forumSettings.AllowPrivateMessages)
        //            throw new InvenioException("Private messages are disabled");
        //        if (User.IsGuest())
        //            throw new InvenioException("User should be registered");
        //        if (String.IsNullOrWhiteSpace(model.SendPm.Subject))
        //            throw new InvenioException("PM subject is empty");
        //        if (String.IsNullOrWhiteSpace(model.SendPm.Message))
        //            throw new InvenioException("PM message is empty");


        //        var privateMessage = new PrivateMessage
        //        {
        //            StoreId = _storeContext.CurrentStore.Id,
        //            ToUserId = User.Id,
        //            FromUserId = _workContext.CurrentUser.Id,
        //            Subject = model.SendPm.Subject,
        //            Text = model.SendPm.Message,
        //            IsDeletedByAuthor = false,
        //            IsDeletedByRecipient = false,
        //            IsRead = false,
        //            CreatedOnUtc = DateTime.UtcNow
        //        };

        //        _forumService.InsertPrivateMessage(privateMessage);
        //        SuccessNotification(_localizationService.GetResource("Admin.Users.Users.SendPM.Sent"));
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc.Message);
        //    }

        //    return RedirectToAction("Edit", new { id = User.Id });
        //}

        #endregion

        //#region Reward points history

        //[HttpPost]
        //public virtual ActionResult RewardPointsHistorySelect(DataSourceRequest command, int UserId)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedKendoGridJson();

        //    var User = _UserService.GetUserById(UserId);
        //    if (User == null)
        //        throw new ArgumentException("No User found with the specified id");

        //    var rewardPoints = _rewardPointService.GetRewardPointsHistory(User.Id, true, true, command.Page - 1, command.PageSize);
        //    var gridModel = new DataSourceResult
        //    {
        //        Data = rewardPoints.Select(rph =>
        //        {
        //            var store = _storeService.GetStoreById(rph.StoreId);
        //            var activatingDate = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc);

        //            return new UserModel.RewardPointsHistoryModel
        //            {
        //                StoreName = store != null ? store.Name : "Unknown",
        //                Points = rph.Points,
        //                PointsBalance = rph.PointsBalance.HasValue ? rph.PointsBalance.ToString()
        //                    : string.Format(_localizationService.GetResource("Admin.Users.Users.RewardPoints.ActivatedLater"), activatingDate),
        //                Message = rph.Message,
        //                CreatedOn = activatingDate
        //            };
        //        }),
        //        Total = rewardPoints.TotalCount
        //    };

        //    return Json(gridModel);
        //}

        //[ValidateInput(false)]
        //public virtual ActionResult RewardPointsHistoryAdd(int UserId, int storeId, int addRewardPointsValue, string addRewardPointsMessage)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedView();

        //    var User = _UserService.GetUserById(UserId);
        //    if (User == null)
        //        return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

        //    _rewardPointService.AddRewardPointsHistoryEntry(User,
        //        addRewardPointsValue, storeId, addRewardPointsMessage);

        //    return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        //}

        //#endregion

        #region Addresses

        [HttpPost]
        public virtual ActionResult AddressesSelect(int UserId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var User = _UserService.GetUserById(UserId);
            if (User == null)
                throw new ArgumentException("No User found with the specified id", "UserId");

            var addresses = User.Addresses.OrderByDescending(a => a.CreatedOnUtc).ThenByDescending(a => a.Id).ToList();
            var gridModel = new DataSourceResult
            {
                Data = addresses.Select(x =>
                {
                    var model = x.ToModel();
                    var addressHtmlSb = new StringBuilder("<div>");
                    if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(model.Company))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.Company));
                    if (_addressSettings.StreetAddressEnabled && !String.IsNullOrEmpty(model.Address1))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(model.Address2))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.Address2));
                    if (_addressSettings.CityEnabled && !String.IsNullOrEmpty(model.City))
                        addressHtmlSb.AppendFormat("{0},", Server.HtmlEncode(model.City));
                    if (_addressSettings.StateProvinceEnabled && !String.IsNullOrEmpty(model.StateProvinceName))
                        addressHtmlSb.AppendFormat("{0},", Server.HtmlEncode(model.StateProvinceName));
                    if (_addressSettings.ZipPostalCodeEnabled && !String.IsNullOrEmpty(model.ZipPostalCode))
                        addressHtmlSb.AppendFormat("{0}<br />", Server.HtmlEncode(model.ZipPostalCode));
                    if (_addressSettings.CountryEnabled && !String.IsNullOrEmpty(model.CountryName))
                        addressHtmlSb.AppendFormat("{0}", Server.HtmlEncode(model.CountryName));
                    var customAttributesFormatted = _addressAttributeFormatter.FormatAttributes(x.CustomAttributes);
                    if (!String.IsNullOrEmpty(customAttributesFormatted))
                    {
                        //already encoded
                        addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
                    }
                    addressHtmlSb.Append("</div>");
                    model.AddressHtml = addressHtmlSb.ToString();
                    return model;
                }),
                Total = addresses.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public virtual ActionResult AddressDelete(int id, int UserId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(UserId);
            if (User == null)
                throw new ArgumentException("No User found with the specified id", "UserId");

            var address = User.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
                //No User found with the specified id
                return Content("No User found with the specified id");
            User.RemoveAddress(address);
            _UserService.UpdateUser(User);
            //now delete the address record
            _addressService.DeleteAddress(address);

            return new NullJsonResult();
        }

        public virtual ActionResult AddressCreate(int UserId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(UserId);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            var model = new UserAddressModel();
            PrepareAddressModel(model, null, User, false);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult AddressCreate(UserAddressModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.UserId);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                User.Addresses.Add(address);
                _UserService.UpdateUser(User);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Addresses.Added"));
                return RedirectToAction("AddressEdit", new { addressId = address.Id, UserId = model.UserId });
            }

            //If we got this far, something failed, redisplay form
            PrepareAddressModel(model, null, User, true);
            return View(model);
        }

        public virtual ActionResult AddressEdit(int addressId, int UserId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(UserId);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = User.Id });

            var model = new UserAddressModel();
            PrepareAddressModel(model, address, User, false);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult AddressEdit(UserAddressModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var User = _UserService.GetUserById(model.UserId);
            if (User == null)
                //No User found with the specified id
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = User.Id });

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, UserId = model.UserId });
            }

            //If we got this far, something failed, redisplay form
            PrepareAddressModel(model, address, User, true);

            return View(model);
        }

        #endregion

        //#region Orders

        //[HttpPost]
        //public virtual ActionResult OrderList(int UserId, DataSourceRequest command)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedKendoGridJson();

        //    var orders = _orderService.SearchOrders(UserId: UserId);

        //    var gridModel = new DataSourceResult
        //    {
        //        Data = orders.PagedForCommand(command)
        //            .Select(order =>
        //            {
        //                var store = _storeService.GetStoreById(order.StoreId);
        //                var orderModel = new UserModel.OrderModel
        //                {
        //                    Id = order.Id, 
        //                    OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
        //                    OrderStatusId = order.OrderStatusId,
        //                    PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
        //                    ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
        //                    OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false),
        //                    StoreName = store != null ? store.Name : "Unknown",
        //                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
        //                    CustomOrderNumber = order.CustomOrderNumber
        //                };
        //                return orderModel;
        //            }),
        //        Total = orders.Count
        //    };


        //    return Json(gridModel);
        //}

        //#endregion

        //   #region Reports

        //   public virtual ActionResult Reports()
        //   {
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //           return AccessDeniedView();

        //       var model = new UserReportsModel();
        //       //Users by number of orders
        //       model.BestUsersByNumberOfOrders = new BestUsersReportModel();
        //       model.BestUsersByNumberOfOrders.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
        //       model.BestUsersByNumberOfOrders.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //       model.BestUsersByNumberOfOrders.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
        //       model.BestUsersByNumberOfOrders.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //       model.BestUsersByNumberOfOrders.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
        //       model.BestUsersByNumberOfOrders.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

        //       //Users by order total
        //       model.BestUsersByOrderTotal = new BestUsersReportModel();
        //       model.BestUsersByOrderTotal.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
        //       model.BestUsersByOrderTotal.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //       model.BestUsersByOrderTotal.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
        //       model.BestUsersByOrderTotal.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
        //       model.BestUsersByOrderTotal.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
        //       model.BestUsersByOrderTotal.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

        //       return View(model);
        //   }

        //   [HttpPost]
        //   public virtual ActionResult ReportBestUsersByOrderTotalList(DataSourceRequest command, BestUsersReportModel model)
        //   {
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //           return AccessDeniedKendoGridJson();

        //       DateTime? startDateValue = (model.StartDate == null) ? null
        //                       : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

        //       DateTime? endDateValue = (model.EndDate == null) ? null
        //                       : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

        //       OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
        //       PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
        //       ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;


        //       var items = _UserReportService.GetBestUsersReport(startDateValue, endDateValue,
        //           orderStatus, paymentStatus, shippingStatus, 1, command.Page - 1, command.PageSize);
        //       var gridModel = new DataSourceResult
        //       {
        //           Data = items.Select(x =>
        //           {
        //               var m = new BestUserReportLineModel
        //               {
        //                   UserId = x.UserId,
        //                   OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
        //                   OrderCount = x.OrderCount,
        //               };
        //               var User = _UserService.GetUserById(x.UserId);
        //               if (User != null)
        //               {
        //                   m.UserName = User.IsRegistered() ? User.Email : _localizationService.GetResource("Admin.Users.Guest");
        //               }
        //               return m;
        //           }),
        //           Total = items.TotalCount
        //       };

        //       return Json(gridModel);
        //   }
        //   [HttpPost]
        //   public virtual ActionResult ReportBestUsersByNumberOfOrdersList(DataSourceRequest command, BestUsersReportModel model)
        //   {
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //           return AccessDeniedKendoGridJson();

        //       DateTime? startDateValue = (model.StartDate == null) ? null
        //                       : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

        //       DateTime? endDateValue = (model.EndDate == null) ? null
        //                       : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

        //       OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
        //       PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
        //       ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;


        //       var items = _UserReportService.GetBestUsersReport(startDateValue, endDateValue,
        //           orderStatus, paymentStatus, shippingStatus, 2, command.Page - 1, command.PageSize);
        //       var gridModel = new DataSourceResult
        //       {
        //           Data = items.Select(x =>
        //           {
        //               var m = new BestUserReportLineModel
        //               {
        //                   UserId = x.UserId,
        //                   OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
        //                   OrderCount = x.OrderCount,
        //               };
        //               var User = _UserService.GetUserById(x.UserId);
        //               if (User != null)
        //               {
        //                   m.UserName = User.IsRegistered() ? User.Email : _localizationService.GetResource("Admin.Users.Guest");
        //               }
        //               return m;
        //           }),
        //           Total = items.TotalCount
        //       };

        //       return Json(gridModel);
        //   }

        //   [ChildActionOnly]
        //   public virtual ActionResult ReportRegisteredUsers()
        //   {
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //           return Content("");

        //       return PartialView();
        //   }

        //   [HttpPost]
        //   public virtual ActionResult ReportRegisteredUsersList(DataSourceRequest command)
        //   {
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //           return AccessDeniedKendoGridJson();

        //       var model = GetReportRegisteredUsersModel();
        //       var gridModel = new DataSourceResult
        //       {
        //           Data = model,
        //           Total = model.Count
        //       };

        //       return Json(gridModel);
        //   }

        //   [ChildActionOnly]
        //public virtual ActionResult UserStatistics()
        //{
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
        //           return Content("");

        //       //a vendor doesn't have access to this report
        //       if (_workContext.CurrentVendor != null)
        //           return Content("");

        //       return PartialView();
        //}

        //   [AcceptVerbs(HttpVerbs.Get)]
        //   public virtual ActionResult LoadUserStatistics(string period)
        //   {
        //       if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //           return Content("");

        //       var result = new List<object>();

        //       var nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
        //       var timeZone = _dateTimeHelper.CurrentTimeZone;
        //       var searchUserRoleIds = new[] { _UserService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id };

        //       var culture = new CultureInfo(_workContext.WorkingLanguage.LanguageCulture);

        //       switch (period)
        //       {
        //           case "year":
        //               //year statistics
        //               var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
        //               var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
        //               if (!timeZone.IsInvalidTime(searchYearDateUser))
        //               {
        //                   for (int i = 0; i <= 12; i++)
        //                   {
        //                       result.Add(new
        //                       {
        //                           date = searchYearDateUser.Date.ToString("Y", culture),
        //                           value = _UserService.GetAllUsers(
        //                               createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
        //                               createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
        //                               UserRoleIds: searchUserRoleIds,
        //                               pageIndex: 0,
        //                               pageSize: 1).TotalCount.ToString()
        //                       });

        //                       searchYearDateUser = searchYearDateUser.AddMonths(1);
        //                   }
        //               }
        //               break;

        //           case "month":
        //               //month statistics
        //               var monthAgoDt = nowDt.AddDays(-30);
        //               var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
        //               if (!timeZone.IsInvalidTime(searchMonthDateUser))
        //               {
        //                   for (int i = 0; i <= 30; i++)
        //                   {
        //                       result.Add(new
        //                       {
        //                           date = searchMonthDateUser.Date.ToString("M", culture),
        //                           value = _UserService.GetAllUsers(
        //                               createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
        //                               createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
        //                               UserRoleIds: searchUserRoleIds,
        //                               pageIndex: 0,
        //                               pageSize: 1).TotalCount.ToString()
        //                       });

        //                       searchMonthDateUser = searchMonthDateUser.AddDays(1);
        //                   }
        //               }
        //               break;

        //           case "week":
        //           default:
        //               //week statistics
        //               var weekAgoDt = nowDt.AddDays(-7);
        //               var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
        //               if (!timeZone.IsInvalidTime(searchWeekDateUser))
        //               {
        //                   for (int i = 0; i <= 7; i++)
        //                   {
        //                       result.Add(new
        //                       {
        //                           date = searchWeekDateUser.Date.ToString("d dddd", culture),
        //                           value = _UserService.GetAllUsers(
        //                               createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
        //                               createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
        //                               UserRoleIds: searchUserRoleIds,
        //                               pageIndex: 0,
        //                               pageSize: 1).TotalCount.ToString()
        //                       });

        //                       searchWeekDateUser = searchWeekDateUser.AddDays(1);
        //                   }
        //               }
        //               break;
        //       }

        //       return Json(result, JsonRequestBehavior.AllowGet);
        //   }

        //   #endregion

        //#region Current shopping cart/ wishlist

        //[HttpPost]
        //public virtual ActionResult GetCartList(int UserId, int cartTypeId)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedKendoGridJson();

        //    var User = _UserService.GetUserById(UserId);
        //    var cart = User.ShoppingCartItems.Where(x => x.ShoppingCartTypeId == cartTypeId).ToList();

        //    var gridModel = new DataSourceResult
        //    {
        //        Data = cart.Select(sci =>
        //        {
        //            decimal taxRate;
        //            var store = _storeService.GetStoreById(sci.StoreId); 
        //            var sciModel = new ShoppingCartItemModel
        //            {
        //                Id = sci.Id,
        //                Store = store != null ? store.Name : "Unknown",
        //                ProductId = sci.ProductId,
        //                Quantity = sci.Quantity,
        //                ProductName = sci.Product.Name,
        //                AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
        //                UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci), out taxRate)),
        //                Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci), out taxRate)),
        //                UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
        //            };
        //            return sciModel;
        //        }),
        //        Total = cart.Count
        //    };

        //    return Json(gridModel);
        //}

        //#endregion

        #region Activity log

        [HttpPost]
        public virtual ActionResult ListActivityLog(DataSourceRequest command, int UserId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var activityLog = _UserActivityService.GetAllActivities(null, null, UserId, 0, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var m = new UserModel.ActivityLogModel
                    {
                        Id = x.Id,
                        ActivityLogTypeName = x.ActivityLogType.Name,
                        Comment = x.Comment,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                        IpAddress = x.IpAddress
                    };
                    return m;

                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        //#region Back in stock subscriptions

        //[HttpPost]
        //public virtual ActionResult BackInStockSubscriptionList(DataSourceRequest command, int UserId)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
        //        return AccessDeniedKendoGridJson();

        //    var subscriptions = _backInStockSubscriptionService.GetAllSubscriptionsByUserId(UserId,
        //        0, command.Page - 1, command.PageSize);
        //    var gridModel = new DataSourceResult
        //    {
        //        Data = subscriptions.Select(x =>
        //        {
        //            var store = _storeService.GetStoreById(x.StoreId);
        //            var product = x.Product;
        //            var m = new UserModel.BackInStockSubscriptionModel
        //            {
        //                Id = x.Id,
        //                StoreName = store != null ? store.Name : "Unknown",
        //                ProductId = x.ProductId,
        //                ProductName = product != null ? product.Name : "Unknown",
        //                CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
        //            };
        //            return m;

        //        }),
        //        Total = subscriptions.TotalCount
        //    };

        //    return Json(gridModel);
        //}

        //#endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public virtual ActionResult ExportExcelAll(UserListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var searchDayOfBirth = 0;
            int searchMonthOfBirth = 0;
            if (!String.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!String.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var Users = _UserService.GetAllUsers(
                UserRoleIds: model.SearchUserRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                dayOfBirth: searchDayOfBirth,
                monthOfBirth: searchMonthOfBirth,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false);

            try
            {
                byte[] bytes = _exportManager.ExportUsersToXlsx(Users);
                return File(bytes, MimeTypes.TextXlsx, "Users.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual ActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var Users = new List<User>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                Users.AddRange(_UserService.GetUsersByIds(ids));
            }

            try
            {
                byte[] bytes = _exportManager.ExportUsersToXlsx(Users);
                return File(bytes, MimeTypes.TextXlsx, "Users.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public virtual ActionResult ExportXmlAll(UserListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var searchDayOfBirth = 0;
            int searchMonthOfBirth = 0;
            if (!String.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!String.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var Users = _UserService.GetAllUsers(
                UserRoleIds: model.SearchUserRoleIds.ToArray(),
                email: model.SearchEmail,
                username: model.SearchUsername,
                firstName: model.SearchFirstName,
                lastName: model.SearchLastName,
                dayOfBirth: searchDayOfBirth,
                monthOfBirth: searchMonthOfBirth,
                company: model.SearchCompany,
                phone: model.SearchPhone,
                zipPostalCode: model.SearchZipPostalCode,
                loadOnlyWithShoppingCart: false);

            try
            {
                var xml = _exportManager.ExportUsersToXml(Users);
                return new XmlDownloadResult(xml, "Users.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual ActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var Users = new List<User>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                Users.AddRange(_UserService.GetUsersByIds(ids));
            }

            var xml = _exportManager.ExportUsersToXml(Users);
            return new XmlDownloadResult(xml, "Users.xml");
        }

        #endregion
    }
}
