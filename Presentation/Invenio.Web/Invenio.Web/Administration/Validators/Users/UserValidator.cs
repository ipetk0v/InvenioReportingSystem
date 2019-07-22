using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Invenio.Admin.Models.Users;
using Invenio.Core.Domain.Users;
using Invenio.Data;
using Invenio.Services.Users;
using Invenio.Services.Directory;
using Invenio.Services.Localization;
using Invenio.Web.Framework.Validators;

namespace Invenio.Admin.Validators.Users
{
    public partial class UserValidator : BaseNopValidator<UserModel>
    {
        public UserValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IUserService UserService,
            UserSettings UserSettings,
            IDbContext dbContext)
        {
            //ensure that valid email address is entered if Registered role is checked to avoid registered Users with empty email address
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                //.WithMessage("Valid Email is required for User to be in 'Registered' role")
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"))
                //only for registered users
                .When(x => IsRegisteredUserRoleChecked(x, UserService));

            RuleFor(x => x.Username).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Username.Required"));
            //RuleFor(x => x.Password).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Password.Required"));

            //form fields
            if (UserSettings.CountryEnabled && UserSettings.CountryRequired)
            {
                RuleFor(x => x.CountryId)
                    .NotEqual(0)
                    .WithMessage(localizationService.GetResource("Account.Fields.Country.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.CountryEnabled &&
                UserSettings.StateProvinceEnabled &&
                UserSettings.StateProvinceRequired)
            {
                Custom(x =>
                {
                    //does selected country have states?
                    var hasStates = stateProvinceService.GetStateProvincesByCountryId(x.CountryId).Any();
                    if (hasStates)
                    {
                        //if yes, then ensure that a state is selected
                        if (x.StateProvinceId == 0)
                        {
                            return new ValidationFailure("StateProvinceId", localizationService.GetResource("Account.Fields.StateProvince.Required"));
                        }
                    }
                    return null;
                });
            }
            if (UserSettings.CompanyRequired && UserSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.Company.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.StreetAddressRequired && UserSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.StreetAddress)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.StreetAddress.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.StreetAddress2Required && UserSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.StreetAddress2)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.StreetAddress2.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.ZipPostalCodeRequired && UserSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.ZipPostalCode.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.CityRequired && UserSettings.CityEnabled)
            {
                RuleFor(x => x.City)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.City.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.PhoneRequired && UserSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.Phone.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }
            if (UserSettings.FaxRequired && UserSettings.FaxEnabled)
            {
                RuleFor(x => x.Fax)
                    .NotEmpty()
                    .WithMessage(localizationService.GetResource("Admin.Users.Users.Fields.Fax.Required"))
                    //only for registered users
                    .When(x => IsRegisteredUserRoleChecked(x, UserService));
            }

            SetDatabaseValidationRules<User>(dbContext);
        }

        private bool IsRegisteredUserRoleChecked(UserModel model, IUserService UserService)
        {
            var allUserRoles = UserService.GetAllUserRoles(true);
            var newUserRoles = new List<UserRole>();
            foreach (var UserRole in allUserRoles)
                if (model.SelectedUserRoleIds.Contains(UserRole.Id))
                    newUserRoles.Add(UserRole);

            bool isInRegisteredRole = newUserRoles.FirstOrDefault(cr => cr.SystemName == SystemUserRoleNames.Registered) != null;
            return isInRegisteredRole;
        }
    }
}