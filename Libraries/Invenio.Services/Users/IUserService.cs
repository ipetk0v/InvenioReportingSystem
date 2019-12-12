using System;
using System.Collections.Generic;
using Invenio.Core;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Orders;

namespace Invenio.Services.Users
{
    /// <summary>
    /// User service interface
    /// </summary>
    public partial interface IUserService
    {
        #region Users

        /// <summary>
        /// Gets all Users
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="UserRoleIds">A list of User role identifiers to filter by (at least one match); pass null or empty list in order to load all Users; </param>
        /// <param name="email">Email; null to load all Users</param>
        /// <param name="username">Username; null to load all Users</param>
        /// <param name="firstName">First name; null to load all Users</param>
        /// <param name="lastName">Last name; null to load all Users</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all Users</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all Users</param>
        /// <param name="company">Company; null to load all Users</param>
        /// <param name="phone">Phone; null to load all Users</param>
        /// <param name="zipPostalCode">Phone; null to load all Users</param>
        /// <param name="ipAddress">IP address; null to load all Users</param>
        /// <param name="loadOnlyWithShoppingCart">Value indicating whether to load Users only with shopping cart</param>
        /// <param name="sct">Value indicating what shopping cart type to filter; userd when 'loadOnlyWithShoppingCart' param is 'true'</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Users</returns>
        IPagedList<User> GetAllUsers(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int affiliateId = 0, int vendorId = 0,
            int[] UserRoleIds = null, string email = null, string username = null,
            string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null,
            string ipAddress = null, bool loadOnlyWithShoppingCart = false, /*ShoppingCartType? sct = null,*/
            int pageIndex = 0, int pageSize = int.MaxValue);
        
        /// <summary>
        /// Gets online Users
        /// </summary>
        /// <param name="lastActivityFromUtc">User last activity date (from)</param>
        /// <param name="UserRoleIds">A list of User role identifiers to filter by (at least one match); pass null or empty list in order to load all Users; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Users</returns>
        IPagedList<User> GetOnlineUsers(DateTime lastActivityFromUtc,
            int[] UserRoleIds, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Delete a User
        /// </summary>
        /// <param name="User">User</param>
        void DeleteUser(User User);

        /// <summary>
        /// Gets a User
        /// </summary>
        /// <param name="UserId">User identifier</param>
        /// <returns>A User</returns>
        User GetUserById(int UserId);

        /// <summary>
        /// Get Users by identifiers
        /// </summary>
        /// <param name="UserIds">User identifiers</param>
        /// <returns>Users</returns>
        IList<User> GetUsersByIds(int[] UserIds);
        
        /// <summary>
        /// Gets a User by GUID
        /// </summary>
        /// <param name="UserGuid">User GUID</param>
        /// <returns>A User</returns>
        User GetUserByGuid(Guid UserGuid);

        /// <summary>
        /// Get User by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>User</returns>
        User GetUserByEmail(string email);
        
        /// <summary>
        /// Get User by system role
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>User</returns>
        User GetUserBySystemName(string systemName);

        /// <summary>
        /// Get User by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User</returns>
        User GetUserByUsername(string username);

        /// <summary>
        /// Insert a guest User
        /// </summary>
        /// <returns>User</returns>
        User InsertGuestUser();

        /// <summary>
        /// Insert a User
        /// </summary>
        /// <param name="User">User</param>
        void InsertUser(User User);

        /// <summary>
        /// Updates the User
        /// </summary>
        /// <param name="User">User</param>
        void UpdateUser(User User);
        
        /// <summary>
        /// Reset data required for checkout
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
        /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        void ResetCheckoutData(User User, int storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true);

        /// <summary>
        /// Delete guest User records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete Users only without shopping cart</param>
        /// <returns>Number of deleted Users</returns>
        int DeleteGuestUsers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart);

        #endregion

        #region User roles

        /// <summary>
        /// Delete a User role
        /// </summary>
        /// <param name="UserRole">User role</param>
        void DeleteUserRole(UserRole UserRole);

        /// <summary>
        /// Gets a User role
        /// </summary>
        /// <param name="UserRoleId">User role identifier</param>
        /// <returns>User role</returns>
        UserRole GetUserRoleById(int UserRoleId);

        /// <summary>
        /// Gets a User role
        /// </summary>
        /// <param name="systemName">User role system name</param>
        /// <returns>User role</returns>
        UserRole GetUserRoleBySystemName(string systemName);

        /// <summary>
        /// Gets all User roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>User roles</returns>
        IList<UserRole> GetAllUserRoles(bool showHidden = false);

        /// <summary>
        /// Inserts a User role
        /// </summary>
        /// <param name="UserRole">User role</param>
        void InsertUserRole(UserRole UserRole);

        /// <summary>
        /// Updates the User role
        /// </summary>
        /// <param name="UserRole">User role</param>
        void UpdateUserRole(UserRole UserRole);

        #endregion

        #region User passwords

        /// <summary>
        /// Gets User passwords
        /// </summary>
        /// <param name="UserId">User identifier; pass null to load all records</param>
        /// <param name="passwordFormat">Password format; pass null to load all records</param>
        /// <param name="passwordsToReturn">Number of returning passwords; pass null to load all records</param>
        /// <returns>List of User passwords</returns>
        IList<UserPassword> GetUserPasswords(int? UserId = null,
            PasswordFormat? passwordFormat = null, int? passwordsToReturn = null);

        /// <summary>
        /// Get current User password
        /// </summary>
        /// <param name="UserId">User identifier</param>
        /// <returns>User password</returns>
        UserPassword GetCurrentPassword(int UserId);

        /// <summary>
        /// Insert a User password
        /// </summary>
        /// <param name="UserPassword">User password</param>
        void InsertUserPassword(UserPassword UserPassword);

        /// <summary>
        /// Update a User password
        /// </summary>
        /// <param name="UserPassword">User password</param>
        void UpdateUserPassword(UserPassword UserPassword);

        #endregion
    }
}