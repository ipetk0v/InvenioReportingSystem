using FluentValidation;
using Invenio.Admin.Models.Customer;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Customer
{
    public partial class CustomerValidator : BaseNopValidator<CustomerModel>
    {
        public CustomerValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Customers.Fields.Name.Required"));
            RuleFor(x => x.CountryId).NotEqual(0)
                .WithMessage(localizationService.GetResource("Admin.Catalog.Customers.Fields.Country.Required"));
            RuleFor(x => x.StateProvinceId).NotEqual(0)
                .WithMessage(
                    localizationService.GetResource("Admin.Catalog.Customers.Fields.StateProvince.Required"));
            SetDatabaseValidationRules<Invenio.Core.Domain.Customers.Customer>(dbContext);
        }
    }
}