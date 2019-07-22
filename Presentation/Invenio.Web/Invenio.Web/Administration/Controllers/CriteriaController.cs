using Invenio.Admin.Models.Criteria;
using Invenio.Core.Domain.Criterias;
using Invenio.Services.Criteria;
using Invenio.Services.Localization;
using Invenio.Services.Orders;
using Invenio.Services.Security;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Kendoui;
using Invenio.Web.Framework.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Invenio.Admin.Controllers
{
    public class CriteriaController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly ICriteriaService _criteriaService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;

        public CriteriaController(
            IPermissionService permissionService,
            ICriteriaService criteriaService,
            ILocalizationService localizationService,
            IOrderService orderService
            )
        {
            _permissionService = permissionService;
            _criteriaService = criteriaService;
            _localizationService = localizationService;
            _orderService = orderService;
        }

        [HttpPost]
        public virtual ActionResult CriteriaBlockedParts(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            if (orderId == 0)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var query = _criteriaService
                .GetAllCriteriaValues(orderId)
                .Where(c => c.CriteriaType == CriteriaType.BlockedParts)
                .OrderBy(x => x.Description)
                .AsQueryable();

            var resources = query
                .Select(x => new CriteriaResourceModel
                {
                    OrderId = orderId,
                    Id = x.Id,
                    Description = x.Description,
                });

            var gridModel = new DataSourceResult
            {
                Data = resources.AsEnumerable().PagedForCommand(command),
                Total = resources.Count()
            };

            return Json(gridModel);
        }

        [ValidateInput(false)]
        public virtual ActionResult CriteriaBlockedPartsAdd(int orderId, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var criteria = new Criteria
            {
                Description = message,
                OrderId = orderId,
                CriteriaType = CriteriaType.BlockedParts
            };

            _criteriaService.InsertCriteria(criteria);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult CriteriaBlockedPartsDelete(int id, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var criteria = _criteriaService.GetCriteriaById(id);
            if (criteria == null)
                throw new ArgumentException("No criteria found with the specified id");

            _criteriaService.DeleteCriteria(criteria);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual ActionResult CriteriaReworkParts(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedKendoGridJson();

            if (orderId == 0)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var query = _criteriaService
                .GetAllCriteriaValues(orderId)
                .Where(c => c.CriteriaType == CriteriaType.ReworkParts)
                .OrderBy(x => x.Description)
                .AsQueryable();

            var resources = query
                .Select(x => new CriteriaResourceModel
                {
                    OrderId = orderId,
                    Id = x.Id,
                    Description = x.Description,
                });

            var gridModel = new DataSourceResult
            {
                Data = resources.AsEnumerable().PagedForCommand(command),
                Total = resources.Count()
            };

            return Json(gridModel);
        }

        [ValidateInput(false)]
        public virtual ActionResult CriteriaReworkPartsAdd(int orderId, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var criteria = new Criteria
            {
                Description = message,
                OrderId = orderId,
                CriteriaType = CriteriaType.ReworkParts
            };

            _criteriaService.InsertCriteria(criteria);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult CriteriaReworkPartsDelete(int id, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var criteria = _criteriaService.GetCriteriaById(id);
            if (criteria == null)
                throw new ArgumentException("No criteria found with the specified id");

            _criteriaService.DeleteCriteria(criteria);

            return new NullJsonResult();
        }

    }
}