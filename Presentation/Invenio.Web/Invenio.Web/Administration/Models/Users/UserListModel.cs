using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    public partial class UserListModel : BaseNopModel
    {
        public UserListModel()
        {
            SearchUserRoleIds = new List<int>();
            AvailableUserRoles = new List<SelectListItem>();
        }

        [UIHint("MultiSelect")]
        [NopResourceDisplayName("Admin.Users.Users.List.UserRoles")]
        public IList<int> SearchUserRoleIds { get; set; }
        public IList<SelectListItem> AvailableUserRoles { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.List.SearchEmail")]
        [AllowHtml]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.List.SearchUsername")]
        [AllowHtml]
        public string SearchUsername { get; set; }
        public bool UsernamesEnabled { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.List.SearchFirstName")]
        [AllowHtml]
        public string SearchFirstName { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.List.SearchLastName")]
        [AllowHtml]
        public string SearchLastName { get; set; }


        [NopResourceDisplayName("Admin.Users.Users.List.SearchDateOfBirth")]
        [AllowHtml]
        public string SearchDayOfBirth { get; set; }
        [NopResourceDisplayName("Admin.Users.Users.List.SearchDateOfBirth")]
        [AllowHtml]
        public string SearchMonthOfBirth { get; set; }
        public bool DateOfBirthEnabled { get; set; }



        [NopResourceDisplayName("Admin.Users.Users.List.SearchCompany")]
        [AllowHtml]
        public string SearchCompany { get; set; }
        public bool CompanyEnabled { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.List.SearchPhone")]
        [AllowHtml]
        public string SearchPhone { get; set; }
        public bool PhoneEnabled { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.List.SearchZipCode")]
        [AllowHtml]
        public string SearchZipPostalCode { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }

        [NopResourceDisplayName("Admin.Users.Users.List.SearchIpAddress")]
        public string SearchIpAddress { get; set; }
    }
}