using FluentValidation;
using Invenio.Admin.Models.Supplier;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Supplier
{
    public class SupplierValidator : BaseNopValidator<SupplierModel>
    {
        public SupplierValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Suppliers.Fields.Name.Required"));
            RuleFor(x => x.CustomerId).NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Supplier.Fields.Customer.Required"));

            SetDatabaseValidationRules<Invenio.Core.Domain.Customers.Customer>(dbContext);
        }
    }
}