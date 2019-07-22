using System.Web.Mvc;
using FluentValidation.Attributes;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;
using Invenio.Web.Validators.User;

namespace Invenio.Web.Models.User
{
    [Validator(typeof(PasswordRecoveryValidator))]
    public partial class PasswordRecoveryModel : BaseNopModel
    {
        [AllowHtml]
        [NopResourceDisplayName("Account.PasswordRecovery.Email")]
        public string Email { get; set; }

        public string Result { get; set; }
    }
}