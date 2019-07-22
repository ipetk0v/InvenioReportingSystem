
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Users;

namespace Invenio.Services.Users
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class UserAttributeExtensions
    {
        /// <summary>
        /// A value indicating whether this User attribute should have values
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        /// <returns>Result</returns>
        public static bool ShouldHaveValues(this UserAttribute UserAttribute)
        {
            if (UserAttribute == null)
                return false;

            if (UserAttribute.AttributeControlType == AttributeControlType.TextBox ||
                UserAttribute.AttributeControlType == AttributeControlType.MultilineTextbox ||
                UserAttribute.AttributeControlType == AttributeControlType.Datepicker ||
                UserAttribute.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            //other attribute controle types support values
            return true;
        }
    }
}
