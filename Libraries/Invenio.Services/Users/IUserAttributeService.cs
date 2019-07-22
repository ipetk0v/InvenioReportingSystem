using System.Collections.Generic;
using Invenio.Core.Domain.Users;

namespace Invenio.Services.Users
{
    /// <summary>
    /// User attribute service
    /// </summary>
    public partial interface IUserAttributeService
    {
        /// <summary>
        /// Deletes a User attribute
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        void DeleteUserAttribute(UserAttribute UserAttribute);

        /// <summary>
        /// Gets all User attributes
        /// </summary>
        /// <returns>User attributes</returns>
        IList<UserAttribute> GetAllUserAttributes();

        /// <summary>
        /// Gets a User attribute 
        /// </summary>
        /// <param name="UserAttributeId">User attribute identifier</param>
        /// <returns>User attribute</returns>
        UserAttribute GetUserAttributeById(int UserAttributeId);

        /// <summary>
        /// Inserts a User attribute
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        void InsertUserAttribute(UserAttribute UserAttribute);

        /// <summary>
        /// Updates the User attribute
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        void UpdateUserAttribute(UserAttribute UserAttribute);

        /// <summary>
        /// Deletes a User attribute value
        /// </summary>
        /// <param name="UserAttributeValue">User attribute value</param>
        void DeleteUserAttributeValue(UserAttributeValue UserAttributeValue);

        /// <summary>
        /// Gets User attribute values by User attribute identifier
        /// </summary>
        /// <param name="UserAttributeId">The User attribute identifier</param>
        /// <returns>User attribute values</returns>
        IList<UserAttributeValue> GetUserAttributeValues(int UserAttributeId);

        /// <summary>
        /// Gets a User attribute value
        /// </summary>
        /// <param name="UserAttributeValueId">User attribute value identifier</param>
        /// <returns>User attribute value</returns>
        UserAttributeValue GetUserAttributeValueById(int UserAttributeValueId);

        /// <summary>
        /// Inserts a User attribute value
        /// </summary>
        /// <param name="UserAttributeValue">User attribute value</param>
        void InsertUserAttributeValue(UserAttributeValue UserAttributeValue);

        /// <summary>
        /// Updates the User attribute value
        /// </summary>
        /// <param name="UserAttributeValue">User attribute value</param>
        void UpdateUserAttributeValue(UserAttributeValue UserAttributeValue);
    }
}
