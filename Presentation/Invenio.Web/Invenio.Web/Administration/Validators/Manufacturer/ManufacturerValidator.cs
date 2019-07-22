using FluentValidation;
using Invenio.Admin.Models.Manufacturer;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Manufacturer
{
    public partial class ManufacturerValidator : BaseNopValidator<ManufacturerModel>
    {
        public ManufacturerValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Name.Required"));
            RuleFor(x => x.CountryId).NotEqual(0)
                .WithMessage(localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Country.Required"));
            RuleFor(x => x.StateProvinceId).NotEqual(0)
                .WithMessage(
                    localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.StateProvince.Required"));
            SetDatabaseValidationRules<Invenio.Core.Domain.Manufacturers.Manufacturer>(dbContext);
        }
    }
}