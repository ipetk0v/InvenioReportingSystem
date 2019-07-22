using FluentValidation;
using Invenio.Admin.Models.Settings;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Settings
{
    public partial class SettingValidator : BaseNopValidator<SettingModel>
    {
        public SettingValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));
        }
    }
}