using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Localization;
//using Invenio.Core.Domain.Tax;
//using Invenio.Core.Domain.Vendors;

namespace Invenio.Core
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current User
        /// </summary>
        User CurrentUser { get; set; }
        /// <summary>
        /// Gets or sets the original User (in case the current one is impersonated)
        /// </summary>
        User OriginalUserIfImpersonated { get; }
        /// <summary>
        /// Gets or sets the current vendor (logged-in manager)
        /// </summary>
        //Vendor CurrentVendor { get; }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        Language WorkingLanguage { get; set; }
        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        Currency WorkingCurrency { get; set; }
        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        //TaxDisplayType TaxDisplayType { get; set; }

        /// <summary>
        /// Get or set value indicating whether we're in admin area
        /// </summary>
        bool IsAdmin { get; set; }
    }
}
