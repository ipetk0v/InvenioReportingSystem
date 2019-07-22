//Contributor:  Nicholas Mayne

using System;
using Invenio.Core;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Localization;
using Invenio.Services.Common;
using Invenio.Services.Users;
using Invenio.Services.Events;
using Invenio.Services.Localization;
using Invenio.Services.Logging;
using Invenio.Services.Messages;
//using Invenio.Services.Orders;

namespace Invenio.Services.Authentication.External
{
    /// <summary>
    /// External authorizer
    /// </summary>
    public partial class ExternalAuthorizer : IExternalAuthorizer
    {
        #region Fields

        private readonly IAuthenticationService _authenticationService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUserRegistrationService _UserRegistrationService;
        private readonly IUserActivityService _UserActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly UserSettings _UserSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        //private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEventPublisher _eventPublisher;
        private readonly LocalizationSettings _localizationSettings;
        #endregion

        #region Ctor

        public ExternalAuthorizer(IAuthenticationService authenticationService,
            IOpenAuthenticationService openAuthenticationService,
            IGenericAttributeService genericAttributeService,
            IUserRegistrationService UserRegistrationService, 
            IUserActivityService UserActivityService, 
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            UserSettings UserSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            //IShoppingCartService shoppingCartService,
            IWorkflowMessageService workflowMessageService,
            IEventPublisher eventPublisher,
            LocalizationSettings localizationSettings)
        {
            this._authenticationService = authenticationService;
            this._openAuthenticationService = openAuthenticationService;
            this._genericAttributeService = genericAttributeService;
            this._UserRegistrationService = UserRegistrationService;
            this._UserActivityService = UserActivityService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._UserSettings = UserSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            //this._shoppingCartService = shoppingCartService;
            this._workflowMessageService = workflowMessageService;
            this._eventPublisher = eventPublisher;
            this._localizationSettings = localizationSettings;
        }
        
        #endregion

        #region Utilities

        private bool RegistrationIsEnabled()
        {
            return _UserSettings.UserRegistrationType != UserRegistrationType.Disabled && !_externalAuthenticationSettings.AutoRegisterEnabled;
        }

        private bool AutoRegistrationIsEnabled()
        {
            return _UserSettings.UserRegistrationType != UserRegistrationType.Disabled && _externalAuthenticationSettings.AutoRegisterEnabled;
        }

        private bool AccountDoesNotExistAndUserIsNotLoggedOn(User userFound, User userLoggedIn)
        {
            return userFound == null && userLoggedIn == null;
        }

        private bool AccountIsAssignedToLoggedOnAccount(User userFound, User userLoggedIn)
        {
            return userFound.Id.Equals(userLoggedIn.Id);
        }

        private bool AccountAlreadyExists(User userFound, User userLoggedIn)
        {
            return userFound != null && userLoggedIn != null;
        }

        #endregion

        #region Methods

