using FluentValidation;
using Invenio.Core.Domain.Users;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;
using Invenio.Web.Models.User;

namespace Invenio.Web.Validators.User
{
    public partial class LoginValidator : BaseNopValidator<LoginModel>
    {
        public LoginValidator(ILocalizationService localizationService, UserSettings UserSettings)
        {
            if (!UserSettings.UsernamesEnabled)
            {
                //login by email
                //RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Email.Required"));
                //RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
                RuleFor(x => x.Username).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Username.Required"));
                RuleFor(x => x.Password).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Password.Required"));
            }
        }
    }
}