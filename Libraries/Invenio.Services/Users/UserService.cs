using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Invenio.Core;
using Invenio.Core.Caching;
using Invenio.Core.Data;
//using Invenio.Core.Domain.Blogs;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Forums;
//using Invenio.Core.Domain.News;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Polls;
//using Invenio.Core.Domain.Shipping;
using Invenio.Data;
using Invenio.Services.Common;
using Invenio.Services.Events;

namespace Invenio.Services.Users
{
    /// <summary>
    /// User service
    /// </summary>
    public partial class UserService : IUserService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        private const string UserROLES_ALL_KEY = "Nop.Userrole.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        private const string UserROLES_BY_SYSTEMNAME_KEY = "Nop.Userrole.systemname-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string UserROLES_PATTERN_KEY = "Nop.Userrole.";

        #endregion

        #region Fields

        private readonly IRepository<User> _UserRepository;
        private readonly IRepository<UserPassword> _UserPasswordRepository;
        private readonly IRepository<UserRole> _UserRoleRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        //private readonly IRepository<Order> _orderRepository;
        //private readonly IRepository<ForumPost> _forumPostRepository;
        //private readonly IRepository<ForumTopic> _forumTopicRepository;
        //private readonly IRepository<BlogComment> _blogCommentRepository;
        //private readonly IRepository<NewsComment> _newsCommentRepository;
        //private readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;
        //private readonly IRepository<ProductReview> _productReviewRepository;
        //private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly UserSettings _UserSettings;
        private readonly CommonSettings _commonSettings;

        #endregion

        #region Ctor

        public UserService(ICacheManager cacheManager,
            IRepository<User> UserRepository,
            IRepository<UserPassword> UserPasswordRepository,
            IRepository<UserRole> UserRoleRepository,
            IRepository<GenericAttribute> gaRepository,
            //IRepository<Order> orderRepository,
            //IRepository<ForumPost> forumPostRepository,
            //IRepository<ForumTopic> forumTopicRepository,
            //IRepository<BlogComment> blogCommentRepository,
            //IRepository<NewsComment> newsCommentRepository,
            //IRepository<PollVotingRecord> pollVotingRecordRepository,
            //IRepository<ProductReview> productReviewRepository,
            //IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IGenericAttributeService genericAttributeService,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher, 
            UserSettings UserSettings,
            CommonSettings commonSettings)
        {
            this._cacheManager = cacheManager;
            this._UserRepository = UserRepository;
            this._UserPasswordRepository = UserPasswordRepository;
            this._UserRoleRepository = UserRoleRepository;
            this._gaRepository = gaRepository;
            //this._orderRepository = orderRepository;
            //this._forumPostRepository = forumPostRepository;
            //this._forumTopicRepository = forumTopicRepository;
            //this._blogCommentRepository = blogCommentRepository;
            //this._newsCommentRepository = newsCommentRepository;
            //this._pollVotingRecordRepository = pollVotingRecordRepository;
            //this._productReviewRepository = productReviewRepository;
            //this._productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            this._genericAttributeService = genericAttributeService;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._eventPublisher = eventPublisher;
            this._UserSettings = UserSettings;
            this._commonSettings = commonSettings;
        }

        #endregion

        #region Methods

        #region Users

