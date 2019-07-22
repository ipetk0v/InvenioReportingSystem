using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Invenio.Core;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Users;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Infrastructure;
using Invenio.Services.Common;
using Invenio.Services.Directory;
using Invenio.Services.Localization;
using Invenio.Web.Models.Common;

namespace Invenio.Web.Factories
{
    /// <summary>
    /// Represents the address model factory
    /// </summary>
    public partial class AddressModelFactory : IAddressModelFactory
    {
        #region Fields

        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;

        #endregion

        #region Constructors

        public AddressModelFactory(IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IAddressAttributeFormatter addressAttributeFormatter)
        {
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeParser = addressAttributeParser;
            this._localizationService = localizationService;
            this._stateProvinceService = stateProvinceService;
            this._addressAttributeFormatter = addressAttributeFormatter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare address attributes
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address entity</param>
        /// <param name="overrideAttributesXml">Overridden address attributes in XML format; pass null to use CustomAttributes of address entity</param>
        protected virtual void PrepareCustomAddressAttributes(AddressModel model,
            Address address, string overrideAttributesXml = "")
        {
            var attributes = _addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _addressAttributeService.GetAddressAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                var selectedAddressAttributes = !String.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    (address != null ? address.CustomAttributes : null);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = _addressAttributeParser.ParseAddressAttributeValues(selectedAddressAttributes);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                var enteredText = _addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                model.CustomAddressAttributes.Add(attributeModel);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address entity</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="addressSettings">Address settings</param>
        /// <param name="loadCountries">Countries loading function; pass null if countries do not need to load</param>
        /// <param name="prePopulateWithUserFields">Whether to populate model properties with the User fields (used with the User entity)</param>
        /// <param name="User">User entity; required if prePopulateWithUserFields is true</param>
        /// <param name="overrideAttributesXml">Overridden address attributes in XML format; pass null to use CustomAttributes of the address entity</param>
        public virtual void PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            AddressSettings addressSettings,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithUserFields = false,
            User User = null,
            string overrideAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (addressSettings == null)
                throw new ArgumentNullException("addressSettings");

            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.CountryName = address.Country != null
                    ? address.Country.GetLocalized(x => x.Name)
                    : null;
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = address.StateProvince != null
                    ? address.StateProvince.GetLocalized(x => x.Name)
                    : null;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            if (address == null && prePopulateWithUserFields)
            {
                if (User == null)
                    throw new Exception("User cannot be null when prepopulating an address");
                model.Email = User.Email;
                model.FirstName = User.GetAttribute<string>(SystemUserAttributeNames.FirstName);
                model.LastName = User.GetAttribute<string>(SystemUserAttributeNames.LastName);
                model.Company = User.GetAttribute<string>(SystemUserAttributeNames.Company);
                model.Address1 = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress);
                model.Address2 = User.GetAttribute<string>(SystemUserAttributeNames.StreetAddress2);
                model.ZipPostalCode = User.GetAttribute<string>(SystemUserAttributeNames.ZipPostalCode);
                model.City = User.GetAttribute<string>(SystemUserAttributeNames.City);
                //ignore country and state for prepopulation. it can cause some issues when posting pack with errors, etc
                //model.CountryId = User.GetAttribute<int>(SystemUserAttributeNames.CountryId);
                //model.StateProvinceId = User.GetAttribute<int>(SystemUserAttributeNames.StateProvinceId);
                model.PhoneNumber = User.GetAttribute<string>(SystemUserAttributeNames.Phone);
                model.FaxNumber = User.GetAttribute<string>(SystemUserAttributeNames.Fax);
            }

            //countries and states
            if (addressSettings.CountryEnabled && loadCountries != null)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in loadCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    var languageId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
                    var states = _stateProvinceService
                        .GetStateProvincesByCountryId(model.CountryId.HasValue ? model.CountryId.Value : 0, languageId)
                        .ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = s.GetLocalized(x => x.Name),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
                        }
                    }
                    else
                    {
                        bool anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = _localizationService.GetResource(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                            Value = "0"
                        });
                    }
                }
            }

            //form fields
            model.CompanyEnabled = addressSettings.CompanyEnabled;
            model.CompanyRequired = addressSettings.CompanyRequired;
            model.StreetAddressEnabled = addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = addressSettings.ZipPostalCodeRequired;
            model.CityEnabled = addressSettings.CityEnabled;
            model.CityRequired = addressSettings.CityRequired;
            model.CountryEnabled = addressSettings.CountryEnabled;
            model.StateProvinceEnabled = addressSettings.StateProvinceEnabled;
            model.PhoneEnabled = addressSettings.PhoneEnabled;
            model.PhoneRequired = addressSettings.PhoneRequired;
            model.FaxEnabled = addressSettings.FaxEnabled;
            model.FaxRequired = addressSettings.FaxRequired;

            //User attribute services
            if (_addressAttributeService != null && _addressAttributeParser != null)
            {
                PrepareCustomAddressAttributes(model, address, overrideAttributesXml);
            }
            if (_addressAttributeFormatter != null && address != null)
            {
                model.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(address.CustomAttributes);
            }
        }

        #endregion
    }
}
