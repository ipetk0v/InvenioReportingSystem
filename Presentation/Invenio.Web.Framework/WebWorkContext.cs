using System;
using System.Linq;
using System.Web;
using Invenio.Core;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Localization;
using Invenio.Core.Domain.Users;
using Invenio.Core.Fakes;
using Invenio.Services.Authentication;
using Invenio.Services.Common;
using Invenio.Services.Directory;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Stores;
using Invenio.Services.Users;
using Invenio.Web.Framework.Localization;

namespace Invenio.Web.Framework
{
    /// <summary>
    ///     Work context for web application
    /// </summary>
    public class WebWorkContext : IWorkContext
    {
        #region Const

        private const string UserCookieName = "Nop.User";

        #endregion

        #region Ctor

        public WebWorkContext(HttpContextBase httpContext,
            IUserService UserService,
            IStoreContext storeContext,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            IUserAgentHelper userAgentHelper,
            IStoreMappingService storeMappingService)
        {
            _httpContext = httpContext;
            _UserService = UserService;
            _storeContext = storeContext;
            _authenticationService = authenticationService;
            _languageService = languageService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _currencySettings = currencySettings;
            _localizationSettings = localizationSettings;
            _userAgentHelper = userAgentHelper;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly IUserService _UserService;
        private readonly IStoreContext _storeContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CurrencySettings _currencySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IUserAgentHelper _userAgentHelper;
        private readonly IStoreMappingService _storeMappingService;

        private User _cachedUser;
        private User _originalUserIfImpersonated;
        private Language _cachedLanguage;
        private Currency _cachedCurrency;

        #endregion

        #region Utilities

        protected virtual HttpCookie GetUserCookie()
        {
            if (_httpContext == null || _httpContext.Request == null)
                return null;

            return _httpContext.Request.Cookies[UserCookieName];
        }

        protected virtual void SetUserCookie(Guid UserGuid)
        {
            if (_httpContext != null && _httpContext.Response != null)
            {
                var cookie = new HttpCookie(UserCookieName);
                cookie.HttpOnly = true;
                cookie.Value = UserGuid.ToString();
                if (UserGuid == Guid.Empty)
                {
                    cookie.Expires = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    var cookieExpires = 24 * 365; //TODO make configurable
                    cookie.Expires = DateTime.Now.AddHours(cookieExpires);
                }

                _httpContext.Response.Cookies.Remove(UserCookieName);
                _httpContext.Response.Cookies.Add(cookie);
            }
        }

        protected virtual Language GetLanguageFromUrl()
        {
            if (_httpContext == null || _httpContext.Request == null)
                return null;

            var virtualPath = _httpContext.Request.AppRelativeCurrentExecutionFilePath;
            var applicationPath = _httpContext.Request.ApplicationPath;
            if (!virtualPath.IsLocalizedUrl(applicationPath, false))
                return null;

            var seoCode = virtualPath.GetLanguageSeoCodeFromUrl(applicationPath, false);
            if (string.IsNullOrEmpty(seoCode))
                return null;

            var language = _languageService
                .GetAllLanguages()
                .FirstOrDefault(l => seoCode.Equals(l.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase));
            if (language != null && language.Published && _storeMappingService.Authorize(language))
                return language;

            return null;
        }

        protected virtual Language GetLanguageFromBrowserSettings()
        {
            if (_httpContext == null ||
                _httpContext.Request == null ||
                _httpContext.Request.UserLanguages == null)
                return null;

            var userLanguage = _httpContext.Request.UserLanguages.FirstOrDefault();
            if (string.IsNullOrEmpty(userLanguage))
                return null;

            var language = _languageService
                .GetAllLanguages()
                .FirstOrDefault(
                    l => userLanguage.Equals(l.LanguageCulture, StringComparison.InvariantCultureIgnoreCase));
            if (language != null && language.Published && _storeMappingService.Authorize(language))
                return language;

            return null;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the current User
        /// </summary>
        public virtual User CurrentUser
        {
            get
            {
                if (_cachedUser != null)
                    return _cachedUser;

                User User = null;
                if (_httpContext == null || _httpContext is FakeHttpContext)
                    User = _UserService.GetUserBySystemName(SystemUserNames.BackgroundTask);

                //check whether request is made by a search engine
                //in this case return built-in User record for search engines 
                //or comment the following two lines of code in order to disable this functionality
                if (User == null || User.Deleted || !User.Active || User.RequireReLogin)
                    if (_userAgentHelper.IsSearchEngine())
                        User = _UserService.GetUserBySystemName(SystemUserNames.SearchEngine);

                //registered user
                if (User == null || User.Deleted || !User.Active || User.RequireReLogin)
                    User = _authenticationService.GetAuthenticatedUser();

                //impersonate user if required (currently used for 'phone order' support)
                if (User != null && !User.Deleted && User.Active && !User.RequireReLogin)
                {
                    var impersonatedUserId = User.GetAttribute<int?>(SystemUserAttributeNames.ImpersonatedUserId);
                    if (impersonatedUserId.HasValue && impersonatedUserId.Value > 0)
                    {
                        var impersonatedUser = _UserService.GetUserById(impersonatedUserId.Value);
                        if (impersonatedUser != null && !impersonatedUser.Deleted && impersonatedUser.Active &&
                            !impersonatedUser.RequireReLogin)
                        {
                            //set impersonated User
                            _originalUserIfImpersonated = User;
                            User = impersonatedUser;
                        }
                    }
                }

                //load guest User
                if (User == null || User.Deleted || !User.Active || User.RequireReLogin)
                {
                    var UserCookie = GetUserCookie();
                    if (UserCookie != null && !string.IsNullOrEmpty(UserCookie.Value))
                    {
                        Guid UserGuid;
                        if (Guid.TryParse(UserCookie.Value, out UserGuid))
                        {
                            var UserByCookie = _UserService.GetUserByGuid(UserGuid);
                            if (UserByCookie != null &&
                                //this User (from cookie) should not be registered
                                !UserByCookie.IsRegistered())
                                User = UserByCookie;
                        }
                    }
                }

                //create guest if not exists
                if (User == null || User.Deleted || !User.Active || User.RequireReLogin)
                    User = _UserService.InsertGuestUser();


                //validation
                if (!User.Deleted && User.Active && !User.RequireReLogin)
                {
                    SetUserCookie(User.UserGuid);
                    _cachedUser = User;
                }

                return _cachedUser;
            }
            set
            {
                SetUserCookie(value.UserGuid);
                _cachedUser = value;
            }
        }

        /// <summary>
        ///     Gets or sets the original User (in case the current one is impersonated)
        /// </summary>
        public virtual User OriginalUserIfImpersonated => _originalUserIfImpersonated;

        /// <summary>
        ///     Gets or sets the current vendor (logged-in manager)
        /// </summary>
        /// <summary>
        ///     Get or set current user working language
        /// </summary>
        public virtual Language WorkingLanguage
        {
            get
            {
                if (_cachedLanguage != null)
                    return _cachedLanguage;

                Language detectedLanguage = null;
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                    detectedLanguage = GetLanguageFromUrl();
                if (detectedLanguage == null && _localizationSettings.AutomaticallyDetectLanguage)
                    if (!CurrentUser.GetAttribute<bool>(SystemUserAttributeNames.LanguageAutomaticallyDetected,
                        _genericAttributeService, _storeContext.CurrentStore.Id))
                    {
                        detectedLanguage = GetLanguageFromBrowserSettings();
                        if (detectedLanguage != null)
                            _genericAttributeService.SaveAttribute(CurrentUser,
                                SystemUserAttributeNames.LanguageAutomaticallyDetected,
                                true, _storeContext.CurrentStore.Id);
                    }
                if (detectedLanguage != null)
                    if (CurrentUser.GetAttribute<int>(SystemUserAttributeNames.LanguageId,
                            _genericAttributeService, _storeContext.CurrentStore.Id) != detectedLanguage.Id)
                        _genericAttributeService.SaveAttribute(CurrentUser, SystemUserAttributeNames.LanguageId,
                            detectedLanguage.Id, _storeContext.CurrentStore.Id);

                var allLanguages = _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id);
                //find current User language
                var languageId = CurrentUser.GetAttribute<int>(SystemUserAttributeNames.LanguageId,
                    _genericAttributeService, _storeContext.CurrentStore.Id);
                var language = allLanguages.FirstOrDefault(x => x.Id == languageId);
                if (language == null)
                {
                    //it not found, then let's load the default currency for the current language (if specified)
                    languageId = _storeContext.CurrentStore.DefaultLanguageId;
                    language = allLanguages.FirstOrDefault(x => x.Id == languageId);
                }
                if (language == null)
                    language = allLanguages.FirstOrDefault();
                if (language == null)
                    language = _languageService.GetAllLanguages().FirstOrDefault();

                //cache
                _cachedLanguage = language;
                return _cachedLanguage;
            }
            set
            {
                var languageId = value != null ? value.Id : 0;
                _genericAttributeService.SaveAttribute(CurrentUser,
                    SystemUserAttributeNames.LanguageId,
                    languageId, _storeContext.CurrentStore.Id);

                //reset cache
                _cachedLanguage = null;
            }
        }

        /// <summary>
        ///     Get or set current user working currency
        /// </summary>
        public virtual Currency WorkingCurrency
        {
            get
            {
                if (_cachedCurrency != null)
                    return _cachedCurrency;

                //return primary store currency when we're in admin area/mode
                if (IsAdmin)
                {
                    var primaryStoreCurrency =
                        _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                    if (primaryStoreCurrency != null)
                    {
                        //cache
                        _cachedCurrency = primaryStoreCurrency;
                        return primaryStoreCurrency;
                    }
                }

                var allCurrencies = _currencyService.GetAllCurrencies(storeId: _storeContext.CurrentStore.Id);
                //find a currency previously selected by a User
                var currencyId = CurrentUser.GetAttribute<int>(SystemUserAttributeNames.CurrencyId,
                    _genericAttributeService, _storeContext.CurrentStore.Id);
                var currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
                if (currency == null)
                {
                    //it not found, then let's load the default currency for the current language (if specified)
                    currencyId = WorkingLanguage.DefaultCurrencyId;
                    currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
                }
                if (currency == null)
                    currency = allCurrencies.FirstOrDefault();
                if (currency == null)
                    currency = _currencyService.GetAllCurrencies().FirstOrDefault();

                //cache
                _cachedCurrency = currency;
                return _cachedCurrency;
            }
            set
            {
                var currencyId = value != null ? value.Id : 0;
                _genericAttributeService.SaveAttribute(CurrentUser,
                    SystemUserAttributeNames.CurrencyId,
                    currencyId, _storeContext.CurrentStore.Id);

                //reset cache
                _cachedCurrency = null;
            }
        }

        /// <summary>
        ///     Get or set current tax display type
        /// </summary>
        /// <summary>
        ///     Get or set value indicating whether we're in admin area
        /// </summary>
        public virtual bool IsAdmin { get; set; }

        #endregion
    }
}