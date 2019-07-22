using FluentValidation;
using Invenio.Admin.Models.Customers;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Customers
{
    public class CustomerValidator : BaseNopValidator<CustomerModel>
    {
        public CustomerValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Customers.Fields.Name.Required"));
            RuleFor(x => x.ManufacturerId).NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Customer.Fields.Manufacturer.Required"));

            SetDatabaseValidationRules<Invenio.Core.Domain.Manufacturers.Manufacturer>(dbContext);
        }
    }
}