using System;
using System.Linq;
using System.Net;
//using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;
using Invenio.Admin.Infrastructure.Cache;
using Invenio.Admin.Models.Home;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Orders;
using Invenio.Services.Catalog;
using Invenio.Services.Configuration;
using Invenio.Services.Users;
//using Invenio.Services.Orders;
using Invenio.Services.Security;
using Invenio.Services.Orders;
using Invenio.Services.Supplier;
using Invenio.Services.Reports;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using Invenio.Services.Customers;
using Invenio.Core.Domain.Reports;

namespace Invenio.Admin.Controllers
{
    public partial class HomeController : BaseAdminController
    {
        #region Fields
        private readonly IStoreContext _storeContext;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IUserService _UserService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly ISupplierService _supplierService;
        private readonly ICustomerService _CustomerService;
        private readonly IReportService _reportService;

        #endregion

        #region Ctor

        public HomeController(IStoreContext storeContext,
            AdminAreaSettings adminAreaSettings,
            ISettingService settingService,
            IPermissionService permissionService,
            IOrderService orderService,
            IUserService userService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            ISupplierService supplierService,
            ICustomerService customerService,
            IReportService reportService)
        {
            this._storeContext = storeContext;
            this._adminAreaSettings = adminAreaSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._orderService = orderService;
            this._UserService = userService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._supplierService = supplierService;
            this._CustomerService = customerService;
            this._reportService = reportService;
        }

        #endregion

        #region Methods

        public virtual ActionResult Index()
        {
            var model = new DashboardModel();
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult EconomicsNews()
        {
            try
            {
                string feedUrl = "https://www.economic.bg/bg/rss.xml"
                    .ToLowerInvariant();

                var rssData = _cacheManager.Get(ModelCacheEventConsumer.OFFICIAL_NEWS_MODEL_KEY, () =>
                {
                    //specify timeout (5 secs)
                    var request = WebRequest.Create(feedUrl);
                    request.Timeout = 5000;
                    using (var response = request.GetResponse())
                    using (var reader = XmlReader.Create(response.GetResponseStream()))
                    {
                        return SyndicationFeed.Load(reader);
                    }
                });

                var model = new SystemNewsModel()
                {
                    HideAdvertisements = _adminAreaSettings.HideAdvertisementsOnAdminArea
                };
                var currentItems = rssData.Items.Take(3).ToList();
                for (int i = 0; i < currentItems.Count(); i++)
                {
                    var item = currentItems.ElementAt(i);
                    var newsItem = new SystemNewsModel.NewsDetailsModel()
                    {
                        Title = item.Title.Text,
                        Summary = item.Summary.Text,
                        Url = item.Links.Any() ? item.Links.First().Uri.OriginalString : null,
                        PublishDate = item.PublishDate
                    };
                    model.Items.Add(newsItem);

                    //has new items?
                    if (i == 0)
                    {
                        var firstRequest = String.IsNullOrEmpty(_adminAreaSettings.LastNewsTitleAdminArea);
                        if (_adminAreaSettings.LastNewsTitleAdminArea != newsItem.Title)
                        {
                            _adminAreaSettings.LastNewsTitleAdminArea = newsItem.Title;
                            _settingService.SaveSetting(_adminAreaSettings);

                            if (!firstRequest)
                            {
                                //new item
                                model.HasNewItems = true;
                            }
                        }
                    }
                }
                return PartialView(model);
            }
            catch (Exception)
            {
                return Content("");
            }
        }

        [HttpPost]
        public virtual ActionResult NopCommerceNewsHideAdv()
        {
            _adminAreaSettings.HideAdvertisementsOnAdminArea = !_adminAreaSettings.HideAdvertisementsOnAdminArea;
            _settingService.SaveSetting(_adminAreaSettings);
            return Content("Setting changed");
        }

        [ChildActionOnly]
        public virtual ActionResult CommonStatistics()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageUsers) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var model = new CommonStatisticsModel();

            var allUsers = _UserService.GetAllUsers();

            var numberOfSuppliers = 0;
            var numberOfUsers = new List<User>();
            var numberNotApprovedReports = new List<Report>();

            if (_workContext.CurrentUser.IsAdmin())
            {
                numberOfUsers = _UserService.GetAllUsers(UserRoleIds: new[] { _UserService.GetUserRoleBySystemName(SystemUserRoleNames.Registered).Id }).ToList();
                numberOfSuppliers = _supplierService.GetAllSuppliers().ToList().Count;

                model.NumberOfUsers = numberOfUsers.Distinct().Count();
                model.NumberOfSuppliers = numberOfSuppliers;
                model.NumberNotApprovedReports = _reportService.GetAllReports(isAprroved: 2).TotalCount;
                model.NumberOfOrders = _orderService.GetAllOrders(published: true).Count;
            }
            else
            {
                foreach (var reg in _workContext.CurrentUser.CustomerRegions)
                {
                    numberOfUsers.AddRange(allUsers.Where(x => x.Active == true && x.CustomerRegions.Contains(reg)));

                    foreach (var man in _CustomerService
                        .GetAllCustomers(countryId: reg.CountryId, stateId: reg.Id))
                    {
                        numberOfUsers.AddRange(allUsers.Where(x => x.Active == true && x.Customers.Contains(man)));
                        numberNotApprovedReports.AddRange(_reportService.GetAllReports(CustomerId: man.Id, isAprroved: 2));

                        foreach (var cus in _supplierService.GetAllSuppliers(customerId: man.Id))
                        {
                            numberOfSuppliers++;
                            model.NumberOfOrders += _orderService.GetAllSupplierOrders(supplierId: cus.Id).TotalCount;
                        }
                    }
                }

                foreach (var man in _workContext.CurrentUser.Customers)
                {
                    numberOfUsers.AddRange(allUsers.Where(x => x.Active == true && x.Customers.Contains(man)));
                    numberNotApprovedReports.AddRange(_reportService.GetAllReports(CustomerId: man.Id, isAprroved: 2));

                    foreach (var cus in _supplierService.GetAllSuppliers(customerId: man.Id))
                    {
                        numberOfSuppliers++;
                        model.NumberOfOrders += _orderService.GetAllSupplierOrders(supplierId: cus.Id).TotalCount;
                    }
                }

                model.NumberOfUsers = numberOfUsers.Distinct().Count();
                model.NumberOfSuppliers = numberOfSuppliers;
                model.NumberNotApprovedReports = numberNotApprovedReports.Distinct().Count();
            }

            return PartialView(model);
        }

        #endregion
    }
}
