using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Domain;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Forums;
using Invenio.Core.Domain.Localization;
using Invenio.Core.Domain.Media;
using Invenio.Core.Domain.Messages;
//using Invenio.Core.Domain.Tax;
using Invenio.Services.Authentication;
using Invenio.Services.Authentication.External;
using Invenio.Services.Common;
using Invenio.Services.Users;
using Invenio.Services.Directory;
using Invenio.Services.Events;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Media;
using Invenio.Services.Messages;
//using Invenio.Services.Orders;
using Invenio.Services.Stores;
//using Invenio.Services.Tax;
using Invenio.Web.Extensions;
using Invenio.Web.Factories;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Controllers;
using Invenio.Web.Framework.Security;
using Invenio.Web.Framework.Security.Captcha;
using Invenio.Web.Framework.Security.Honeypot;
using Invenio.Web.Models.User;

namespace Invenio.Web.Controllers
{
    public partial class UserController : BasePublicController
    {
        #region Fields

        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IUserModelFactory _UserModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly DateTimeSettings _dateTimeSettings;
        //private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IUserService _UserService;
        private readonly IUserAttributeParser _UserAttributeParser;
        private readonly IUserAttributeService _UserAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUserRegistrationService _UserRegistrationService;
        //private readonly ITaxService _taxService;
        private readonly UserSettings _UserSettings;
        private readonly AddressSettings _addressSettings;
        //private readonly ForumSettings _forumSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        //private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        //private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        //private readonly IShoppingCartService _shoppingCartService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IWebHelper _webHelper;
        private readonly IUserActivityService _UserActivityService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IStoreService _storeService;
        private readonly IEventPublisher _eventPublisher;

        private readonly MediaSettings _mediaSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly StoreInformationSettings _storeInformationSettings;

        #endregion

        #region Ctor

