using System.Collections.Generic;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Users;
//using Invenio.Core.Domain.Orders;
using Invenio.Web.Models.User;

namespace Invenio.Web.Factories
{
    /// <summary>
    /// Represents the interface of the User model factory
    /// </summary>
    public partial interface IUserModelFactory
    {
        /// <summary>
        /// Prepare the custom User attribute models
        /// </summary>
        /// <param name="User">User</param>
        /// <param name="overrideAttributesXml">Overridden User attributes in XML format; pass null to use CustomUserAttributes of User</param>
        /// <returns>List of the User attribute model</returns>
        IList<UserAttributeModel> PrepareCustomUserAttributes(User User, string overrideAttributesXml = "");

        /// <summary>
        /// Prepare the User info model
        /// </summary>
        /// <param name="model">User info model</param>
        /// <param name="User">User</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomUserAttributesXml">Overridden User attributes in XML format; pass null to use CustomUserAttributes of User</param>
        /// <returns>User info model</returns>
        UserInfoModel PrepareUserInfoModel(UserInfoModel model, User User, 
            bool excludeProperties, string overrideCustomUserAttributesXml = "");

        /// <summary>
        /// Prepare the User register model
        /// </summary>
        /// <param name="model">User register model</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="overrideCustomUserAttributesXml">Overridden User attributes in XML format; pass null to use CustomUserAttributes of User</param>
        /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
        /// <returns>User register model</returns>
        RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties, 
            string overrideCustomUserAttributesXml = "", bool setDefaultValues = false);

        /// <summary>
        /// Prepare the login model
        /// </summary>
        /// <param name="checkoutAsGuest">Whether to checkout as guest is enabled</param>
        /// <returns>Login model</returns>
        LoginModel PrepareLoginModel(bool? checkoutAsGuest);

        /// <summary>
        /// Prepare the password recovery model
        /// </summary>
        /// <returns>Password recovery model</returns>
        PasswordRecoveryModel PreparePasswordRecoveryModel();

        /// <summary>
        /// Prepare the password recovery confirm model
        /// </summary>
        /// <returns>Password recovery confirm model</returns>
        PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel();

        /// <summary>
        /// Prepare the register result model
        /// </summary>
        /// <param name="resultId">Value of UserRegistrationType enum</param>
        /// <returns>Register result model</returns>
        RegisterResultModel PrepareRegisterResultModel(int resultId);

        /// <summary>
        /// Prepare the User navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>User navigation model</returns>
        UserNavigationModel PrepareUserNavigationModel(int selectedTabId = 0);

        /// <summary>
        /// Prepare the User address list model
        /// </summary>
        /// <returns>User address list model</returns>  
        UserAddressListModel PrepareUserAddressListModel();

        /// <summary>
        /// Prepare the User downloadable products model
        /// </summary>
        /// <returns>User downloadable products model</returns>
        //UserDownloadableProductsModel PrepareUserDownloadableProductsModel();

        /// <summary>
        /// Prepare the user agreement model
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product</param>
        /// <returns>User agreement model</returns>
        //UserAgreementModel PrepareUserAgreementModel(OrderItem orderItem, Product product);

        /// <summary>
        /// Prepare the change password model
        /// </summary>
        /// <returns>Change password model</returns>
        ChangePasswordModel PrepareChangePasswordModel();

        /// <summary>
        /// Prepare the User avatar model
        /// </summary>
        /// <param name="model">User avatar model</param>
        /// <returns>User avatar model</returns>
        UserAvatarModel PrepareUserAvatarModel(UserAvatarModel model);
    }
}
