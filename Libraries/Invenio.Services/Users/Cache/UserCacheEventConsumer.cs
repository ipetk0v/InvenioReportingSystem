using Invenio.Core.Caching;
using Invenio.Core.Domain.Users;
using Invenio.Core.Infrastructure;
using Invenio.Services.Events;

namespace Invenio.Services.Users.Cache
{
    /// <summary>
    /// User cache event consumer (used for caching of current User password)
    /// </summary>
    public partial class UserCacheEventConsumer : IConsumer<UserPasswordChangedEvent>
    {
        #region Constants

        /// <summary>
        /// Key for current User password lifetime
        /// </summary>
        /// <remarks>
        /// {0} : User identifier
        /// </remarks>
        public const string User_PASSWORD_LIFETIME = "Nop.Users.passwordlifetime-{0}";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public UserCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        #endregion

        #region Methods

        //password changed
        public void HandleEvent(UserPasswordChangedEvent eventMessage)
        {
            _cacheManager.Remove(string.Format(User_PASSWORD_LIFETIME, eventMessage.Password.UserId));
        }

        #endregion
    }
}