        public virtual AuthorizationResult Authorize(OpenAuthenticationParameters parameters)
        {
            var userFound = _openAuthenticationService.GetUser(parameters);

            var userLoggedIn = _workContext.CurrentUser.IsRegistered() ? _workContext.CurrentUser : null;

            if (AccountAlreadyExists(userFound, userLoggedIn))
            {
                if (AccountIsAssignedToLoggedOnAccount(userFound, userLoggedIn))
                {
                    // The person is trying to log in as himself.. bit weird
                    return new AuthorizationResult(OpenAuthenticationStatus.Authenticated);
                }

                var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                result.AddError("Account is already assigned");
                return result;
            }
            if (AccountDoesNotExistAndUserIsNotLoggedOn(userFound, userLoggedIn))
            {
                ExternalAuthorizerHelper.StoreParametersForRoundTrip(parameters);

                if (AutoRegistrationIsEnabled())
                {
                    #region Register user

                    var currentUser = _workContext.CurrentUser;
                    var details = new RegistrationDetails(parameters);
                    var randomPassword = CommonHelper.GenerateRandomDigitCode(20);


                    bool isApproved =
                        //standard registration
                        (_UserSettings.UserRegistrationType == UserRegistrationType.Standard) ||
                        //skip email validation?
                        (_UserSettings.UserRegistrationType == UserRegistrationType.EmailValidation &&
                         !_externalAuthenticationSettings.RequireEmailValidation);

                    var registrationRequest = new UserRegistrationRequest(currentUser, 
                        details.EmailAddress,
                        _UserSettings.UsernamesEnabled ? details.UserName : details.EmailAddress, 
                        randomPassword,
                        PasswordFormat.Clear,
                        _storeContext.CurrentStore.Id,
                        isApproved);
                    var registrationResult = _UserRegistrationService.RegisterUser(registrationRequest);
                    if (registrationResult.Success)
                    {
                        //store other parameters (form fields)
                        if (!String.IsNullOrEmpty(details.FirstName))
                            _genericAttributeService.SaveAttribute(currentUser, SystemUserAttributeNames.FirstName, details.FirstName);
                        if (!String.IsNullOrEmpty(details.LastName))
                            _genericAttributeService.SaveAttribute(currentUser, SystemUserAttributeNames.LastName, details.LastName);
                    

                        userFound = currentUser;
                        _openAuthenticationService.AssociateExternalAccountWithUser(currentUser, parameters);
                        ExternalAuthorizerHelper.RemoveParameters();

                        //code below is copied from UserController.Register method

                        //authenticate
                        if (isApproved)
                            _authenticationService.SignIn(userFound ?? userLoggedIn, false);

                        //notifications
                        if (_UserSettings.NotifyNewUserRegistration)
                            _workflowMessageService.SendUserRegisteredNotificationMessage(currentUser, _localizationSettings.DefaultAdminLanguageId);

                        //raise event       
                        _eventPublisher.Publish(new UserRegisteredEvent(currentUser));

                        if (isApproved)
                        {
                            //standard registration
                            //or
                            //skip email validation

                            //send User welcome message
                            _workflowMessageService.SendUserWelcomeMessage(currentUser, _workContext.WorkingLanguage.Id);

                            //result
                            return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredStandard);
                        }
                        else if (_UserSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
                        {
                            //email validation message
                            _genericAttributeService.SaveAttribute(currentUser, SystemUserAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                            _workflowMessageService.SendUserEmailValidationMessage(currentUser, _workContext.WorkingLanguage.Id);

                            //result
                            return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredEmailValidation);
                        }
                        else if (_UserSettings.UserRegistrationType == UserRegistrationType.AdminApproval)
                        {
                            //result
                            return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredAdminApproval);
                        }
                    }
                    else
                    {
                        ExternalAuthorizerHelper.RemoveParameters();

                        var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                        foreach (var error in registrationResult.Errors)
                            result.AddError(string.Format(error));
                        return result;
                    }

                    #endregion
                }
                else if (RegistrationIsEnabled())
                {
                    return new AuthorizationResult(OpenAuthenticationStatus.AssociateOnLogon);
                }
                else
                {
                    ExternalAuthorizerHelper.RemoveParameters();

                    var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                    result.AddError("Registration is disabled");
                    return result;
                }
            }
            if (userFound == null)
            {
                _openAuthenticationService.AssociateExternalAccountWithUser(userLoggedIn, parameters);
            }

            //migrate shopping cart
            //_shoppingCartService.MigrateShoppingCart(_workContext.CurrentUser, userFound ?? userLoggedIn, true);
            //authenticate
            _authenticationService.SignIn(userFound ?? userLoggedIn, false);
            //raise event       
            _eventPublisher.Publish(new UserLoggedinEvent(userFound ?? userLoggedIn));
            //activity log
            _UserActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), 
                userFound ?? userLoggedIn);
            
            return new AuthorizationResult(OpenAuthenticationStatus.Authenticated);
        }

        #endregion
    }
}