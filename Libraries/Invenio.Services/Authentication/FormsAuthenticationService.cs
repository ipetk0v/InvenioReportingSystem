using System;
using System.Web;
using System.Web.Security;
using Invenio.Core.Domain.Users;
using Invenio.Services.Users;

namespace Invenio.Services.Authentication
{
    /// <summary>
    /// Authentication service
    /// </summary>
    public partial class FormsAuthenticationService : IAuthenticationService
    {
        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly IUserService _UserService;
        private readonly UserSettings _UserSettings;
        private readonly TimeSpan _expirationTimeSpan;

        private User _cachedUser;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="UserService">User service</param>
        /// <param name="UserSettings">User settings</param>
        public FormsAuthenticationService(HttpContextBase httpContext,
            IUserService UserService, UserSettings UserSettings)
        {
            this._httpContext = httpContext;
            this._UserService = UserService;
            this._UserSettings = UserSettings;
            this._expirationTimeSpan = FormsAuthentication.Timeout;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get authenticated User
        /// </summary>
        /// <param name="ticket">Ticket</param>
        /// <returns>User</returns>
        protected virtual User GetAuthenticatedUserFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var usernameOrEmail = ticket.UserData;

            if (String.IsNullOrWhiteSpace(usernameOrEmail))
                return null;
            var User = _UserSettings.UsernamesEnabled
                ? _UserService.GetUserByUsername(usernameOrEmail)
                : _UserService.GetUserByEmail(usernameOrEmail);
            return User;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="createPersistentCookie">A value indicating whether to create a persistent cookie</param>
        public virtual void SignIn(User User, bool createPersistentCookie)
        {
            var now = DateTime.UtcNow.ToLocalTime();

            var ticket = new FormsAuthenticationTicket(
                1 /*version*/,
                _UserSettings.UsernamesEnabled ? User.Username : User.Email,
                now,
                now.Add(_expirationTimeSpan),
                createPersistentCookie,
                _UserSettings.UsernamesEnabled ? User.Username : User.Email,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            _httpContext.Response.Cookies.Add(cookie);
            _cachedUser = User;
        }

        /// <summary>
        /// Sign out
        /// </summary>
        public virtual void SignOut()
        {
            _cachedUser = null;
            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Get authenticated User
        /// </summary>
        /// <returns>User</returns>
        public virtual User GetAuthenticatedUser()
        {
            if (_cachedUser != null)
                return _cachedUser;

            if (_httpContext == null ||
                _httpContext.Request == null ||
                !_httpContext.Request.IsAuthenticated ||
                !(_httpContext.User.Identity is FormsIdentity))
            {
                return null;
            }

            var formsIdentity = (FormsIdentity)_httpContext.User.Identity;
            var User = GetAuthenticatedUserFromTicket(formsIdentity.Ticket);
            if (User != null && User.Active && !User.RequireReLogin && !User.Deleted  && User.IsRegistered())
                _cachedUser = User;
            return _cachedUser;
        }

        #endregion

    }
}