using FluentValidation;
using Invenio.Admin.Models.Users;
using Invenio.Core.Domain.Users;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Users
{
    public partial class UserRoleValidator : BaseNopValidator<UserRoleModel>
    {
        public UserRoleValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Users.UserRoles.Fields.Name.Required"));

            SetDatabaseValidationRules<UserRole>(dbContext);
        }
    }
}