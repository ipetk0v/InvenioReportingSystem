using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Admin.Extensions;
using Invenio.Admin.Models.Logging;
using Invenio.Services.Helpers;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Security;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Controllers
{
    public partial class ActivityLogController : BaseAdminController
    {
        #region Fields

        private readonly IUserActivityService _UserActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion Fields

        #region Constructors

        public ActivityLogController(IUserActivityService UserActivityService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService)
		{
            this._UserActivityService = UserActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
		}

		#endregion 

        #region Activity log types

        public virtual ActionResult ListTypes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var model = _UserActivityService
                .GetAllActivityTypes()
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult SaveTypes(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //activity log
            _UserActivityService.InsertActivity("EditActivityLogTypes", _localizationService.GetResource("ActivityLog.EditActivityLogTypes"));

            string formKey = "checkbox_activity_types";
            var checkedActivityTypes = form[formKey] != null ? form[formKey].Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
            
            var activityTypes = _UserActivityService.GetAllActivityTypes();
            foreach (var activityType in activityTypes)
            {
                activityType.Enabled = checkedActivityTypes.Contains(activityType.Id);
                _UserActivityService.UpdateActivityType(activityType);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ActivityLog.ActivityLogType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

        #region Activity log

        public virtual ActionResult ListLogs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLogSearchModel = new ActivityLogSearchModel();
            activityLogSearchModel.ActivityLogType.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });


            foreach (var at in _UserActivityService.GetAllActivityTypes())
            {
                activityLogSearchModel.ActivityLogType.Add(new SelectListItem
                {
                    Value = at.Id.ToString(),
                    Text = at.Name
                });
            }
            return View(activityLogSearchModel);
        }

        [HttpPost]
        public virtual ActionResult ListLogs(DataSourceRequest command, ActivityLogSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedKendoGridJson();

            DateTime? startDateValue = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var activityLog = _UserActivityService.GetAllActivities(startDateValue, endDateValue,null, model.ActivityLogTypeId, command.Page - 1, command.PageSize, model.IpAddress);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return m;
                    
                }),
                Total = activityLog.TotalCount
            };
            return Json(gridModel);
        }

        //public virtual ActionResult AcivityLogDelete(int id)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
        //        return AccessDeniedView();

        //    var activityLog = _UserActivityService.GetActivityById(id);
        //    if (activityLog == null)
        //    {
        //        throw new ArgumentException("No activity log found with the specified id");
        //    }
        //    _UserActivityService.DeleteActivity(activityLog);

        //    //activity log
        //    _UserActivityService.InsertActivity("DeleteActivityLog", _localizationService.GetResource("ActivityLog.DeleteActivityLog"));

        //    return new NullJsonResult();
        //}

        //public virtual ActionResult ClearAll()
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
        //        return AccessDeniedView();

        //    _UserActivityService.ClearAllActivities();

        //    //activity log
        //    _UserActivityService.InsertActivity("DeleteActivityLog", _localizationService.GetResource("ActivityLog.DeleteActivityLog"));

        //    return RedirectToAction("ListLogs");
        //}

        #endregion
    }
}
