using FluentValidation;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;
using Invenio.Web.Models.User;

namespace Invenio.Web.Validators.User
{
    public partial class PasswordRecoveryValidator : BaseNopValidator<PasswordRecoveryModel>
    {
        public PasswordRecoveryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.PasswordRecovery.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }}
}