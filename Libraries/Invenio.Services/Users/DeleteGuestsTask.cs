using System;
using Invenio.Core.Domain.Users;
using Invenio.Services.Tasks;

namespace Invenio.Services.Users
{
    /// <summary>
    /// Represents a task for deleting guest Users
    /// </summary>
    public partial class DeleteGuestsTask : ITask
    {
        private readonly IUserService _UserService;
        private readonly UserSettings _UserSettings;

        public DeleteGuestsTask(IUserService UserService, UserSettings UserSettings)
        {
            this._UserService = UserService;
            this._UserSettings = UserSettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var olderThanMinutes = _UserSettings.DeleteGuestTaskOlderThanMinutes;
            // Default value in case 0 is returned.  0 would effectively disable this service and harm performance.
            olderThanMinutes = olderThanMinutes == 0 ? 1440 : olderThanMinutes;
    
            _UserService.DeleteGuestUsers(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
        }
    }
}
