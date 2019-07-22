using System;
using System.Linq;
using Invenio.Core;
using Invenio.Core.Data;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Payments;
//using Invenio.Core.Domain.Shipping;
using Invenio.Services.Helpers;

namespace Invenio.Services.Users
{
    /// <summary>
    /// User report service
    /// </summary>
    public partial class UserReportService : IUserReportService
    {
        #region Fields

        private readonly IRepository<User> _UserRepository;
        //private readonly IRepository<Order> _orderRepository;
        private readonly IUserService _UserService;
        private readonly IDateTimeHelper _dateTimeHelper;
        
        #endregion

        #region Ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="UserRepository">User repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="UserService">User service</param>
        /// <param name="dateTimeHelper">Date time helper</param>
        public UserReportService(IRepository<User> UserRepository,
            /*IRepository<Order> orderRepository,*/ IUserService UserService,
            IDateTimeHelper dateTimeHelper)
        {
            this._UserRepository = UserRepository;
            //this._orderRepository = orderRepository;
            this._UserService = UserService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get best Users
        /// </summary>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shipment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Report</returns>
        //public virtual IPagedList<BestUserReportLine> GetBestUsersReport(DateTime? createdFromUtc,
        //    DateTime? createdToUtc, OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, int orderBy,
        //    int pageIndex = 0, int pageSize = 214748364)
        //{
        //    int? orderStatusId = null;
        //    if (os.HasValue)
        //        orderStatusId = (int)os.Value;

        //    int? paymentStatusId = null;
        //    if (ps.HasValue)
        //        paymentStatusId = (int)ps.Value;

        //    int? shippingStatusId = null;
        //    if (ss.HasValue)
        //        shippingStatusId = (int)ss.Value;
        //    var query1 = from c in _UserRepository.Table
        //                 join o in _orderRepository.Table on c.Id equals o.UserId
        //                 where (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
        //                 (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
        //                 (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
        //                 (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
        //                 (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
        //                 (!o.Deleted) &&
        //                 (!c.Deleted)
        //                 select new { c, o };

        //    var query2 = from co in query1
        //                 group co by co.c.Id into g
        //                 select new
        //                 {
        //                     UserId = g.Key,
        //                     OrderTotal = g.Sum(x => x.o.OrderTotal),
        //                     OrderCount = g.Count()
        //                 };
        //    switch (orderBy)
        //    {
        //        case 1:
        //            {
        //                query2 = query2.OrderByDescending(x => x.OrderTotal);
        //            }
        //            break;
        //        case 2:
        //            {
        //                query2 = query2.OrderByDescending(x => x.OrderCount);
        //            }
        //            break;
        //        default:
        //            throw new ArgumentException("Wrong orderBy parameter", "orderBy");
        //    }

        //    var tmp = new PagedList<dynamic>(query2, pageIndex, pageSize);
        //    return new PagedList<BestUserReportLine>(tmp.Select(x => new BestUserReportLine
        //        {
        //            UserId = x.UserId,
        //            OrderTotal = x.OrderTotal,
        //            OrderCount = x.OrderCount
        //        }),
        //        tmp.PageIndex, tmp.PageSize, tmp.TotalCount);
        //}

        /// <summary>
        /// Gets a report of Users registered in the last days
        /// </summary>
        /// <param name="days">Users registered in the last days</param>
        /// <returns>Number of registered Users</returns>
        public virtual int GetRegisteredUsersReport(int days)
        {
            DateTime date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-days);

            var registeredUserRole = _UserService.GetUserRoleBySystemName(SystemUserRoleNames.Registered);
            if (registeredUserRole == null)
                return 0;

            var query = from c in _UserRepository.Table
                        from cr in c.UserRoles
                        where !c.Deleted &&
                        cr.Id == registeredUserRole.Id &&
                        c.CreatedOnUtc >= date 
                        //&& c.CreatedOnUtc <= DateTime.UtcNow
                        select c;
            int count = query.Count();
            return count;
        }

        #endregion
    }
}