using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Invenio.Admin.Validators.Users;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Localization;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Users
{
    [Validator(typeof(UserAttributeValidator))]
    public partial class UserAttributeModel : BaseNopEntityModel, ILocalizedModel<UserAttributeLocalizedModel>
    {
        public UserAttributeModel()
        {
            Locales = new List<UserAttributeLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }
        [NopResourceDisplayName("Admin.Users.UserAttributes.Fields.AttributeControlType")]
        [AllowHtml]
        public string AttributeControlTypeName { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        public IList<UserAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class UserAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Users.UserAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

    }
}