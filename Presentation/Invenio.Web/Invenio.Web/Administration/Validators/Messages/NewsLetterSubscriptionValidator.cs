using FluentValidation;
using Invenio.Admin.Models.Messages;
using Invenio.Core.Domain.Messages;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Messages
{
    public partial class NewsLetterSubscriptionValidator : BaseNopValidator<NewsLetterSubscriptionModel>
    {
        public NewsLetterSubscriptionValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));

            //SetDatabaseValidationRules<NewsLetterSubscription>(dbContext);
        }
    }
}