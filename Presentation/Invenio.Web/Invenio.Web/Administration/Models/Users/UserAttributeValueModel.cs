using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Invenio.Admin.Validators.Users;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Localization;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    [Validator(typeof(UserAttributeValueValidator))]
    public partial class UserAttributeValueModel : BaseNopEntityModel, ILocalizedModel<UserAttributeValueLocalizedModel>
    {
        public UserAttributeValueModel()
        {
            Locales = new List<UserAttributeValueLocalizedModel>();
        }

        public int UserAttributeId { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Values.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<UserAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class UserAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Values.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}