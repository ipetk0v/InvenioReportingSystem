using Invenio.Admin.Models.Orders;
using Invenio.Data;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;
using FluentValidation;


namespace Invenio.Admin.Validators.Orders
{
    public class OrderValidator : BaseNopValidator<OrderModel>
    {
        public OrderValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Order.Validator.Customer.IsRequired"));
            RuleFor(x => x.Number).NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Order.Validator.Number.IsRequired"));

            //RuleFor(x => x.PartName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Order.Validator.Number.PartName"));
            //RuleFor(x => x.PartSerNumer).NotEmpty().WithMessage(localizationService.GetResource("Admin.Order.Validator.Number.PartSerNumer"));

            SetDatabaseValidationRules<Invenio.Core.Domain.Orders.Order>(dbContext);
        }
    }
}