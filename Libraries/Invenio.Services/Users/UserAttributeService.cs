using System;
using System.Collections.Generic;
using System.Linq;
using Invenio.Core.Caching;
using Invenio.Core.Data;
using Invenio.Core.Domain.Users;
using Invenio.Services.Events;

namespace Invenio.Services.Users
{
    /// <summary>
    /// User attribute service
    /// </summary>
    public partial class UserAttributeService : IUserAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string UserATTRIBUTES_ALL_KEY = "Nop.Userattribute.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : User attribute ID
        /// </remarks>
        private const string UserATTRIBUTES_BY_ID_KEY = "Nop.Userattribute.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : User attribute ID
        /// </remarks>
        private const string UserATTRIBUTEVALUES_ALL_KEY = "Nop.Userattributevalue.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : User attribute value ID
        /// </remarks>
        private const string UserATTRIBUTEVALUES_BY_ID_KEY = "Nop.Userattributevalue.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string UserATTRIBUTES_PATTERN_KEY = "Nop.Userattribute.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string UserATTRIBUTEVALUES_PATTERN_KEY = "Nop.Userattributevalue.";
        #endregion
        
        #region Fields

        private readonly IRepository<UserAttribute> _UserAttributeRepository;
        private readonly IRepository<UserAttributeValue> _UserAttributeValueRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="UserAttributeRepository">User attribute repository</param>
        /// <param name="UserAttributeValueRepository">User attribute value repository</param>
        /// <param name="eventPublisher">Event published</param>
        public UserAttributeService(ICacheManager cacheManager,
            IRepository<UserAttribute> UserAttributeRepository,
            IRepository<UserAttributeValue> UserAttributeValueRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._UserAttributeRepository = UserAttributeRepository;
            this._UserAttributeValueRepository = UserAttributeValueRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a User attribute
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        public virtual void DeleteUserAttribute(UserAttribute UserAttribute)
        {
            if (UserAttribute == null)
                throw new ArgumentNullException("UserAttribute");

            _UserAttributeRepository.Delete(UserAttribute);

            _cacheManager.RemoveByPattern(UserATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(UserATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(UserAttribute);
        }

        /// <summary>
        /// Gets all User attributes
        /// </summary>
        /// <returns>User attributes</returns>
        public virtual IList<UserAttribute> GetAllUserAttributes()
        {
            string key = UserATTRIBUTES_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from ca in _UserAttributeRepository.Table
                            orderby ca.DisplayOrder, ca.Id
                            select ca;
                return query.ToList();
            });
        }

        /// <summary>
        /// Gets a User attribute 
        /// </summary>
        /// <param name="UserAttributeId">User attribute identifier</param>
        /// <returns>User attribute</returns>
        public virtual UserAttribute GetUserAttributeById(int UserAttributeId)
        {
            if (UserAttributeId == 0)
                return null;

            string key = string.Format(UserATTRIBUTES_BY_ID_KEY, UserAttributeId);
            return _cacheManager.Get(key, () => _UserAttributeRepository.GetById(UserAttributeId));
        }

        /// <summary>
        /// Inserts a User attribute
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        public virtual void InsertUserAttribute(UserAttribute UserAttribute)
        {
            if (UserAttribute == null)
                throw new ArgumentNullException("UserAttribute");

            _UserAttributeRepository.Insert(UserAttribute);

            _cacheManager.RemoveByPattern(UserATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(UserATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(UserAttribute);
        }

        /// <summary>
        /// Updates the User attribute
        /// </summary>
        /// <param name="UserAttribute">User attribute</param>
        public virtual void UpdateUserAttribute(UserAttribute UserAttribute)
        {
            if (UserAttribute == null)
                throw new ArgumentNullException("UserAttribute");

            _UserAttributeRepository.Update(UserAttribute);

            _cacheManager.RemoveByPattern(UserATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(UserATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(UserAttribute);
        }

        /// <summary>
        /// Deletes a User attribute value
        /// </summary>
        /// <param name="UserAttributeValue">User attribute value</param>
        public virtual void DeleteUserAttributeValue(UserAttributeValue UserAttributeValue)
        {
            if (UserAttributeValue == null)
                throw new ArgumentNullException("UserAttributeValue");

            _UserAttributeValueRepository.Delete(UserAttributeValue);

            _cacheManager.RemoveByPattern(UserATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(UserATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(UserAttributeValue);
        }

        /// <summary>
        /// Gets User attribute values by User attribute identifier
        /// </summary>
        /// <param name="UserAttributeId">The User attribute identifier</param>
        /// <returns>User attribute values</returns>
        public virtual IList<UserAttributeValue> GetUserAttributeValues(int UserAttributeId)
        {
            string key = string.Format(UserATTRIBUTEVALUES_ALL_KEY, UserAttributeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from cav in _UserAttributeValueRepository.Table
                            orderby cav.DisplayOrder, cav.Id
                            where cav.UserAttributeId == UserAttributeId
                            select cav;
                var UserAttributeValues = query.ToList();
                return UserAttributeValues;
            });
        }
        
        /// <summary>
        /// Gets a User attribute value
        /// </summary>
        /// <param name="UserAttributeValueId">User attribute value identifier</param>
        /// <returns>User attribute value</returns>
        public virtual UserAttributeValue GetUserAttributeValueById(int UserAttributeValueId)
        {
            if (UserAttributeValueId == 0)
                return null;

            string key = string.Format(UserATTRIBUTEVALUES_BY_ID_KEY, UserAttributeValueId);
            return _cacheManager.Get(key, () => _UserAttributeValueRepository.GetById(UserAttributeValueId));
        }

        /// <summary>
        /// Inserts a User attribute value
        /// </summary>
        /// <param name="UserAttributeValue">User attribute value</param>
        public virtual void InsertUserAttributeValue(UserAttributeValue UserAttributeValue)
        {
            if (UserAttributeValue == null)
                throw new ArgumentNullException("UserAttributeValue");

            _UserAttributeValueRepository.Insert(UserAttributeValue);

            _cacheManager.RemoveByPattern(UserATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(UserATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(UserAttributeValue);
        }

        /// <summary>
        /// Updates the User attribute value
        /// </summary>
        /// <param name="UserAttributeValue">User attribute value</param>
        public virtual void UpdateUserAttributeValue(UserAttributeValue UserAttributeValue)
        {
            if (UserAttributeValue == null)
                throw new ArgumentNullException("UserAttributeValue");

            _UserAttributeValueRepository.Update(UserAttributeValue);

            _cacheManager.RemoveByPattern(UserATTRIBUTES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(UserATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(UserAttributeValue);
        }
        
        #endregion
    }
}
