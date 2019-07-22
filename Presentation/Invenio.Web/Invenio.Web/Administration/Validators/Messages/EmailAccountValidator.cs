using FluentValidation;
using Invenio.Admin.Models.Messages;
using Invenio.Core.Domain.Messages;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Messages
{
    public partial class EmailAccountValidator : BaseNopValidator<EmailAccountModel>
    {
        public EmailAccountValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            
            RuleFor(x => x.DisplayName).NotEmpty();

            SetDatabaseValidationRules<EmailAccount>(dbContext);
        }
    }
}