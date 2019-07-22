using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Admin.Models.Users;
using Invenio.Admin.Models.Security;
using Invenio.Core;
using Invenio.Core.Domain.Users;
using Invenio.Services.Users;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Security;

namespace Invenio.Admin.Controllers
{
    public partial class SecurityController : BaseAdminController
	{
		#region Fields

        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly IUserService _UserService;
        private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

        public SecurityController(ILogger logger, IWorkContext workContext,
            IPermissionService permissionService,
            IUserService UserService, ILocalizationService localizationService)
		{
            this._logger = logger;
            this._workContext = workContext;
            this._permissionService = permissionService;
            this._UserService = UserService;
            this._localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public virtual ActionResult AccessDenied(string pageUrl)
        {
            var currentUser = _workContext.CurrentUser;
            if (currentUser == null || currentUser.IsGuest())
            {
                _logger.Information(string.Format("Access denied to anonymous request on {0}", pageUrl));
                return View();
            }

            _logger.Information(string.Format("Access denied to user #{0} '{1}' on {2}", currentUser.Email, currentUser.Email, pageUrl));


            return View();
        }

        public virtual ActionResult Permissions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var model = new PermissionMappingModel();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var UserRoles = _UserService.GetAllUserRoles(true);
            foreach (var pr in permissionRecords)
            {
                model.AvailablePermissions.Add(new PermissionRecordModel
                {
                    //Name = pr.Name,
                    Name = pr.GetLocalizedPermissionName(_localizationService, _workContext),
                    SystemName = pr.SystemName
                });
            }
            foreach (var cr in UserRoles)
            {
                model.AvailableUserRoles.Add(new UserRoleModel
                {
                    Id = cr.Id,
                    Name = cr.Name
                });
            }
            foreach (var pr in permissionRecords)
                foreach (var cr in UserRoles)
                {
                    bool allowed = pr.UserRoles.Count(x => x.Id == cr.Id) > 0;
                    if (!model.Allowed.ContainsKey(pr.SystemName))
                        model.Allowed[pr.SystemName] = new Dictionary<int, bool>();
                    model.Allowed[pr.SystemName][cr.Id] = allowed;
                }

            return View(model);
        }

        [HttpPost, ActionName("Permissions")]
        public virtual ActionResult PermissionsSave(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var UserRoles = _UserService.GetAllUserRoles(true);


            foreach (var cr in UserRoles)
            {
                string formKey = "allow_" + cr.Id;
                var permissionRecordSystemNamesToRestrict = form[formKey] != null ? form[formKey].Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();

                foreach (var pr in permissionRecords)
                {

                    bool allow = permissionRecordSystemNamesToRestrict.Contains(pr.SystemName);
                    if (allow)
                    {
                        if (pr.UserRoles.FirstOrDefault(x => x.Id == cr.Id) == null)
                        {
                            pr.UserRoles.Add(cr);
                            _permissionService.UpdatePermissionRecord(pr);
                        }
                    }
                    else
                    {
                        if (pr.UserRoles.FirstOrDefault(x => x.Id == cr.Id) != null)
                        {
                            pr.UserRoles.Remove(cr);
                            _permissionService.UpdatePermissionRecord(pr);
                        }
                    }
                }
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ACL.Updated"));
            return RedirectToAction("Permissions");
        }

        #endregion
    }
}
