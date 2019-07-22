using Invenio.Core.Domain.Users;

namespace Invenio.Services.Authentication
{
    /// <summary>
    /// Authentication service interface
    /// </summary>
    public partial interface IAuthenticationService 
    {
        /// <summary>
        /// Sign in
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="createPersistentCookie">A value indicating whether to create a persistent cookie</param>
        void SignIn(User User, bool createPersistentCookie);

        /// <summary>
        /// Sign out
        /// </summary>
        void SignOut();

        /// <summary>
        /// Get authenticated User
        /// </summary>
        /// <returns>User</returns>
        User GetAuthenticatedUser();
    }
}