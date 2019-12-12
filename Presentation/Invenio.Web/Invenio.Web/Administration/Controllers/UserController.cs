using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Common;
using Invenio.Admin.Models.Users;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Messages;
using Invenio.Core.Domain.Users;
using Invenio.Services.Authentication.External;
using Invenio.Services.Common;
using Invenio.Services.Customers;
using Invenio.Services.Directory;
using Invenio.Services.ExportImport;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Messages;
using Invenio.Services.Security;
using Invenio.Services.Stores;
using Invenio.Services.Users;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Invenio.Core.Domain.Orders;

namespace Invenio.Admin.Controllers
{
    public partial class UserController : BaseAdminController
    {
        #region Fields

        private readonly IUserService _userService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IUserReportService _userReportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly UserSettings _userSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IExportManager _exportManager;
        private readonly IUserActivityService _userActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly AddressSettings _addressSettings;
        private readonly IStoreService _storeService;
        private readonly IUserAttributeParser _userAttributeParser;
        private readonly IUserAttributeService _userAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerService _customerService;

        #endregion

        #region Constructors

        public UserController(IUserService userService,
            IGenericAttributeService genericAttributeService,
            IUserRegistrationService userRegistrationService,
            IUserReportService userReportService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            DateTimeSettings dateTimeSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            UserSettings userSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IExportManager exportManager,
            IUserActivityService userActivityService,
            IPermissionService permissionService,
            IQueuedEmailService queuedEmailService,
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            IOpenAuthenticationService openAuthenticationService,
            AddressSettings addressSettings,
            IStoreService storeService,
            IUserAttributeParser userAttributeParser,
            IUserAttributeService userAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IWorkflowMessageService workflowMessageService,
            ICacheManager cacheManager,
            ICustomerService customerService)
        {
            this._userService = userService;
            this._genericAttributeService = genericAttributeService;
            this._userRegistrationService = userRegistrationService;
            this._userReportService = userReportService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._dateTimeSettings = dateTimeSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._userSettings = userSettings;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._exportManager = exportManager;
            this._userActivityService = userActivityService;
            this._permissionService = permissionService;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountSettings = emailAccountSettings;
            this._emailAccountService = emailAccountService;
            this._openAuthenticationService = openAuthenticationService;
            this._addressSettings = addressSettings;
            this._storeService = storeService;
            this._userAttributeParser = userAttributeParser;
            this._userAttributeService = userAttributeService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._workflowMessageService = workflowMessageService;
            this._cacheManager = cacheManager;
            this._customerService = customerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual string GetUserRolesNames(IList<UserRole> userRoles, string separator = ",")
        {
            var sb = new StringBuilder();
            for (var i = 0; i < userRoles.Count; i++)
            {
                sb.Append(userRoles[i].Name);
                if (i == userRoles.Count - 1) continue;
                sb.Append(separator);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        [NonAction]
        protected virtual IList<RegisteredUserReportLineModel> GetReportRegisteredUsersModel()
        {
            var report = new List<RegisteredUserReportLineModel>
            {
                new RegisteredUserReportLineModel
                {
                    Period =
                        _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.7days"),
                    Users = _userReportService.GetRegisteredUsersReport(7)
                },
                new RegisteredUserReportLineModel
                {
                    Period = _localizationService.GetResource(
                        "Admin.Users.Reports.RegisteredUsers.Fields.Period.14days"),
                    Users = _userReportService.GetRegisteredUsersReport(14)
                },
                new RegisteredUserReportLineModel
                {
                    Period =
                        _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.month"),
                    Users = _userReportService.GetRegisteredUsersReport(30)
                },
                new RegisteredUserReportLineModel
                {
                    Period = _localizationService.GetResource("Admin.Users.Reports.RegisteredUsers.Fields.Period.year"),
                    Users = _userReportService.GetRegisteredUsersReport(365)
                }
            };


            return report;
        }

        [NonAction]
        protected virtual IList<UserModel.AssociatedExternalAuthModel> GetAssociatedExternalAuthRecords(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var result = new List<UserModel.AssociatedExternalAuthModel>();
            foreach (var record in _openAuthenticationService.GetExternalIdentifiersFor(user))
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
        protected virtual UserModel PrepareUserModelForList(User user)
        {
            return new UserModel
            {
                Id = user.Id,
                Email = user.IsRegistered() ? user.Email : _localizationService.GetResource("Admin.Users.Guest"),
                Username = user.Username,
                FullName = user.GetFullName(),
                Company = user.GetAttribute<string>(SystemUserAttributeNames.Company),
                Phone = user.GetAttribute<string>(SystemUserAttributeNames.Phone),
                ZipPostalCode = user.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode),
                UserRoleNames = GetUserRolesNames(user.UserRoles.ToList()),
                Active = user.Active,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(user.CreatedOnUtc, DateTimeKind.Utc),
                LastActivityDate = _dateTimeHelper.ConvertToUserTime(user.LastActivityDateUtc, DateTimeKind.Utc),
            };
        }

        [NonAction]
        protected virtual string ValidateUserRoles(IList<UserRole> userRoles)
        {
            if (userRoles == null)
                throw new ArgumentNullException(nameof(userRoles));

            //ensure a User is not added to both 'Guests' and 'Registered' User roles
            //ensure that a User is in at least one required role ('Guests' and 'Registered')
            var isInGuestsRole = userRoles.FirstOrDefault(cr => cr.SystemName == SystemUserRoleNames.Guests) != null;
            var isInRegisteredRole = userRoles.FirstOrDefault(cr => cr.SystemName == SystemUserRoleNames.Registered) != null;
            if (isInGuestsRole && isInRegisteredRole)
                return _localizationService.GetResource("Admin.Users.Users.GuestsAndRegisteredRolesError");
            if (!isInGuestsRole && !isInRegisteredRole)
                return _localizationService.GetResource("Admin.Users.Users.AddUserToGuestsOrRegisteredRoleError");

            //no errors
            return "";
        }

        [NonAction]
        protected virtual void PrepareUserAttributeModel(UserModel model, User user)
        {
            var userAttributes = _userAttributeService.GetAllUserAttributes();
            foreach (var attribute in userAttributes)
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
                    var attributeValues = _userAttributeService.GetUserAttributeValues(attribute.Id);
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
                if (user != null)
                {
                    var selectedUserAttributes = user.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes, _genericAttributeService);
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
                                    var selectedValues = _userAttributeParser.ParseUserAttributeValues(selectedUserAttributes);
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
                                    var enteredText = _userAttributeParser.ParseValues(selectedUserAttributes, attribute.Id);
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
                throw new ArgumentNullException(nameof(form));

            string attributesXml = "";
            var userAttributes = _userAttributeService.GetAllUserAttributes();
            foreach (var attribute in userAttributes)
            {
                string controlId = $"User_attribute_{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!string.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _userAttributeParser.AddUserAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!string.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _userAttributeParser.AddUserAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _userAttributeService.GetUserAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _userAttributeParser.AddUserAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!string.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.Trim();
                                attributesXml = _userAttributeParser.AddUserAttribute(attributesXml,
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
            var newsletterSubscriptionStoreIds = new List<int>();
            if (User != null)
            {
                model.Id = User.Id;
                model.UserId = User.Id;
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

                    model.TimeZoneId = User.GetAttribute<string>(SystemUserAttributeNames.TimeZoneId);
                    model.VatNumber = User.GetAttribute<string>(SystemUserAttributeNames.VatNumber);
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(User.CreatedOnUtc, DateTimeKind.Utc);
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(User.LastActivityDateUtc, DateTimeKind.Utc);
                    model.LastIpAddress = User.LastIpAddress;
                    model.LastVisitedPage = User.GetAttribute<string>(SystemUserAttributeNames.LastVisitedPage);

                    model.SelectedUserRoleIds = User.UserRoles.Select(cr => cr.Id).ToList();

                    model.SelectedCustomerIds = User.Customers.Select(x => x.Id).ToList();

                    model.SelectedCustomerRegionIds = User.CustomerRegions.Select(x => x.Id).ToList();

                    //newsletter subscriptions
                    if (!string.IsNullOrEmpty(User.Email))
                    {
                        foreach (var store in allStores)
                        {
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

            model.UsernamesEnabled = _userSettings.UsernamesEnabled;
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

            model.GenderEnabled = _userSettings.GenderEnabled;
            model.DateOfBirthEnabled = _userSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _userSettings.CompanyEnabled;
            model.StreetAddressEnabled = _userSettings.StreetAddressEnabled;
            model.StreetAddress2Enabled = _userSettings.StreetAddress2Enabled;
            model.ZipPostalCodeEnabled = _userSettings.ZipPostalCodeEnabled;
            model.CityEnabled = _userSettings.CityEnabled;
            model.CountryEnabled = _userSettings.CountryEnabled;
            model.StateProvinceEnabled = _userSettings.StateProvinceEnabled;
            model.PhoneEnabled = _userSettings.PhoneEnabled;
            model.FaxEnabled = _userSettings.FaxEnabled;

            //countries and states
            if (_userSettings.CountryEnabled)
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

                if (_userSettings.StateProvinceEnabled)
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
            var allRoles = _userService.GetAllUserRoles(true);
            var adminRole = allRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered);
            //precheck Registered Role as a default role while creating a new User through admin
            if (User == null && adminRole != null)
            {
                model.SelectedUserRoleIds.Add(adminRole.Id);
            }

            if (_workContext.CurrentUser.IsAdmin())
            {
                foreach (var role in allRoles)
                {
                    model.AvailableUserRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = model.SelectedUserRoleIds.Contains(role.Id)
                    });
                }
            }
            else
            {
                var topRoleId = _workContext.CurrentUser.GetUserRoleIds().Max();
                foreach (var role in allRoles.Where(x => x.Id < topRoleId))
                {
                    model.AvailableUserRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = model.SelectedUserRoleIds.Contains(role.Id)
                    });
                }
            }

            if (_workContext.CurrentUser.IsAdmin())
            {
                var customers = _customerService.GetAllCustomers();
                foreach (var customer in customers)
                {
                    model.AvailableCustomers.Add(new SelectListItem
                    {
                        Text = customer.Name,
                        Value = customer.Id.ToString(),
                        Selected = model.SelectedCustomerIds.Contains(model.Id)
                    });
                }

                var stateProvinces = _customerService
                                       .GetAllCustomers()
                                       .Select(x => x.Country.StateProvinces)
                                       .Distinct();

                foreach (var stateProvince in stateProvinces)
                {
                    foreach (var region in stateProvince)
                    {
                        model.AvailableCustomerRegions.Add(new SelectListItem
                        {
                            Text = region.Name,
                            Value = region.Id.ToString(),
                            Selected = model.SelectedCustomerRegionIds.Contains(model.Id)
                        });
                    }
                }
            }
            else
            {
                //Customer
                foreach (var customer in _workContext.CurrentUser.Customers)
                {
                    foreach (var man in _customerService.GetAllCustomers().Where(x => x.Id == customer.Id))
                    {
                        model.AvailableCustomers.Add(new SelectListItem
                        {
                            Text = man.Name,
                            Value = man.Id.ToString(),
                            Selected = model.SelectedCustomerIds.Contains(model.Id)
                        });
                    }
                }

                //Customer region
                foreach (var cr in _workContext.CurrentUser.CustomerRegions)
                {
                    var stateProvinces = _customerService
                        .GetAllCustomers(countryId: cr.CountryId, stateId: cr.Id)
                        .Select(x => x.StateProvince)
                        //.Where(x => x.StateProvinceId == cr.Id)
                        //.Select(x => x.Country.StateProvinces.Where(a => a.Id == cr.Id).Distinct())
                        .Distinct();

                    //foreach (var stateProvince in stateProvinces)
                    //{
                    foreach (var region in stateProvinces)
                    {
                        model.AvailableCustomerRegions.Add(new SelectListItem
                        {
                            Text = region.Name,
                            Value = region.Id.ToString(),
                            Selected = model.SelectedCustomerRegionIds.Contains(model.Id)
                        });
                    }
                    //}

                    foreach (var man in _customerService.GetAllCustomers(countryId: cr.CountryId, stateId: cr.Id))
                    {
                        model.AvailableCustomers.Add(new SelectListItem
                        {
                            Text = man.Name,
                            Value = man.Id.ToString(),
                            Selected = model.SelectedCustomerIds.Contains(model.Id)
                        });
                    }
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
            model.AllowSendingOfWelcomeMessage = _userSettings.UserRegistrationType == UserRegistrationType.AdminApproval &&
                User != null &&
                User.IsRegistered();
            //sending of the activation message
            //1. "email validation" registration method
            //2. already created User
            //3. registered
            //4. not active
            model.AllowReSendingOfActivationMessage = _userSettings.UserRegistrationType == UserRegistrationType.EmailValidation &&
                User != null &&
                User.IsRegistered() &&
                !User.Active;
        }

        [NonAction]
        protected virtual void PrepareAddressModel(UserAddressModel model, Address address, User user, bool excludeProperties)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            model.UserId = user.Id;
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
        private bool SecondAdminAccountExists(User user)
        {
            var users = _userService.GetAllUsers(UserRoleIds: new[] { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Administrators).Id });

            return users.Any(c => c.Active && c.Id != user.Id);
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
            var defaultRoleIds = new List<int> { _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id };
            var model = new UserListModel
            {
                UsernamesEnabled = _userSettings.UsernamesEnabled,
                DateOfBirthEnabled = _userSettings.DateOfBirthEnabled,
                CompanyEnabled = _userSettings.CompanyEnabled,
                PhoneEnabled = _userSettings.PhoneEnabled,
                ZipPostalCodeEnabled = _userSettings.ZipPostalCodeEnabled,
                SearchUserRoleIds = defaultRoleIds,
            };
            var allRoles = _userService.GetAllUserRoles(true);
            if (_workContext.CurrentUser.IsAdmin())
            {
                foreach (var role in allRoles)
                {
                    model.AvailableUserRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = defaultRoleIds.Any(x => x == role.Id)
                    });
                }
            }
            else
            {
                var topRoleId = _workContext.CurrentUser.GetUserRoleIds().Max();
                foreach (var role in allRoles.Where(x => x.Id < topRoleId))
                {
                    model.AvailableUserRoles.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id.ToString(),
                        Selected = defaultRoleIds.Any(x => x == role.Id)
                    });
                }
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

            if (_workContext.CurrentUser.IsAdmin())
            {
                var users2 = _userService.GetAllUsers(
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

                var gridModel2 = new DataSourceResult
                {
                    Data = users2.Select(PrepareUserModelForList),
                    Total = users2.TotalCount
                };

                return Json(gridModel2);
            }

            var users = _userService.GetAllUsers(
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
                loadOnlyWithShoppingCart: false);

            var filtretUsers = new List<User>();
            if (_workContext.CurrentUser.CustomerRegions.Any())
            {
                foreach (var region in _workContext.CurrentUser.CustomerRegions)
                {
                    filtretUsers.AddRange(users.Where(x => x.CustomerRegions.Contains(region)));

                    foreach (var user in users)
                    {
                        foreach (var customer in user.Customers)
                        {
                            if (customer.StateProvinceId == region.Id)
                            {
                                filtretUsers.Add(user);
                            }
                        }
                    }
                }
            }
            //filtretUsers.AddRange(from user in users from cr in _workContext.CurrentUser.CustomerRegions where user.CustomerRegions.Contains(cr) select user);

            if (_workContext.CurrentUser.Customers.Any())
                filtretUsers.AddRange(from user in users from c in _workContext.CurrentUser.Customers where user.Customers.Contains(c) select user);

            var topRoleId = _workContext.CurrentUser.GetUserRoleIds().Max();
            var result = filtretUsers.Distinct().Where(x => x.UserRoles.Select(ur => ur.Id).Max() < topRoleId).ToList();

            var pagedList = new PagedList<User>(result, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = pagedList.Select(PrepareUserModelForList),
                Total = pagedList.TotalCount
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
                var cust2 = _userService.GetUserByEmail(model.Email);
                if (cust2 != null)
                    ModelState.AddModelError("", "Email is already registered");
            }
            if (!String.IsNullOrWhiteSpace(model.Username) & _userSettings.UsernamesEnabled)
            {
                var cust2 = _userService.GetUserByUsername(model.Username);
                if (cust2 != null)
                    ModelState.AddModelError("", "Username is already registered");
            }

            //validate User roles
            var allUserRoles = _userService.GetAllUserRoles(true);
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
            var userAttributesXml = ParseCustomUserAttributes(form);
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null)
            {
                var userAttributeWarnings = _userAttributeParser.GetAttributeWarnings(userAttributesXml);
                foreach (var error in userAttributeWarnings)
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
                var user = new User
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
                _userService.InsertUser(user);

                //form fields
                if (_dateTimeSettings.AllowUsersToSetTimeZone)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.TimeZoneId, model.TimeZoneId);
                if (_userSettings.GenderEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Gender, model.Gender);
                _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.FirstName, model.FirstName);
                _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.LastName, model.LastName);
                if (_userSettings.DateOfBirthEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.DateOfBirth, model.DateOfBirth);
                if (_userSettings.CompanyEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Company, model.Company);
                if (_userSettings.StreetAddressEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.StreetAddress, model.StreetAddress);
                if (_userSettings.StreetAddress2Enabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.StreetAddress2, model.StreetAddress2);
                if (_userSettings.ZipPostalCodeEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.ZipPostalCode, model.ZipPostalCode);
                if (_userSettings.CityEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.City, model.City);
                if (_userSettings.CountryEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.CountryId, model.CountryId);
                if (_userSettings.CountryEnabled && _userSettings.StateProvinceEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.StateProvinceId, model.StateProvinceId);
                if (_userSettings.PhoneEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Phone, model.Phone);
                if (_userSettings.FaxEnabled)
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Fax, model.Fax);