        public UserController(IAddressModelFactory addressModelFactory,
            IUserModelFactory UserModelFactory,
            IAuthenticationService authenticationService,
            DateTimeSettings dateTimeSettings,
            //TaxSettings taxSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IUserService UserService,
            IUserAttributeParser UserAttributeParser,
            IUserAttributeService UserAttributeService,
            IGenericAttributeService genericAttributeService,
            IUserRegistrationService UserRegistrationService,
            //ITaxService taxService,
            UserSettings UserSettings,
            AddressSettings addressSettings,
            //ForumSettings forumSettings,
            IAddressService addressService,
            ICountryService countryService,
            //IOrderService orderService,
            IPictureService pictureService,
            //INewsLetterSubscriptionService newsLetterSubscriptionService,
            //IShoppingCartService shoppingCartService,
            IOpenAuthenticationService openAuthenticationService,
            IWebHelper webHelper,
            IUserActivityService UserActivityService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IStoreService storeService,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            StoreInformationSettings storeInformationSettings)
        {
            this._addressModelFactory = addressModelFactory;
            this._UserModelFactory = UserModelFactory;
            this._authenticationService = authenticationService;
            this._dateTimeSettings = dateTimeSettings;
            //this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._UserService = UserService;
            this._UserAttributeParser = UserAttributeParser;
            this._UserAttributeService = UserAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._UserRegistrationService = UserRegistrationService;
            //this._taxService = taxService;
            this._UserSettings = UserSettings;
            this._addressSettings = addressSettings;
            //this._forumSettings = forumSettings;
            this._addressService = addressService;
            this._countryService = countryService;
            //this._orderService = orderService;
            this._pictureService = pictureService;
            //this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            //this._shoppingCartService = shoppingCartService;
            this._openAuthenticationService = openAuthenticationService;
            this._webHelper = webHelper;
            this._UserActivityService = UserActivityService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._storeService = storeService;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._storeInformationSettings = storeInformationSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void TryAssociateAccountWithExternalAccount(User User)
        {
            var parameters = ExternalAuthorizerHelper.RetrieveParametersFromRoundTrip(true);
            if (parameters == null)
                return;

            if (_openAuthenticationService.AccountExists(parameters))
                return;

            _openAuthenticationService.AssociateExternalAccountWithUser(User, parameters);
        }

        [NonAction]
        protected virtual string ParseCustomUserAttributes(FormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var attributes = _UserAttributeService.GetAllUserAttributes();
            foreach (var attribute in attributes)
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
                            foreach (var item in cblAttributes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                            )
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

        #endregion

        #region Login / logout

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Login(bool? checkoutAsGuest)
        {
            var model = _UserModelFactory.PrepareLoginModel(checkoutAsGuest);
            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                if (_UserSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult =
                    _UserRegistrationService.ValidateUser(
                        _UserSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case UserLoginResults.Successful:
                    {
                        var User = _UserSettings.UsernamesEnabled
                            ? _UserService.GetUserByUsername(model.Username)
                            : _UserService.GetUserByEmail(model.Email);

                        //migrate shopping cart
                        //_shoppingCartService.MigrateShoppingCart(_workContext.CurrentUser, User, true);

                        //sign in new User
                        _authenticationService.SignIn(User, model.RememberMe);

                        //raise event       
                        _eventPublisher.Publish(new UserLoggedinEvent(User));

                        //activity log
                        _UserActivityService.InsertActivity(User, "PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"));

                        if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                            return RedirectToRoute("HomePage");

                        return Redirect(returnUrl);
                    }
                    case UserLoginResults.UserNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.UserNotExist"));
                        break;
                    case UserLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case UserLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case UserLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case UserLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case UserLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model = _UserModelFactory.PrepareLoginModel(model.CheckoutAsGuest);
            return View(model);
        }

        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Logout()
        {
            //external authentication
            ExternalAuthorizerHelper.RemoveParameters();

            if (_workContext.OriginalUserIfImpersonated != null)
            {
                //activity log
                _UserActivityService.InsertActivity(_workContext.OriginalUserIfImpersonated,
                    "Impersonation.Finished",
                    _localizationService.GetResource("ActivityLog.Impersonation.Finished.StoreOwner"),
                    _workContext.CurrentUser.Email, _workContext.CurrentUser.Id);
                _UserActivityService.InsertActivity("Impersonation.Finished",
                    _localizationService.GetResource("ActivityLog.Impersonation.Finished.User"),
                    _workContext.OriginalUserIfImpersonated.Email, _workContext.OriginalUserIfImpersonated.Id);

                //logout impersonated User
                _genericAttributeService.SaveAttribute<int?>(_workContext.OriginalUserIfImpersonated,
                    SystemUserAttributeNames.ImpersonatedUserId, null);

                //redirect back to User details page (admin area)
                return this.RedirectToAction("Edit", "User",
                    new {id = _workContext.CurrentUser.Id, area = "Admin"});

            }

            //activity log
            _UserActivityService.InsertActivity("PublicStore.Logout", _localizationService.GetResource("ActivityLog.PublicStore.Logout"));
            
            //standard logout 
            _authenticationService.SignOut();

            //raise logged out event       
            _eventPublisher.Publish(new UserLoggedOutEvent(_workContext.CurrentUser));

            //EU Cookie
            if (_storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData["nop.IgnoreEuCookieLawWarning"] = true;
            }

            return RedirectToRoute("HomePage");
        }

        #endregion

        #region Password recovery

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecovery()
        {
            var model = _UserModelFactory.PreparePasswordRecoveryModel();
            return View(model);
        }

        [HttpPost, ActionName("PasswordRecovery")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecoverySend(PasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                var User = _UserService.GetUserByEmail(model.Email);
                if (User != null && User.Active && !User.Deleted)
                {
                    //save token and current date
                    var passwordRecoveryToken = Guid.NewGuid();
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.PasswordRecoveryToken,
                        passwordRecoveryToken.ToString());
                    DateTime? generatedDateTime = DateTime.UtcNow;
                    _genericAttributeService.SaveAttribute(User,
                        SystemUserAttributeNames.PasswordRecoveryTokenDateGenerated, generatedDateTime);

                    //send email
                    _workflowMessageService.SendUserPasswordRecoveryMessage(User,
                        _workContext.WorkingLanguage.Id);

                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailHasBeenSent");
                }
                else
                {
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.EmailNotFound");
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecoveryConfirm(string token, string email)
        {
            var User = _UserService.GetUserByEmail(email);
            if (User == null)
                return RedirectToRoute("HomePage");

            if (string.IsNullOrEmpty(User.GetAttribute<string>(SystemUserAttributeNames.PasswordRecoveryToken)))
            {
                return View(new PasswordRecoveryConfirmModel
                {
                    DisablePasswordChanging = true,
                    Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordAlreadyHasBeenChanged")
                });
            }

            var model = _UserModelFactory.PreparePasswordRecoveryConfirmModel();

            //validate token
            if (!User.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
            }

            //validate token expiration date
            if (User.IsPasswordRecoveryLinkExpired(_UserSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
            }

            return View(model);
        }

        [HttpPost, ActionName("PasswordRecoveryConfirm")]
        [PublicAntiForgery]
        [FormValueRequired("set-password")]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult PasswordRecoveryConfirmPOST(string token, string email, PasswordRecoveryConfirmModel model)
        {
            var User = _UserService.GetUserByEmail(email);
            if (User == null)
                return RedirectToRoute("HomePage");

            //validate token
            if (!User.IsPasswordRecoveryTokenValid(token))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.WrongToken");
                return View(model);
            }

            //validate token expiration date
            if (User.IsPasswordRecoveryLinkExpired(_UserSettings))
            {
                model.DisablePasswordChanging = true;
                model.Result = _localizationService.GetResource("Account.PasswordRecovery.LinkExpired");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var response = _UserRegistrationService.ChangePassword(new ChangePasswordRequest(email,
                    false, _UserSettings.DefaultPasswordFormat, model.NewPassword));
                if (response.Success)
                {
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.PasswordRecoveryToken,
                        "");

                    model.DisablePasswordChanging = true;
                    model.Result = _localizationService.GetResource("Account.PasswordRecovery.PasswordHasBeenChanged");
                }
                else
                {
                    model.Result = response.Errors.FirstOrDefault();
                }

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Register

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Register()
        {
            //check whether registration is allowed
            if (_UserSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new {resultId = (int) UserRegistrationType.Disabled});

            var model = new RegisterModel();
            model = _UserModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);

            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        [HoneypotValidator]
        [PublicAntiForgery]
        [ValidateInput(false)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult Register(RegisterModel model, string returnUrl, bool captchaValid, FormCollection form)
        {
            //check whether registration is allowed
            if (_UserSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new {resultId = (int) UserRegistrationType.Disabled});

            if (_workContext.CurrentUser.IsRegistered())
            {
                //Already registered User. 
                _authenticationService.SignOut();

                //raise logged out event       
                _eventPublisher.Publish(new UserLoggedOutEvent(_workContext.CurrentUser));

                //Save a new record
                _workContext.CurrentUser = _UserService.InsertGuestUser();
            }
            var User = _workContext.CurrentUser;
            User.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            //custom User attributes
            var UserAttributesXml = ParseCustomUserAttributes(form);
            var UserAttributeWarnings = _UserAttributeParser.GetAttributeWarnings(UserAttributesXml);
            foreach (var error in UserAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                if (_UserSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                bool isApproved = _UserSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new UserRegistrationRequest(User,
                    model.Email,
                    _UserSettings.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    _UserSettings.DefaultPasswordFormat,
                    _storeContext.CurrentStore.Id,
                    isApproved);
                var registrationResult = _UserRegistrationService.RegisterUser(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowUsersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.TimeZoneId, model.TimeZoneId);
                    }
                    //VAT number
                    //if (_taxSettings.EuVatEnabled)
                    //{
                    //    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.VatNumber, model.VatNumber);

                    //    string vatName;
                    //    string vatAddress;
                    //    var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out vatName,
                    //        out vatAddress);
                    //    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.VatNumberStatusId, (int) vatNumberStatus);
                    //    //send VAT number admin notification
                    //    if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                    //        _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(User, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);

                    //}

                    //form fields
                    if (_UserSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Gender, model.Gender);
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.LastName, model.LastName);
                    if (_UserSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.DateOfBirth, dateOfBirth);
                    }
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
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StateProvinceId,
                            model.StateProvinceId);
                    if (_UserSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Phone, model.Phone);
                    if (_UserSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Fax, model.Fax);

                    ////newsletter
                    //if (_UserSettings.NewsletterEnabled)
                    //{
                    //    //save newsletter value
                    //    var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                    //    if (newsletter != null)
                    //    {
                    //        if (model.Newsletter)
                    //        {
                    //            newsletter.Active = true;
                    //            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                    //        }
                    //        //else
                    //        //{
                    //        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                    //        //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                    //        //}
                    //    }
                    //    else
                    //    {
                    //        if (model.Newsletter)
                    //        {
                    //            _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                    //            {
                    //                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    //                Email = model.Email,
                    //                Active = true,
                    //                StoreId = _storeContext.CurrentStore.Id,
                    //                CreatedOnUtc = DateTime.UtcNow
                    //            });
                    //        }
                    //    }
                    //}

                    //save User attributes
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.CustomUserAttributes, UserAttributesXml);

                    //login User now
                    if (isApproved)
                        _authenticationService.SignIn(User, true);

                    //associated with external account (if possible)
                    TryAssociateAccountWithExternalAccount(User);

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = User.GetAttribute<string>(SystemUserAttributeNames.FirstName),
                        LastName = User.GetAttribute<string>(SystemUserAttributeNames.LastName),
                        Email = User.Email,
                        Company = User.GetAttribute<string>(SystemUserAttributeNames.Company),
                        CountryId = User.GetAttribute<int>(SystemUserAttributeNames.CountryId) > 0
                            ? (int?) User.GetAttribute<int>(SystemUserAttributeNames.CountryId)
                            : null,
                        StateProvinceId = User.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId) > 0
                            ? (int?) User.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId)
                            : null,
                        City = User.GetAttribute<string>(SystemUserAttributeNames.City),
                        Address1 = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress),
                        Address2 = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress2),
                        ZipPostalCode = User.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode),
                        PhoneNumber = User.GetAttribute<string>(SystemUserAttributeNames.Phone),
                        FaxNumber = User.GetAttribute<string>(SystemUserAttributeNames.Fax),
                        CreatedOnUtc = User.CreatedOnUtc
                    };
                    if (this._addressService.IsAddressValid(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        User.Addresses.Add(defaultAddress);
                        User.BillingAddress = defaultAddress;
                        User.ShippingAddress = defaultAddress;
                        _UserService.UpdateUser(User);
                    }

                    //notifications
                    if (_UserSettings.NotifyNewUserRegistration)
                        _workflowMessageService.SendUserRegisteredNotificationMessage(User,
                            _localizationSettings.DefaultAdminLanguageId);

                    //raise event       
                    _eventPublisher.Publish(new UserRegisteredEvent(User));

                    switch (_UserSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                        {
                            //email validation message
                            _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                            _workflowMessageService.SendUserEmailValidationMessage(User, _workContext.WorkingLanguage.Id);

                            //result
                            return RedirectToRoute("RegisterResult",
                                new {resultId = (int) UserRegistrationType.EmailValidation});
                        }
                        case UserRegistrationType.AdminApproval:
                        {
                            return RedirectToRoute("RegisterResult",
                                new {resultId = (int) UserRegistrationType.AdminApproval});
                        }
                        case UserRegistrationType.Standard:
                        {
                            //send User welcome message
                            _workflowMessageService.SendUserWelcomeMessage(User, _workContext.WorkingLanguage.Id);

                            var redirectUrl = Url.RouteUrl("RegisterResult", new {resultId = (int) UserRegistrationType.Standard});
                            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                redirectUrl = _webHelper.ModifyQueryString(redirectUrl, "returnurl=" + HttpUtility.UrlEncode(returnUrl), null);
                            return Redirect(redirectUrl);
                        }
                        default:
                        {
                            return RedirectToRoute("HomePage");
                        }
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = _UserModelFactory.PrepareRegisterModel(model, true, UserAttributesXml);
            return View(model);
        }

        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult RegisterResult(int resultId)
        {
            var model = _UserModelFactory.PrepareRegisterResultModel(resultId);
            return View(model);
        }

        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        [HttpPost]
        public virtual ActionResult RegisterResult(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToRoute("HomePage");

            return Redirect(returnUrl);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult CheckUsernameAvailability(string username)
        {
            var usernameAvailable = false;
            var statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.NotAvailable");

            if (_UserSettings.UsernamesEnabled && !String.IsNullOrWhiteSpace(username))
            {
                if (_workContext.CurrentUser != null &&
                    _workContext.CurrentUser.Username != null &&
                    _workContext.CurrentUser.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                {
                    statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var User = _UserService.GetUserByUsername(username);
                    if (User == null)
                    {
                        statusText = _localizationService.GetResource("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return Json(new {Available = usernameAvailable, Text = statusText});
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult AccountActivation(string token, string email)
        {
            var User = _UserService.GetUserByEmail(email);
            if (User == null)
                return RedirectToRoute("HomePage");

            var cToken = User.GetAttribute<string>(SystemUserAttributeNames.AccountActivationToken);
            if (string.IsNullOrEmpty(cToken))
                return
                    View(new AccountActivationModel
                    {
                        Result = _localizationService.GetResource("Account.AccountActivation.AlreadyActivated")
                    });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("HomePage");

            //activate user account
            User.Active = true;
            _UserService.UpdateUser(User);
            _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.AccountActivationToken, "");
            //send welcome message
            _workflowMessageService.SendUserWelcomeMessage(User, _workContext.WorkingLanguage.Id);

            var model = new AccountActivationModel();
            model.Result = _localizationService.GetResource("Account.AccountActivation.Activated");
            return View(model);
        }

        #endregion

        #region My account / Info

        [ChildActionOnly]
        public virtual ActionResult UserNavigation(int selectedTabId = 0)
        {
            var model = _UserModelFactory.PrepareUserNavigationModel(selectedTabId);
            return PartialView(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult Info()
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new UserInfoModel();
            model = _UserModelFactory.PrepareUserInfoModel(model, _workContext.CurrentUser, false);

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult Info(UserInfoModel model, FormCollection form)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var User = _workContext.CurrentUser;

            //custom User attributes
            var UserAttributesXml = ParseCustomUserAttributes(form);
            var UserAttributeWarnings = _UserAttributeParser.GetAttributeWarnings(UserAttributesXml);
            foreach (var error in UserAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //username 
                    if (_UserSettings.UsernamesEnabled && this._UserSettings.AllowUsersToChangeUsernames)
                    {
                        if (
                            !User.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _UserRegistrationService.SetUsername(User, model.Username.Trim());

                            //re-authenticate
                            //do not authenticate users in impersonation mode
                            if (_workContext.OriginalUserIfImpersonated == null)
                                _authenticationService.SignIn(User, true);
                        }
                    }
                    //email
                    if (!User.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation = _UserSettings.UserRegistrationType ==
                                                UserRegistrationType.EmailValidation;
                        _UserRegistrationService.SetEmail(User, model.Email.Trim(), requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalUserIfImpersonated == null)
                        {
                            //re-authenticate (if usernames are disabled)
                            if (!_UserSettings.UsernamesEnabled && !requireValidation)
                                _authenticationService.SignIn(User, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowUsersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.TimeZoneId,
                            model.TimeZoneId);
                    }
                    //VAT number
                    //if (_taxSettings.EuVatEnabled)
                    //{
                    //    var prevVatNumber = User.GetAttribute<string>(SystemUserAttributeNames.VatNumber);

                    //    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.VatNumber,
                    //        model.VatNumber);
                    //    if (prevVatNumber != model.VatNumber)
                    //    {
                    //        string vatName;
                    //        string vatAddress;
                    //        var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out vatName,
                    //            out vatAddress);
                    //        _genericAttributeService.SaveAttribute(User,
                    //            SystemUserAttributeNames.VatNumberStatusId, (int) vatNumberStatus);
                    //        //send VAT number admin notification
                    //        if (!String.IsNullOrEmpty(model.VatNumber) &&
                    //            _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                    //            _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(User,
                    //                model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    //    }
                    //}

                    //form fields
                    if (_UserSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Gender,
                            model.Gender);
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.FirstName,
                        model.FirstName);
                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.LastName,
                        model.LastName);
                    if (_UserSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.DateOfBirth,
                            dateOfBirth);
                    }
                    if (_UserSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Company,
                            model.Company);
                    if (_UserSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StreetAddress,
                            model.StreetAddress);
                    if (_UserSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StreetAddress2,
                            model.StreetAddress2);
                    if (_UserSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.ZipPostalCode,
                            model.ZipPostalCode);
                    if (_UserSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.City, model.City);
                    if (_UserSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.CountryId,
                            model.CountryId);
                    if (_UserSettings.CountryEnabled && _UserSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.StateProvinceId,
                            model.StateProvinceId);
                    if (_UserSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Phone, model.Phone);
                    if (_UserSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Fax, model.Fax);

                    //newsletter
                    //if (_UserSettings.NewsletterEnabled)
                    //{
                    //    //save newsletter value
                    //    var newsletter =
                    //        _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(User.Email,
                    //            _storeContext.CurrentStore.Id);
                    //    if (newsletter != null)
                    //    {
                    //        if (model.Newsletter)
                    //        {
                    //            newsletter.Active = true;
                    //            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                    //        }
                    //        else
                    //            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                    //    }
                    //    else
                    //    {
                    //        if (model.Newsletter)
                    //        {
                    //            _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                    //            {
                    //                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    //                Email = User.Email,
                    //                Active = true,
                    //                StoreId = _storeContext.CurrentStore.Id,
                    //                CreatedOnUtc = DateTime.UtcNow
                    //            });
                    //        }
                    //    }
                    //}

                    //if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    //    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.Signature,
                    //        model.Signature);

                    //save User attributes
                    _genericAttributeService.SaveAttribute(_workContext.CurrentUser,
                        SystemUserAttributeNames.CustomUserAttributes, UserAttributesXml);

                    return RedirectToRoute("UserInfo");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
            }


            //If we got this far, something failed, redisplay form
            model = _UserModelFactory.PrepareUserInfoModel(model, User, true, UserAttributesXml);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual ActionResult RemoveExternalAssociation(int id)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            //ensure it's our record
            var ear = _openAuthenticationService.GetExternalIdentifiersFor(_workContext.CurrentUser)
                .FirstOrDefault(x => x.Id == id);

            if (ear == null)
            {
                return Json(new
                {
                    redirect = Url.Action("Info"),
                });
            }

            _openAuthenticationService.DeleteExternalAuthenticationRecord(ear);

            return Json(new
            {
                redirect = Url.Action("Info"),
            });
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult EmailRevalidation(string token, string email)
        {
            var User = _UserService.GetUserByEmail(email);
            if (User == null)
                return RedirectToRoute("HomePage");

            var cToken = User.GetAttribute<string>(SystemUserAttributeNames.EmailRevalidationToken);
            if (string.IsNullOrEmpty(cToken))
                return View(new EmailRevalidationModel
                {
                        Result = _localizationService.GetResource("Account.EmailRevalidation.AlreadyChanged")
                    });

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return RedirectToRoute("HomePage");

            if (String.IsNullOrEmpty(User.EmailToRevalidate))
                return RedirectToRoute("HomePage");

            if (_UserSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return RedirectToRoute("HomePage");

            //change email
            try
            {
                _UserRegistrationService.SetEmail(User, User.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                return View(new EmailRevalidationModel
                {
                    Result = _localizationService.GetResource(exc.Message)
                });
            }
            User.EmailToRevalidate = null;
            _UserService.UpdateUser(User);
            _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.EmailRevalidationToken, "");

            //re-authenticate (if usernames are disabled)
            if (!_UserSettings.UsernamesEnabled)
            {
                _authenticationService.SignIn(User, true);
            }

            var model = new EmailRevalidationModel()
            {
                Result = _localizationService.GetResource("Account.EmailRevalidation.Changed")
            };
            return View(model);
        }

        #endregion

        #region My account / Addresses

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult Addresses()
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = _UserModelFactory.PrepareUserAddressListModel();
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddressDelete(int addressId)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var User = _workContext.CurrentUser;

            //find address (ensure that it belongs to the current User)
            var address = User.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address != null)
            {
                User.RemoveAddress(address);
                _UserService.UpdateUser(User);
                //now delete the address record
                _addressService.DeleteAddress(address);
            }

            //redirect to the address list page
            return Json(new
            {
                redirect = Url.RouteUrl("UserAddresses"),
            });
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddressAdd()
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = new UserAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: null,
                excludeProperties: false,
                addressSettings:_addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult AddressAdd(UserAddressEditModel model, FormCollection form)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var User = _workContext.CurrentUser;

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

                return RedirectToRoute("UserAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address, 
                address: null,
                excludeProperties: true,
                addressSettings:_addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);

            return View(model);
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult AddressEdit(int addressId)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var User = _workContext.CurrentUser;
            //find address (ensure that it belongs to the current User)
            var address = User.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("UserAddresses");

            var model = new UserAddressEditModel();
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        [ValidateInput(false)]
        public virtual ActionResult AddressEdit(UserAddressEditModel model, int addressId, FormCollection form)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var User = _workContext.CurrentUser;
            //find address (ensure that it belongs to the current User)
            var address = User.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
                //address is not found
                return RedirectToRoute("UserAddresses");

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

                return RedirectToRoute("UserAddresses");
            }

            //If we got this far, something failed, redisplay form
            _addressModelFactory.PrepareAddressModel(model.Address,
                address: address,
                excludeProperties: true,
                addressSettings: _addressSettings,
                loadCountries: () => _countryService.GetAllCountries(_workContext.WorkingLanguage.Id),
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        #endregion

        #region My account / Downloadable products

        //[NopHttpsRequirement(SslRequirement.Yes)]
        //public virtual ActionResult DownloadableProducts()
        //{
        //    if (!_workContext.CurrentUser.IsRegistered())
        //        return new HttpUnauthorizedResult();

        //    if (_UserSettings.HideDownloadableProductsTab)
        //        return RedirectToRoute("UserInfo");

        //    var model = _UserModelFactory.PrepareUserDownloadableProductsModel();
        //    return View(model);
        //}

        //public virtual ActionResult UserAgreement(Guid orderItemId)
        //{
        //    var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
        //    if (orderItem == null)
        //        return RedirectToRoute("HomePage");

        //    var product = orderItem.Product;
        //    if (product == null || !product.HasUserAgreement)
        //        return RedirectToRoute("HomePage");

        //    var model = _UserModelFactory.PrepareUserAgreementModel(orderItem, product);
        //    return View(model);
        //}

        #endregion

        #region My account / Change password

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult ChangePassword()
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = _UserModelFactory.PrepareChangePasswordModel();

            //display the cause of the change password 
            if (_workContext.CurrentUser.PasswordIsExpired())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            var User = _workContext.CurrentUser;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(User.Email,
                    true, _UserSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _UserRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
                    return View(model);
                }
                
                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region My account / Avatar

        [NopHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult Avatar()
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            if (!_UserSettings.AllowUsersToUploadAvatars)
                return RedirectToRoute("UserInfo");

            var model = new UserAvatarModel();
            model = _UserModelFactory.PrepareUserAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("upload-avatar")]
        public virtual ActionResult UploadAvatar(UserAvatarModel model, HttpPostedFileBase uploadedFile)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            if (!_UserSettings.AllowUsersToUploadAvatars)
                return RedirectToRoute("UserInfo");

            var User = _workContext.CurrentUser;
            
            if (ModelState.IsValid)
            {
                try
                {
                    var UserAvatar = _pictureService.GetPictureById(User.GetAttribute<int>(SystemUserAttributeNames.AvatarPictureId));
                    if ((uploadedFile != null) && (!String.IsNullOrEmpty(uploadedFile.FileName)))
                    {
                        int avatarMaxSize = _UserSettings.AvatarMaximumSizeBytes;
                        if (uploadedFile.ContentLength > avatarMaxSize)
                            throw new InvenioException(string.Format(_localizationService.GetResource("Account.Avatar.MaximumUploadedFileSize"), avatarMaxSize));

                        byte[] UserPictureBinary = uploadedFile.GetPictureBits();
                        if (UserAvatar != null)
                            UserAvatar = _pictureService.UpdatePicture(UserAvatar.Id, UserPictureBinary, uploadedFile.ContentType, null);
                        else
                            UserAvatar = _pictureService.InsertPicture(UserPictureBinary, uploadedFile.ContentType, null);
                    }

                    int UserAvatarId = 0;
                    if (UserAvatar != null)
                        UserAvatarId = UserAvatar.Id;

                    _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.AvatarPictureId, UserAvatarId);

                    model.AvatarUrl = _pictureService.GetPictureUrl(
                        User.GetAttribute<int>(SystemUserAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        false);
                    return View(model);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }


            //If we got this far, something failed, redisplay form
            model = _UserModelFactory.PrepareUserAvatarModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Avatar")]
        [PublicAntiForgery]
        [FormValueRequired("remove-avatar")]
        public virtual ActionResult RemoveAvatar(UserAvatarModel model, HttpPostedFileBase uploadedFile)
        {
            if (!_workContext.CurrentUser.IsRegistered())
                return new HttpUnauthorizedResult();

            if (!_UserSettings.AllowUsersToUploadAvatars)
                return RedirectToRoute("UserInfo");

            var User = _workContext.CurrentUser;

            var UserAvatar = _pictureService.GetPictureById(User.GetAttribute<int>(SystemUserAttributeNames.AvatarPictureId));
            if (UserAvatar != null)
                _pictureService.DeletePicture(UserAvatar);
            _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.AvatarPictureId, 0);

            return RedirectToRoute("UserAvatar");
        }

        #endregion
    }
}
