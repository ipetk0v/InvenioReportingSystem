using System;
using System.Linq;
using Invenio.Core.Domain.Common;

namespace Invenio.Core.Domain.Users
{
    public static class UserExtensions
    {
        #region User role

        /// <summary>
        /// Gets a value indicating whether User is in a certain User role
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="UserRoleSystemName">User role system name</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsInUserRole(this User User,
            string UserRoleSystemName, bool onlyActiveUserRoles = true)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            if (String.IsNullOrEmpty(UserRoleSystemName))
                throw new ArgumentNullException("UserRoleSystemName");

            var result = User.UserRoles
                .FirstOrDefault(cr => (!onlyActiveUserRoles || cr.Active) && (cr.SystemName == UserRoleSystemName)) != null;
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether User a search engine
        /// </summary>
        /// <param name="User">User</param>
        /// <returns>Result</returns>
        public static bool IsSearchEngineAccount(this User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            if (!User.IsSystemAccount || String.IsNullOrEmpty(User.SystemName))
                return false;

            var result = User.SystemName.Equals(SystemUserNames.SearchEngine, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the User is a built-in record for background tasks
        /// </summary>
        /// <param name="User">User</param>
        /// <returns>Result</returns>
        public static bool IsBackgroundTaskAccount(this User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            if (!User.IsSystemAccount || String.IsNullOrEmpty(User.SystemName))
                return false;

            var result = User.SystemName.Equals(SystemUserNames.BackgroundTask, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether User is administrator
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsAdmin(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.Administrators, onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether User is administrator
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsRegionalManager(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.RegionalManager, onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether User is administrator
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsSupervisor(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.Supervisor, onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether User is administrator
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsTeamLeader(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.TeamLeader, onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether User is administrator
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsOperator(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.Operator, onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether User is administrator
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsGeneralManager(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.GeneralManager, onlyActiveUserRoles);
        }

        ///// <summary>
        ///// Gets a value indicating whether User is a forum moderator
        ///// </summary>
        ///// <param name="User">User</param>
        ///// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        ///// <returns>Result</returns>
        //public static bool IsForumModerator(this User User, bool onlyActiveUserRoles = true)
        //{
        //    return IsInUserRole(User, SystemUserRoleNames.ForumModerators, onlyActiveUserRoles);
        //}

        /// <summary>
        /// Gets a value indicating whether User is registered
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsRegistered(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.Registered, onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether User is guest
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        /// <returns>Result</returns>
        public static bool IsGuest(this User User, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(User, SystemUserRoleNames.Guests, onlyActiveUserRoles);
        }

        ///// <summary>
        ///// Gets a value indicating whether User is vendor
        ///// </summary>
        ///// <param name="User">User</param>
        ///// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active User roles</param>
        ///// <returns>Result</returns>
        //public static bool IsVendor(this User User, bool onlyActiveUserRoles = true)
        //{
        //    return IsInUserRole(User, SystemUserRoleNames.Vendors, onlyActiveUserRoles);
        //}
        #endregion

        #region Addresses

        public static void RemoveAddress(this User User, Address address)
        {
            if (User.Addresses.Contains(address))
            {
                if (User.BillingAddress == address) User.BillingAddress = null;
                if (User.ShippingAddress == address) User.ShippingAddress = null;

                User.Addresses.Remove(address);
            }
        }

        #endregion
    }
}
