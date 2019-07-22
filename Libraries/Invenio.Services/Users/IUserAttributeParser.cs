using System.Collections.Generic;
using Invenio.Core.Domain.Users;

namespace Invenio.Services.Users
{
    /// <summary>
    /// User attribute parser interface
    /// </summary>
    public partial interface IUserAttributeParser
    {
        /// <summary>
        /// Gets selected User attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected User attributes</returns>
        IList<UserAttribute> ParseUserAttributes(string attributesXml);

        /// <summary>
        /// Get User attribute values
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>User attribute values</returns>
        IList<UserAttributeValue> ParseUserAttributeValues(string attributesXml);

        /// <summary>
        /// Gets selected User attribute value
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="UserAttributeId">User attribute identifier</param>
        /// <returns>User attribute value</returns>
        IList<string> ParseValues(string attributesXml, int UserAttributeId);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ca">User attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        string AddUserAttribute(string attributesXml, UserAttribute ca, string value);

        /// <summary>
        /// Validates User attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Warnings</returns>
        IList<string> GetAttributeWarnings(string attributesXml);
    }
}