        /// <summary>
        /// Gets all Users
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="UserRoleIds">A list of User role identifiers to filter by (at least one match); pass null or empty list in order to load all Users; </param>
        /// <param name="email">Email; null to load all Users</param>
        /// <param name="username">Username; null to load all Users</param>
        /// <param name="firstName">First name; null to load all Users</param>
        /// <param name="lastName">Last name; null to load all Users</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all Users</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all Users</param>
        /// <param name="company">Company; null to load all Users</param>
        /// <param name="phone">Phone; null to load all Users</param>
        /// <param name="zipPostalCode">Phone; null to load all Users</param>
        /// <param name="ipAddress">IP address; null to load all Users</param>
        /// <param name="loadOnlyWithShoppingCart">Value indicating whether to load Users only with shopping cart</param>
        /// <param name="sct">Value indicating what shopping cart type to filter; userd when 'loadOnlyWithShoppingCart' param is 'true'</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Users</returns>
        public virtual IPagedList<User> GetAllUsers(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int affiliateId = 0, int vendorId = 0,
            int[] UserRoleIds = null, string email = null, string username = null,
            string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null,
            string ipAddress = null, bool loadOnlyWithShoppingCart = false, /*ShoppingCartType? sct = null,*/
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _UserRepository.Table;
            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
            if (affiliateId > 0)
                query = query.Where(c => affiliateId == c.AffiliateId);
            if (vendorId > 0)
                query = query.Where(c => vendorId == c.VendorId);
            query = query.Where(c => !c.Deleted);
            if (UserRoleIds != null && UserRoleIds.Length > 0)
                query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(UserRoleIds).Any());
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));
            if (!String.IsNullOrWhiteSpace(firstName))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.FirstName &&
                        z.Attribute.Value.Contains(firstName)))
                    .Select(z => z.User);
            }
            if (!String.IsNullOrWhiteSpace(lastName))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.LastName &&
                        z.Attribute.Value.Contains(lastName)))
                    .Select(z => z.User);
            }
            //date of birth is stored as a string into database.
            //we also know that date of birth is stored in the following format YYYY-MM-DD (for example, 1983-02-18).
            //so let's search it as a string
            if (dayOfBirth > 0 && monthOfBirth > 0)
            {
                //both are specified
                string dateOfBirthStr = monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-" + dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                //EndsWith is not supported by SQL Server Compact
                //so let's use the following workaround http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00
                
                //we also cannot use Length function in SQL Server Compact (not supported in this context)
                //z.Attribute.Value.Length - dateOfBirthStr.Length = 5
                //dateOfBirthStr.Length = 5
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Substring(5, 5) == dateOfBirthStr))
                    .Select(z => z.User);
            }
            else if (dayOfBirth > 0)
            {
                //only day is specified
                string dateOfBirthStr = dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                //EndsWith is not supported by SQL Server Compact
                //so let's use the following workaround http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00
                
                //we also cannot use Length function in SQL Server Compact (not supported in this context)
                //z.Attribute.Value.Length - dateOfBirthStr.Length = 8
                //dateOfBirthStr.Length = 2
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Substring(8, 2) == dateOfBirthStr))
                    .Select(z => z.User);
            }
            else if (monthOfBirth > 0)
            {
                //only month is specified
                string dateOfBirthStr = "-" + monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-";
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Contains(dateOfBirthStr)))
                    .Select(z => z.User);
            }
            //search by company
            if (!String.IsNullOrWhiteSpace(company))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.Company &&
                        z.Attribute.Value.Contains(company)))
                    .Select(z => z.User);
            }
            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.Phone &&
                        z.Attribute.Value.Contains(phone)))
                    .Select(z => z.User);
            }
            //search by zip
            if (!String.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.ZipPostalCode &&
                        z.Attribute.Value.Contains(zipPostalCode)))
                    .Select(z => z.User);
            }

            //search by IpAddress
            if (!String.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                    query = query.Where(w => w.LastIpAddress == ipAddress);
            }

            //if (loadOnlyWithShoppingCart)
            //{
            //    int? sctId = null;
            //    if (sct.HasValue)
            //        sctId = (int)sct.Value;

            //    query = sct.HasValue ?
            //        query.Where(c => c.ShoppingCartItems.Any(x => x.ShoppingCartTypeId == sctId)) :
            //        query.Where(c => c.ShoppingCartItems.Any());
            //}
            
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var Users = new PagedList<User>(query, pageIndex, pageSize);
            return Users;
        }

        /// <summary>
        /// Gets online Users
        /// </summary>
        /// <param name="lastActivityFromUtc">User last activity date (from)</param>
        /// <param name="UserRoleIds">A list of User role identifiers to filter by (at least one match); pass null or empty list in order to load all Users; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Users</returns>
        public virtual IPagedList<User> GetOnlineUsers(DateTime lastActivityFromUtc,
            int[] UserRoleIds, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _UserRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);
            if (UserRoleIds != null && UserRoleIds.Length > 0)
                query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(UserRoleIds).Any());
            
            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var Users = new PagedList<User>(query, pageIndex, pageSize);
            return Users;
        }

        /// <summary>
        /// Delete a User
        /// </summary>
        /// <param name="User">User</param>
        public virtual void DeleteUser(User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            if (User.IsSystemAccount)
                throw new InvenioException(string.Format("System User account ({0}) could not be deleted", User.SystemName));

            User.Deleted = true;

            if (_UserSettings.SuffixDeletedUsers)
            {
                if (!String.IsNullOrEmpty(User.Email))
                    User.Email += "-DELETED";
                if (!String.IsNullOrEmpty(User.Username))
                    User.Username += "-DELETED";
            }

            UpdateUser(User);

            //event notification
            _eventPublisher.EntityDeleted(User);
        }

        /// <summary>
        /// Gets a User
        /// </summary>
        /// <param name="UserId">User identifier</param>
        /// <returns>A User</returns>
        public virtual User GetUserById(int UserId)
        {
            if (UserId == 0)
                return null;
            
            return _UserRepository.GetById(UserId);
        }

        /// <summary>
        /// Get Users by identifiers
        /// </summary>
        /// <param name="UserIds">User identifiers</param>
        /// <returns>Users</returns>
        public virtual IList<User> GetUsersByIds(int[] UserIds)
        {
            if (UserIds == null || UserIds.Length == 0)
                return new List<User>();

            var query = from c in _UserRepository.Table
                        where UserIds.Contains(c.Id) && !c.Deleted
                        select c;
            var Users = query.ToList();
            //sort by passed identifiers
            var sortedUsers = new List<User>();
            foreach (int id in UserIds)
            {
                var User = Users.Find(x => x.Id == id);
                if (User != null)
                    sortedUsers.Add(User);
            }
            return sortedUsers;
        }
        
        /// <summary>
        /// Gets a User by GUID
        /// </summary>
        /// <param name="UserGuid">User GUID</param>
        /// <returns>A User</returns>
        public virtual User GetUserByGuid(Guid UserGuid)
        {
            if (UserGuid == Guid.Empty)
                return null;

            var query = from c in _UserRepository.Table
                        where c.UserGuid == UserGuid
                        orderby c.Id
                        select c;
            var User = query.FirstOrDefault();
            return User;
        }

        /// <summary>
        /// Get User by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>User</returns>
        public virtual User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in _UserRepository.Table
                        orderby c.Id
                        where c.Email == email
                        select c;
            var User = query.FirstOrDefault();
            return User;
        }

        /// <summary>
        /// Get User by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>User</returns>
        public virtual User GetUserBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from c in _UserRepository.Table
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;
            var User = query.FirstOrDefault();
            return User;
        }

        /// <summary>
        /// Get User by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User</returns>
        public virtual User GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in _UserRepository.Table
                        orderby c.Id
                        where c.Username == username
                        select c;
            var User = query.FirstOrDefault();
            return User;
        }
        
        /// <summary>
        /// Insert a guest User
        /// </summary>
        /// <returns>User</returns>
        public virtual User InsertGuestUser()
        {
            var User = new User
            {
                UserGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            //add to 'Guests' role
            var guestRole = GetUserRoleBySystemName(SystemUserRoleNames.Guests);
            if (guestRole == null)
                throw new InvenioException("'Guests' role could not be loaded");
            User.UserRoles.Add(guestRole);

            _UserRepository.Insert(User);

            return User;
        }
        
        /// <summary>
        /// Insert a User
        /// </summary>
        /// <param name="User">User</param>
        public virtual void InsertUser(User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            _UserRepository.Insert(User);

            //event notification
            _eventPublisher.EntityInserted(User);
        }
        
        /// <summary>
        /// Updates the User
        /// </summary>
        /// <param name="User">User</param>
        public virtual void UpdateUser(User User)
        {
            if (User == null)
                throw new ArgumentNullException("User");

            _UserRepository.Update(User);

            //event notification
            _eventPublisher.EntityUpdated(User);
        }

        /// <summary>
        /// Reset data required for checkout
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
        /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        public virtual void ResetCheckoutData(User User, int storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true)
        {
            if (User == null)
                throw new ArgumentNullException();
            
            //clear entered coupon codes
            //if (clearCouponCodes)
            //{
            //    _genericAttributeService.SaveAttribute<ShippingOption>(User, SystemUserAttributeNames.DiscountCouponCode, null);
            //    _genericAttributeService.SaveAttribute<ShippingOption>(User, SystemUserAttributeNames.GiftCardCouponCodes, null);
            //}

            ////clear checkout attributes
            //if (clearCheckoutAttributes)
            //{
            //    _genericAttributeService.SaveAttribute<ShippingOption>(User, SystemUserAttributeNames.CheckoutAttributes, null, storeId);
            //}

            //clear reward points flag
            if (clearRewardPoints)
            {
                _genericAttributeService.SaveAttribute(User, SystemUserAttributeNames.UseRewardPointsDuringCheckout, false, storeId);
            }

            //clear selected shipping method
            //if (clearShippingMethod)
            //{
            //    _genericAttributeService.SaveAttribute<ShippingOption>(User, SystemUserAttributeNames.SelectedShippingOption, null, storeId);
            //    _genericAttributeService.SaveAttribute<ShippingOption>(User, SystemUserAttributeNames.OfferedShippingOptions, null, storeId);
            //    _genericAttributeService.SaveAttribute<ShippingOption>(User, SystemUserAttributeNames.SelectedPickupPoint, null, storeId);
            //}

            //clear selected payment method
            if (clearPaymentMethod)
            {
                _genericAttributeService.SaveAttribute<string>(User, SystemUserAttributeNames.SelectedPaymentMethod, null, storeId);
            }
            
            UpdateUser(User);
        }
        
        /// <summary>
        /// Delete guest User records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete Users only without shopping cart</param>
        /// <returns>Number of deleted Users</returns>
        public virtual int DeleteGuestUsers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart)
        {
            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //stored procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                #region Stored procedure

                //prepare parameters
                //var pOnlyWithoutShoppingCart = _dataProvider.GetParameter();
                //pOnlyWithoutShoppingCart.ParameterName = "OnlyWithoutShoppingCart";
                //pOnlyWithoutShoppingCart.Value = onlyWithoutShoppingCart;
                //pOnlyWithoutShoppingCart.DbType = DbType.Boolean;

                var pCreatedFromUtc = _dataProvider.GetParameter();
                pCreatedFromUtc.ParameterName = "CreatedFromUtc";
                pCreatedFromUtc.Value = createdFromUtc.HasValue ? (object)createdFromUtc.Value : DBNull.Value;
                pCreatedFromUtc.DbType = DbType.DateTime;

                var pCreatedToUtc = _dataProvider.GetParameter();
                pCreatedToUtc.ParameterName = "CreatedToUtc";
                pCreatedToUtc.Value = createdToUtc.HasValue ? (object)createdToUtc.Value : DBNull.Value;
                pCreatedToUtc.DbType = DbType.DateTime;

                var pTotalRecordsDeleted = _dataProvider.GetParameter();
                pTotalRecordsDeleted.ParameterName = "TotalRecordsDeleted";
                pTotalRecordsDeleted.Direction = ParameterDirection.Output;
                pTotalRecordsDeleted.DbType = DbType.Int32;

                //invoke stored procedure
                _dbContext.ExecuteSqlCommand(
                    "EXEC [DeleteGuests] @CreatedFromUtc, @CreatedToUtc, @TotalRecordsDeleted OUTPUT",
                    false,
                    null,
                    pCreatedFromUtc,
                    pCreatedToUtc,
                    pTotalRecordsDeleted);

                int totalRecordsDeleted = (pTotalRecordsDeleted.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecordsDeleted.Value) : 0;
                return totalRecordsDeleted;

                #endregion
            }
            else
            {
                //stored procedures aren't supported. Use LINQ

                #region No stored procedure

                var guestRole = GetUserRoleBySystemName(SystemUserRoleNames.Guests);
                if (guestRole == null)
                    throw new InvenioException("'Guests' role could not be loaded");

                var query = _UserRepository.Table;
                if (createdFromUtc.HasValue)
                    query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
                if (createdToUtc.HasValue)
                    query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
                query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Contains(guestRole.Id));
                //if (onlyWithoutShoppingCart)
                //    query = query.Where(c => !c.ShoppingCartItems.Any());
                ////no orders
                //query = from c in query
                //        join o in _orderRepository.Table on c.Id equals o.UserId into c_o
                //        from o in c_o.DefaultIfEmpty()
                //        where !c_o.Any()
                //        select c;
                ////no blog comments
                //query = from c in query
                //        join bc in _blogCommentRepository.Table on c.Id equals bc.UserId into c_bc
                //        from bc in c_bc.DefaultIfEmpty()
                //        where !c_bc.Any()
                //        select c;
                ////no news comments
                //query = from c in query
                //        join nc in _newsCommentRepository.Table on c.Id equals nc.UserId into c_nc
                //        from nc in c_nc.DefaultIfEmpty()
                //        where !c_nc.Any()
                //        select c;
                ////no product reviews
                //query = from c in query
                //        join pr in _productReviewRepository.Table on c.Id equals pr.UserId into c_pr
                //        from pr in c_pr.DefaultIfEmpty()
                //        where !c_pr.Any()
                //        select c;
                ////no product reviews helpfulness
                //query = from c in query
                //        join prh in _productReviewHelpfulnessRepository.Table on c.Id equals prh.UserId into c_prh
                //        from prh in c_prh.DefaultIfEmpty()
                //        where !c_prh.Any()
                //        select c;
                ////no poll voting
                //query = from c in query
                //        join pvr in _pollVotingRecordRepository.Table on c.Id equals pvr.UserId into c_pvr
                //        from pvr in c_pvr.DefaultIfEmpty()
                //        where !c_pvr.Any()
                //        select c;
                ////no forum posts 
                //query = from c in query
                //        join fp in _forumPostRepository.Table on c.Id equals fp.UserId into c_fp
                //        from fp in c_fp.DefaultIfEmpty()
                //        where !c_fp.Any()
                //        select c;
                ////no forum topics
                //query = from c in query
                //        join ft in _forumTopicRepository.Table on c.Id equals ft.UserId into c_ft
                //        from ft in c_ft.DefaultIfEmpty()
                //        where !c_ft.Any()
                //        select c;
                //don't delete system accounts
                query = query.Where(c => !c.IsSystemAccount);

                //only distinct Users (group by ID)
                query = from c in query
                        group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                query = query.OrderBy(c => c.Id);
                var Users = query.ToList();


                int totalRecordsDeleted = 0;
                foreach (var c in Users)
                {
                    try
                    {
                        //delete attributes
                        var attributes = _genericAttributeService.GetAttributesForEntity(c.Id, "User");
                        _genericAttributeService.DeleteAttributes(attributes);

                        //delete from database
                        _UserRepository.Delete(c);
                        totalRecordsDeleted++;
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                    }
                }
                return totalRecordsDeleted;

                #endregion
            }
        }

        #endregion
        
        #region User roles

        /// <summary>
        /// Delete a User role
        /// </summary>
        /// <param name="UserRole">User role</param>
        public virtual void DeleteUserRole(UserRole UserRole)
        {
            if (UserRole == null)
                throw new ArgumentNullException("UserRole");

            if (UserRole.IsSystemRole)
                throw new InvenioException("System role could not be deleted");

            _UserRoleRepository.Delete(UserRole);

            _cacheManager.RemoveByPattern(UserROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(UserRole);
        }

        /// <summary>
        /// Gets a User role
        /// </summary>
        /// <param name="UserRoleId">User role identifier</param>
        /// <returns>User role</returns>
        public virtual UserRole GetUserRoleById(int UserRoleId)
        {
            if (UserRoleId == 0)
                return null;

            return _UserRoleRepository.GetById(UserRoleId);
        }

        /// <summary>
        /// Gets a User role
        /// </summary>
        /// <param name="systemName">User role system name</param>
        /// <returns>User role</returns>
        public virtual UserRole GetUserRoleBySystemName(string systemName)
        {
            if (String.IsNullOrWhiteSpace(systemName))
                return null;

            string key = string.Format(UserROLES_BY_SYSTEMNAME_KEY, systemName);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _UserRoleRepository.Table
                            orderby cr.Id
                            where cr.SystemName == systemName
                            select cr;
                var UserRole = query.FirstOrDefault();
                return UserRole;
            });
        }

        /// <summary>
        /// Gets all User roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>User roles</returns>
        public virtual IList<UserRole> GetAllUserRoles(bool showHidden = false)
        {
            string key = string.Format(UserROLES_ALL_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _UserRoleRepository.Table
                            orderby cr.Name
                            where showHidden || cr.Active
                            select cr;
                var UserRoles = query.ToList();
                return UserRoles;
            });
        }
        
        /// <summary>
        /// Inserts a User role
        /// </summary>
        /// <param name="UserRole">User role</param>
        public virtual void InsertUserRole(UserRole UserRole)
        {
            if (UserRole == null)
                throw new ArgumentNullException("UserRole");

            _UserRoleRepository.Insert(UserRole);

            _cacheManager.RemoveByPattern(UserROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(UserRole);
        }

        /// <summary>
        /// Updates the User role
        /// </summary>
        /// <param name="UserRole">User role</param>
        public virtual void UpdateUserRole(UserRole UserRole)
        {
            if (UserRole == null)
                throw new ArgumentNullException("UserRole");

            _UserRoleRepository.Update(UserRole);

            _cacheManager.RemoveByPattern(UserROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(UserRole);
        }

        #endregion

        #region User passwords

        /// <summary>
        /// Gets User passwords
        /// </summary>
        /// <param name="UserId">User identifier; pass null to load all records</param>
        /// <param name="passwordFormat">Password format; pass null to load all records</param>
        /// <param name="passwordsToReturn">Number of returning passwords; pass null to load all records</param>
        /// <returns>List of User passwords</returns>
        public virtual IList<UserPassword> GetUserPasswords(int? UserId = null, 
            PasswordFormat? passwordFormat = null, int? passwordsToReturn = null)
        {
            var query = _UserPasswordRepository.Table;

            //filter by User
            if (UserId.HasValue)
                query = query.Where(password => password.UserId == UserId.Value);

            //filter by password format
            if (passwordFormat.HasValue)
                query = query.Where(password => password.PasswordFormatId == (int)(passwordFormat.Value));

            //get the latest passwords
            if (passwordsToReturn.HasValue)
                query = query.OrderByDescending(password => password.CreatedOnUtc).Take(passwordsToReturn.Value);

            return query.ToList();
        }

        /// <summary>
        /// Get current User password
        /// </summary>
        /// <param name="UserId">User identifier</param>
        /// <returns>User password</returns>
        public virtual UserPassword GetCurrentPassword(int UserId)
        {
            if (UserId == 0)
                return null;

            //return the latest password
            return GetUserPasswords(UserId, passwordsToReturn: 1).FirstOrDefault();
        }

        /// <summary>
        /// Insert a User password
        /// </summary>
        /// <param name="UserPassword">User password</param>
        public virtual void InsertUserPassword(UserPassword UserPassword)
        {
            if (UserPassword == null)
                throw new ArgumentNullException("UserPassword");

            _UserPasswordRepository.Insert(UserPassword);

            //event notification
            _eventPublisher.EntityInserted(UserPassword);
        }

        /// <summary>
        /// Update a User password
        /// </summary>
        /// <param name="UserPassword">User password</param>
        public virtual void UpdateUserPassword(UserPassword UserPassword)
        {
            if (UserPassword == null)
                throw new ArgumentNullException("UserPassword");

            _UserPasswordRepository.Update(UserPassword);

            //event notification
            _eventPublisher.EntityUpdated(UserPassword);
        }

        #endregion

        #endregion
    }
}