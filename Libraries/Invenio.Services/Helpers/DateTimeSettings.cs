
using Invenio.Core.Configuration;

namespace Invenio.Services.Helpers
{
    public class DateTimeSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a default store time zone identifier
        /// </summary>
        public string DefaultStoreTimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Users are allowed to select theirs time zone
        /// </summary>
        public bool AllowUsersToSetTimeZone { get; set; }
    }
}