using System.Collections.Generic;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Security;

namespace Invenio.Services.Security
{
    // <summary>
    // Standard permission provider
    // </summary>
    public partial class StandardPermissionProvider : IPermissionProvider
    {
        //admin area permissions
        public static readonly PermissionRecord AccessAdminPanel = new PermissionRecord { Name = "Access admin area", SystemName = "AccessAdminPanel", Category = "Standard" };
        public static readonly PermissionRecord AllowUserImpersonation = new PermissionRecord { Name = "Admin area. Allow User Impersonation", SystemName = "AllowUserImpersonation", Category = "Users" };
        public static readonly PermissionRecord ManageCustomers = new PermissionRecord { Name = "Admin area. Manage Customers", SystemName = "ManageCustomers", Category = "Catalog" };
        public static readonly PermissionRecord ManageUsers = new PermissionRecord { Name = "Admin area. Manage Users", SystemName = "ManageUsers", Category = "Users" };
        public static readonly PermissionRecord ManageOrders = new PermissionRecord { Name = "Admin area. Manage Orders", SystemName = "ManageOrders", Category = "Orders" };
        public static readonly PermissionRecord ManageCountries = new PermissionRecord { Name = "Admin area. Manage Countries", SystemName = "ManageCountries", Category = "Configuration" };
        public static readonly PermissionRecord ManageLanguages = new PermissionRecord { Name = "Admin area. Manage Languages", SystemName = "ManageLanguages", Category = "Configuration" };
        public static readonly PermissionRecord ManageSettings = new PermissionRecord { Name = "Admin area. Manage Settings", SystemName = "ManageSettings", Category = "Configuration" };
        public static readonly PermissionRecord ManageActivityLog = new PermissionRecord { Name = "Admin area. Manage Activity Log", SystemName = "ManageActivityLog", Category = "Configuration" };
        public static readonly PermissionRecord ManageAcl = new PermissionRecord { Name = "Admin area. Manage ACL", SystemName = "ManageACL", Category = "Configuration" };
        public static readonly PermissionRecord ManageSystemLog = new PermissionRecord { Name = "Admin area. Manage System Log", SystemName = "ManageSystemLog", Category = "Configuration" };
        public static readonly PermissionRecord ManageMessageQueue = new PermissionRecord { Name = "Admin area. Manage Message Queue", SystemName = "ManageMessageQueue", Category = "Configuration" };
        public static readonly PermissionRecord ManageMaintenance = new PermissionRecord { Name = "Admin area. Manage Maintenance", SystemName = "ManageMaintenance", Category = "Configuration" };
        public static readonly PermissionRecord ManageScheduleTasks = new PermissionRecord { Name = "Admin area. Manage Schedule Tasks", SystemName = "ManageScheduleTasks", Category = "Configuration" };
        public static readonly PermissionRecord ManageSuppliers = new PermissionRecord { Name = "Admin area. Manage Suppliers", SystemName = "ManageSuppliers", Category = "Catalog"};
        public static readonly PermissionRecord ManageReports = new PermissionRecord { Name = "Admin area. Manage Reports", SystemName = "ManageReports", Category = "Reports"};
        public static readonly PermissionRecord Manage흀ficiency = new PermissionRecord {Name = "Admin area. Manage 흀ficiency", SystemName = "Manage흀ficiency", Category = "Reports"};

        public static readonly PermissionRecord PublicStoreAllowNavigation = new PermissionRecord { Name = "Public store. Allow navigation", SystemName = "PublicStoreAllowNavigation", Category = "PublicStore" };


        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel,
                AllowUserImpersonation,
                ManageCustomers,
                ManageUsers,
                ManageOrders,
                ManageCountries,
                ManageLanguages,
                ManageSettings,
                ManageActivityLog,
                ManageAcl,
                ManageSystemLog,
                ManageMessageQueue,
                ManageMaintenance,
                ManageScheduleTasks,
                PublicStoreAllowNavigation,
                ManageSuppliers,
                ManageReports,
                Manage흀ficiency
            };
        }

        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissionRecord
                {
                    UserRoleSystemName = SystemUserRoleNames.Administrators,
                    PermissionRecords = new[]
                    {
                        AccessAdminPanel,
                        AllowUserImpersonation,
                        ManageCustomers,
                        ManageUsers,
                        ManageOrders,
                        ManageCountries,
                        ManageLanguages,
                        ManageSettings,
                        ManageActivityLog,
                        ManageAcl,
                        ManageSystemLog,
                        ManageMessageQueue,
                        ManageMaintenance,
                        ManageScheduleTasks,
                        PublicStoreAllowNavigation,
                        ManageSuppliers,
                        ManageReports,
                        Manage흀ficiency
                    }
                },
                new DefaultPermissionRecord
                {
                    UserRoleSystemName = SystemUserRoleNames.Guests,
                    PermissionRecords = new[]
                    {
                        PublicStoreAllowNavigation
                    }
                },
                new DefaultPermissionRecord
                {
                    UserRoleSystemName = SystemUserRoleNames.Registered,
                    PermissionRecords = new[]
                    {
                        PublicStoreAllowNavigation
                    }
                }
            };
        }
    }
}