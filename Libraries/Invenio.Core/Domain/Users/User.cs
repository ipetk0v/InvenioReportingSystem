using System;
using System.Collections.Generic;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Manufacturers;

namespace Invenio.Core.Domain.Users
{
    /// <summary>
    /// Represents a User
    /// </summary>
    public partial class User : BaseEntity
    {
        private ICollection<ExternalAuthenticationRecord> _externalAuthenticationRecords;
        private ICollection<UserRole> _UserRoles;
        private ICollection<Manufacturer> _Manufacturers;
        private ICollection<StateProvince> _ManufacturerRegions;
        //private ICollection<ShoppingCartItem> _shoppingCartItems;
        //private ICollection<ReturnRequest> _returnRequests;
        private ICollection<Address> _addresses;

        /// <summary>
        /// Ctor
        /// </summary>
        public User()
        {
            this.UserGuid = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the User Guid
        /// </summary>
        public Guid UserGuid { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the email that should be re-validated. Used in scenarios when a User is already registered and wants to change an email address.
        /// </summary>
        public string EmailToRevalidate { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the User is tax exempt
        /// </summary>
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public int AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier with which this User is associated (maganer)
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this User has some products in the shopping cart
        /// <remarks>The same as if we run this.ShoppingCartItems.Count > 0
        /// We use this property for performance optimization:
        /// if this property is set to false, then we do not need to load "ShoppingCartItems" navigation property for each page load
        /// It's used only in a couple of places in the presenation layer
        /// </remarks>
        /// </summary>
        public bool HasShoppingCartItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the User is required to re-login
        /// </summary>
        public bool RequireReLogin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating number of failed login attempts (wrong password)
        /// </summary>
        public int FailedLoginAttempts { get; set; }
        /// <summary>
        /// Gets or sets the date and time until which a User cannot login (locked out)
        /// </summary>
        public DateTime? CannotLoginUntilDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the User is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the User has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the User account is system
        /// </summary>
        public bool IsSystemAccount { get; set; }

        /// <summary>
        /// Gets or sets the User system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }

        /// <summary>
        ///  Gets or sets the store identifier in which User registered
        /// </summary>
        public int RegisteredInStoreId { get; set; }

        #region Navigation properties

        /// <summary>
        /// Gets or sets User generated content
        /// </summary>
        public virtual ICollection<ExternalAuthenticationRecord> ExternalAuthenticationRecords
        {
            get { return _externalAuthenticationRecords ?? (_externalAuthenticationRecords = new List<ExternalAuthenticationRecord>()); }
            protected set { _externalAuthenticationRecords = value; }
        }

        /// <summary>
        /// Gets or sets the User roles
        /// </summary>
        public virtual ICollection<UserRole> UserRoles
        {
            get => _UserRoles ?? (_UserRoles = new List<UserRole>());
            protected set => _UserRoles = value;
        }

        public virtual ICollection<Manufacturer> Manufacturers
        {
            get => _Manufacturers ?? (_Manufacturers = new List<Manufacturer>());
            protected set => _Manufacturers = value;
        }

        public virtual ICollection<StateProvince> ManufacturerRegions
        {
            get => _ManufacturerRegions ?? (_ManufacturerRegions = new List<StateProvince>());
            protected set => _ManufacturerRegions = value;
        }

        /// <summary>
        /// Gets or sets shopping cart items
        /// </summary>
        //public virtual ICollection<ShoppingCartItem> ShoppingCartItems
        //{
        //    get { return _shoppingCartItems ?? (_shoppingCartItems = new List<ShoppingCartItem>()); }
        //    protected set { _shoppingCartItems = value; }            
        //}

        /// <summary>
        /// Gets or sets return request of this User
        ///// </summary>
        //public virtual ICollection<ReturnRequest> ReturnRequests
        //{
        //    get { return _returnRequests ?? (_returnRequests = new List<ReturnRequest>()); }
        //    protected set { _returnRequests = value; }            
        //}

        /// <summary>
        /// Default billing address
        /// </summary>
        public virtual Address BillingAddress { get; set; }

        /// <summary>
        /// Default shipping address
        /// </summary>
        public virtual Address ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets User addresses
        /// </summary>
        public virtual ICollection<Address> Addresses
        {
            get { return _addresses ?? (_addresses = new List<Address>()); }
            protected set { _addresses = value; }
        }

        #endregion
    }
}