                //custom User attributes
                _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.CustomUserAttributes, userAttributesXml);

                //password
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.Email, false, _userSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = _userRegistrationService.ChangePassword(changePassRequest);
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

                    user.UserRoles.Add(userRole);
                }
                _userService.UpdateUser(user);

                foreach (var customerId in model.SelectedCustomerIds)
                {
                    var Customer = _customerService.GetCustomerById(customerId);

                    if (Customer != null)
                    {
                        user.Customers.Add(Customer);
                    }
                }
                _userService.UpdateUser(user);

                foreach (var stateProvinceId in model.SelectedCustomerRegionIds)
                {
                    var region = _stateProvinceService.GetStateProvinceById(stateProvinceId);

                    if (region != null)
                    {
                        user.CustomerRegions.Add(region);
                    }
                }
                _userService.UpdateUser(user);

                //ensure that a User with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to other functionality in admin area
                if (user.IsAdmin() && user.VendorId > 0)
                {
                    user.VendorId = 0;
                    _userService.UpdateUser(user);
                    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminCouldNotbeVendor"));
                }

                //activity log
                _userActivityService.InsertActivity("AddNewUser", _localizationService.GetResource("ActivityLog.AddNewUser"), user.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = user.Id });
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

            var user = _userService.GetUserById(id);
            if (user == null || user.Deleted)
                //No User found with the specified id
                return RedirectToAction("List");

            var model = new UserModel();
            PrepareUserModel(model, user, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        public virtual ActionResult Edit(UserModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null || user.Deleted)
                //No User found with the specified id
                return RedirectToAction("List");

            //validate User roles
            var allUserRoles = _userService.GetAllUserRoles(true);
            var newUserRoles = new List<UserRole>();
            foreach (var userRole in allUserRoles)
                if (model.SelectedUserRoleIds.Contains(userRole.Id))
                    newUserRoles.Add(userRole);
            var userRolesError = ValidateUserRoles(newUserRoles);
            if (!string.IsNullOrEmpty(userRolesError))
            {
                ModelState.AddModelError("", userRolesError);
                ErrorNotification(userRolesError, false);
            }

            // Ensure that valid email address is entered if Registered role is checked to avoid registered Users with empty email address
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null && !CommonHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("", _localizationService.GetResource("Admin.Users.Users.ValidEmailRequiredRegisteredRole"));
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.ValidEmailRequiredRegisteredRole"), false);
            }

            //custom User attributes
            var userAttributesXml = ParseCustomUserAttributes(form);
            if (newUserRoles.Any() && newUserRoles.FirstOrDefault(c => c.SystemName == SystemUserRoleNames.Registered) != null)
            {
                var userAttributeWarnings = _userAttributeParser.GetAttributeWarnings(userAttributesXml);
                foreach (var error in userAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.AdminComment = model.AdminComment;
                    user.IsTaxExempt = model.IsTaxExempt;

                    //prevent deactivation of the last active administrator
                    if (!user.IsAdmin() || model.Active || SecondAdminAccountExists(user))
                        user.Active = model.Active;
                    else
                        ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminAccountShouldExists.Deactivate"));

                    //email
                    if (!string.IsNullOrWhiteSpace(model.Email))
                    {
                        _userRegistrationService.SetEmail(user, model.Email, false);
                    }
                    else
                    {
                        user.Email = model.Email;
                    }

                    //username
                    if (_userSettings.UsernamesEnabled)
                    {
                        if (!string.IsNullOrWhiteSpace(model.Username))
                        {
                            _userRegistrationService.SetUsername(user, model.Username);
                        }
                        else
                        {
                            user.Username = model.Username;
                        }
                    }

                    //form fields
                    if (_dateTimeSettings.AllowUsersToSetTimeZone)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.TimeZoneId, model.TimeZoneId);
                    if (_userSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Gender, model.Gender);
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.LastName, model.LastName);
                    if (_userSettings.DateOfBirthEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.DateOfBirth, model.DateOfBirth);
                    if (_userSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Company, model.Company);
                    if (_userSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.StreetAddress, model.StreetAddress);
                    if (_userSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_userSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_userSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.City, model.City);
                    if (_userSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.CountryId, model.CountryId);
                    if (_userSettings.CountryEnabled && _userSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_userSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Phone, model.Phone);
                    if (_userSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.Fax, model.Fax);

                    //custom User attributes
                    _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.CustomUserAttributes, userAttributesXml);

                    if (model.SelectedCustomerIds.Any())
                    {
                        user.Customers.Clear();
                        foreach (var customerId in model.SelectedCustomerIds)
                        {
                            var customer = _customerService.GetCustomerById(customerId);

                            if (customer != null)
                            {
                                user.Customers.Add(customer);
                            }
                        }
                        _userService.UpdateUser(user);
                    }
                    else
                    {
                        user.Customers.Clear();
                        _userService.UpdateUser(user);
                    }

                    if (model.SelectedCustomerRegionIds.Any())
                    {
                        user.CustomerRegions.Clear();
                        foreach (var regionId in model.SelectedCustomerRegionIds)
                        {
                            var region = _stateProvinceService.GetStateProvinceById(regionId);

                            if (region != null)
                            {
                                user.CustomerRegions.Add(region);
                            }
                        }
                        _userService.UpdateUser(user);
                    }
                    else
                    {
                        user.CustomerRegions.Clear();
                        _userService.UpdateUser(user);
                    }

                    //User roles
                    foreach (var userRole in allUserRoles)
                    {
                        //ensure that the current User cannot add/remove to/from "Administrators" system role
                        //if he's not an admin himself
                        if (userRole.SystemName == SystemUserRoleNames.Administrators &&
                            !_workContext.CurrentUser.IsAdmin())
                            continue;

                        if (model.SelectedUserRoleIds.Contains(userRole.Id))
                        {
                            //new role
                            if (user.UserRoles.Count(cr => cr.Id == userRole.Id) == 0)
                                user.UserRoles.Add(userRole);
                        }
                        else
                        {
                            //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                            if (userRole.SystemName == SystemUserRoleNames.Administrators && !SecondAdminAccountExists(user))
                            {
                                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminAccountShouldExists.DeleteRole"));
                                continue;
                            }

                            //remove role
                            if (user.UserRoles.Count(cr => cr.Id == userRole.Id) > 0)
                                user.UserRoles.Remove(userRole);
                        }
                    }
                    _userService.UpdateUser(user);


                    //ensure that a User with a vendor associated is not in "Administrators" role
                    //otherwise, he won't have access to the other functionality in admin area
                    if (user.IsAdmin() && user.VendorId > 0)
                    {
                        user.VendorId = 0;
                        _userService.UpdateUser(user);
                        ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminCouldNotbeVendor"));
                    }

                    //activity log
                    _userActivityService.InsertActivity("EditUser", _localizationService.GetResource("ActivityLog.EditUser"), user.Id);

                    SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Updated"));
                    if (continueEditing)
                    {
                        //selected tab
                        SaveSelectedTabName();

                        return RedirectToAction("Edit", new { id = user.Id });
                    }
                    return RedirectToAction("List");
                }
                catch (Exception exc)
                {
                    ErrorNotification(exc.Message, false);
                }
            }


            //If we got this far, something failed, redisplay form
            PrepareUserModel(model, user, true);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual ActionResult ChangePassword(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //ensure that the current User cannot change passwords of "Administrators" if he's not an admin himself
            if (user.IsAdmin() && !_workContext.CurrentUser.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.OnlyAdminCanChangePassword"));
                return RedirectToAction("Edit", new { id = user.Id });
            }

            if (ModelState.IsValid)
            {
                var changePassRequest = new ChangePasswordRequest(model.Email,
                    false, _userSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = _userRegistrationService.ChangePassword(changePassRequest);
                if (changePassResult.Success)
                    SuccessNotification(_localizationService.GetResource("Admin.Users.Users.PasswordChanged"));
                else
                    foreach (var error in changePassResult.Errors)
                        ErrorNotification(error);
            }

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsValid")]
        public virtual ActionResult MarkVatNumberAsValid(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //_genericAttributeService.SaveAttribute(User, 
            //    SystemUserAttributeNames.VatNumberStatusId,
            //    (int)VatNumberStatus.Valid);

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markVatNumberAsInvalid")]
        public virtual ActionResult MarkVatNumberAsInvalid(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //_genericAttributeService.SaveAttribute(User,
            //    SystemUserAttributeNames.VatNumberStatusId,
            //    (int)VatNumberStatus.Invalid);

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("remove-affiliate")]
        public virtual ActionResult RemoveAffiliate(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            user.AffiliateId = 0;
            _userService.UpdateUser(user);

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            try
            {
                //prevent attempts to delete the user, if it is the last active administrator
                if (user.IsAdmin() && !SecondAdminAccountExists(user))
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.AdminAccountShouldExists.DeleteAdministrator"));
                    return RedirectToAction("Edit", new { id = user.Id });
                }

                //ensure that the current User cannot delete "Administrators" if he's not an admin himself
                if (user.IsAdmin() && !_workContext.CurrentUser.IsAdmin())
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Users.Users.OnlyAdminCanDeleteAdmin"));
                    return RedirectToAction("Edit", new { id = user.Id });
                }

                //delete
                _userService.DeleteUser(user);

                //remove newsletter subscription (if exists)
                //foreach (var store in _storeService.GetAllStores())
                //{
                //    var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(User.Email, store.Id);
                //    if (subscription != null)
                //        _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
                //}

                //activity log
                _userActivityService.InsertActivity("DeleteUser", _localizationService.GetResource("ActivityLog.DeleteUser"), user.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = user.Id });
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("impersonate")]
        public virtual ActionResult Impersonate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AllowUserImpersonation))
                return AccessDeniedView();

            var user = _userService.GetUserById(id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (!_workContext.CurrentUser.IsAdmin() && user.IsAdmin())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Users.Users.NonAdminNotImpersonateAsAdminError"));
                return RedirectToAction("Edit", user.Id);
            }

            //activity log
            _userActivityService.InsertActivity("Impersonation.Started", _localizationService.GetResource("ActivityLog.Impersonation.Started.StoreOwner"), user.Email, user.Id);
            _userActivityService.InsertActivity(user, "Impersonation.Started", _localizationService.GetResource("ActivityLog.Impersonation.Started.User"), _workContext.CurrentUser.Email, _workContext.CurrentUser.Id);

            //ensure login is not required
            user.RequireReLogin = false;
            _userService.UpdateUser(user);
            _genericAttributeService.SaveAttribute<int?>(_workContext.CurrentUser, SystemUserAttributeNames.ImpersonatedUserId, user.Id);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-welcome-message")]
        public virtual ActionResult SendWelcomeMessage(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            _workflowMessageService.SendUserWelcomeMessage(user, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Users.Users.SendWelcomeMessage.Success"));

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("resend-activation-message")]
        public virtual ActionResult ReSendActivationMessage(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            //email validation message
            _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
            _workflowMessageService.SendUserEmailValidationMessage(user, _workContext.WorkingLanguage.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Users.Users.ReSendActivationMessage.Success"));

            return RedirectToAction("Edit", new { id = user.Id });
        }

        public virtual ActionResult SendEmail(UserModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.Id);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            try
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                    throw new InvenioException("User email is empty");
                if (!CommonHelper.IsValidEmail(user.Email))
                    throw new InvenioException("User email is not valid");
                if (string.IsNullOrWhiteSpace(model.SendEmail.Subject))
                    throw new InvenioException("Email subject is empty");
                if (string.IsNullOrWhiteSpace(model.SendEmail.Body))
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
                    ToName = user.GetFullName(),
                    To = user.Email,
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

            return RedirectToAction("Edit", new { id = user.Id });
        }
        #endregion

        #region Addresses

        [HttpPost]
        public virtual ActionResult AddressesSelect(int userId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var user = _userService.GetUserById(userId);
            if (user == null)
                throw new ArgumentException("No User found with the specified id", "userId");

            var addresses = user.Addresses.OrderByDescending(a => a.CreatedOnUtc).ThenByDescending(a => a.Id).ToList();
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
        public virtual ActionResult AddressDelete(int id, int userId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(userId);
            if (user == null)
                throw new ArgumentException("No User found with the specified id", "userId");

            var address = user.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
                //No User found with the specified id
                return Content("No User found with the specified id");
            user.RemoveAddress(address);
            _userService.UpdateUser(user);
            //now delete the address record
            _addressService.DeleteAddress(address);

            return new NullJsonResult();
        }

        public virtual ActionResult AddressCreate(int userId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(userId);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            var model = new UserAddressModel();
            PrepareAddressModel(model, null, user, false);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult AddressCreate(UserAddressModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.UserId);
            if (user == null)
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
                user.Addresses.Add(address);
                _userService.UpdateUser(user);

                SuccessNotification(_localizationService.GetResource("Admin.Users.Users.Addresses.Added"));
                return RedirectToAction("AddressEdit", new { addressId = address.Id, UserId = model.UserId });
            }

            //If we got this far, something failed, redisplay form
            PrepareAddressModel(model, null, user, true);
            return View(model);
        }

        public virtual ActionResult AddressEdit(int addressId, int userId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(userId);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = user.Id });

            var model = new UserAddressModel();
            PrepareAddressModel(model, address, user, false);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult AddressEdit(UserAddressModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var user = _userService.GetUserById(model.UserId);
            if (user == null)
                //No User found with the specified id
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = user.Id });

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
            PrepareAddressModel(model, address, user, true);

            return View(model);
        }

        #endregion

        #region UserMonthlyWorkHours
        public virtual ActionResult UserMonthlyWorkHoursList(int userId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            if (userId == 0)
                return new NullJsonResult();

            var userMonthlyWorkingHoursList = _userService.GetUserById(userId)?.UserMonthlyWorkingHours.ToList().OrderByDescending(x => x.FirstDateOfMonth);

            var listOfModel = new List<UserMonthlyWorkingHoursModel>();
            foreach (var item in userMonthlyWorkingHoursList)
            {
                var resultModel = new UserMonthlyWorkingHoursModel
                {
                    UserId = userId,
                    WorkHours = item.WorkHours,
                    Period = GetPeriodAsString(item),
                    Id = item.Id
                };

                listOfModel.Add(resultModel);
            }

            //var pagedList = new PagedList<UserMonthlyWorkingHoursModel>(listOfModel, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = listOfModel,
                Total = listOfModel.Count
            };

            return Json(gridModel);
        }

        private string GetPeriodAsString(UserMonthlyWorkingHours umwh)
        {
            var month = umwh.FirstDateOfMonth.ToString("MMMM", CultureInfo.InvariantCulture);
            var year = umwh.FirstDateOfMonth.Year;

            return $"{month} {year}";
        }

        [HttpPost]
        public virtual ActionResult UserMonthlyWorkHoursInsert(UserModel.UserWorkingMonthlyHours model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var user = _userService.GetUserById(model.UserId);

            if (user != null && !string.IsNullOrEmpty(model.Period))
            {
                var period = model.Period.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (period.Length > 2)
                {
                    var month = GetMonthByString(period[1]);
                    var year = int.Parse(period[3]);

                    var umwh = new UserMonthlyWorkingHours
                    {
                        FirstDateOfMonth = new DateTime(year, month, 1),
                        LastDateOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                        WorkHours = model.WorkHours
                    };

                    user.UserMonthlyWorkingHours.Add(umwh);

                    _userService.UpdateUser(user);
                }
            }

            return new NullJsonResult();
        }

        private int GetMonthByString(string month)
        {
            switch (month)
            {
                case "Jan": return 1;
                case "Feb": return 2;
                case "Mar": return 3;
                case "Apr": return 4;
                case "May": return 5;
                case "Jun": return 6;
                case "Jul": return 7;
                case "Aug": return 8;
                case "Sep": return 9;
                case "Oct": return 10;
                case "Nov": return 11;
                case "Dec": return 12;
                default: return 0;
            }
        }

        [HttpPost]
        public virtual ActionResult UserMonthlyWorkHoursUpdate(UserModel.UserWorkingMonthlyHours model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var user = _userService.GetUserById(model.UserId);

            if (user != null)
            {
                var period = model.Period.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (period.Length > 2)
                {
                    var month = GetMonthByString(period[1]);
                    var year = int.Parse(period[3]);

                    var existLine = user.UserMonthlyWorkingHours
                                        .FirstOrDefault(x => x.FirstDateOfMonth.Month == month && x.FirstDateOfMonth.Year == year);

                    if (existLine == null)
                    {
                        var umwh = new UserMonthlyWorkingHours
                        {
                            FirstDateOfMonth = new DateTime(year, month, 1),
                            LastDateOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                            WorkHours = model.WorkHours
                        };

                        user.UserMonthlyWorkingHours.Add(umwh);
                    }
                    else
                    {
                        var nel = existLine;
                        nel.WorkHours = model.WorkHours;
                        user.UserMonthlyWorkingHours.Remove(existLine);
                        user.UserMonthlyWorkingHours.Add(nel);
                    }

                    _userService.UpdateUser(user);
                }
                else
                {
                    var monthInt = GetMonthLikeNumber(period[0]);
                    var year = int.Parse(period[1]);

                    var existLine = user.UserMonthlyWorkingHours
                        .FirstOrDefault(x => x.FirstDateOfMonth.Month == monthInt && x.FirstDateOfMonth.Year == year);

                    if (existLine != null)
                    {
                        var nel = existLine;
                        nel.WorkHours = model.WorkHours;
                        user.UserMonthlyWorkingHours.Remove(existLine);
                        user.UserMonthlyWorkingHours.Add(nel);
                        _userService.UpdateUser(user);
                    }
                }
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual ActionResult UserMonthlyWorkHoursDelete(UserModel.UserWorkingMonthlyHours model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var user = _userService.GetUserById(model.UserId);

            if (user != null)
            {
                var period = model.Period.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (period.Length > 2)
                {
                    var month = GetMonthByString(period[1]);
                    var year = int.Parse(period[3]);

                    var existLine = user.UserMonthlyWorkingHours
                        .FirstOrDefault(x => x.FirstDateOfMonth.Month == month && x.FirstDateOfMonth.Year == year);

                    if (existLine != null)
                    {
                        user.UserMonthlyWorkingHours.Remove(existLine);
                        _userService.UpdateUser(user);
                    }
                }
                else
                {
                    var monthInt = GetMonthLikeNumber(period[0]);
                    var year = int.Parse(period[1]);

                    var existLine = user.UserMonthlyWorkingHours
                        .FirstOrDefault(x => x.FirstDateOfMonth.Month == monthInt && x.FirstDateOfMonth.Year == year);

                    if (existLine != null)
                    {
                        user.UserMonthlyWorkingHours.Remove(existLine);
                        _userService.UpdateUser(user);
                    }
                }
            }

            return new NullJsonResult();
        }

        private int GetMonthLikeNumber(string month)
        {
            switch (month)
            {
                case "January": return 1;
                case "February": return 2;
                case "March": return 3;
                case "April": return 4;
                case "May": return 5;
                case "June": return 6;
                case "July": return 7;
                case "August": return 8;
                case "September": return 9;
                case "October": return 10;
                case "November": return 11;
                case "December": return 12;
                default: return 0;
            }
        }
        #endregion

        #region Activity log

        [HttpPost]
        public virtual ActionResult ListActivityLog(DataSourceRequest command, int userId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedKendoGridJson();

            var activityLog = _userActivityService.GetAllActivities(null, null, userId, 0, command.Page - 1, command.PageSize);
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

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public virtual ActionResult ExportExcelAll(UserListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers))
                return AccessDeniedView();

            var searchDayOfBirth = 0;
            int searchMonthOfBirth = 0;
            if (!string.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!string.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var users = _userService.GetAllUsers(
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
                byte[] bytes = _exportManager.ExportUsersToXlsx(users);
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

            var users = new List<User>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                users.AddRange(_userService.GetUsersByIds(ids));
            }

            try
            {
                byte[] bytes = _exportManager.ExportUsersToXlsx(users);
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
            if (!string.IsNullOrWhiteSpace(model.SearchDayOfBirth))
                searchDayOfBirth = Convert.ToInt32(model.SearchDayOfBirth);
            if (!string.IsNullOrWhiteSpace(model.SearchMonthOfBirth))
                searchMonthOfBirth = Convert.ToInt32(model.SearchMonthOfBirth);

            var users = _userService.GetAllUsers(
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
                var xml = _exportManager.ExportUsersToXml(users);
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

            var users = new List<User>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                users.AddRange(_userService.GetUsersByIds(ids));
            }

            var xml = _exportManager.ExportUsersToXml(users);
            return new XmlDownloadResult(xml, "Users.xml");
        }

        #endregion
    }
}
