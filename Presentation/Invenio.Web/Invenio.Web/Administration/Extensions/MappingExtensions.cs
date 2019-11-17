using Invenio.Admin.Models.Customer;
using Invenio.Admin.Models.Common;
using Invenio.Admin.Models.Directory;
using Invenio.Admin.Models.Localization;
using Invenio.Admin.Models.Logging;
using Invenio.Admin.Models.Messages;
using Invenio.Admin.Models.Settings;
using Invenio.Admin.Models.Stores;
using Invenio.Admin.Models.Users;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Common;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Localization;
using Invenio.Core.Domain.Logging;
using Invenio.Core.Domain.Customers;
using Invenio.Core.Domain.Messages;
using Invenio.Core.Domain.Stores;
using Invenio.Core.Domain.Users;
using Invenio.Core.Infrastructure.Mapper;
using Invenio.Services.Common;
using Invenio.Web.Framework.Security.Captcha;
using System;
using System.Linq;
using Invenio.Admin.Models.Supplier;
using Invenio.Admin.Models.Orders;
using Invenio.Admin.Models.Report;
using Invenio.Core.Domain.Orders;
using Invenio.Core.Domain.Reports;
using Invenio.Core.Domain.Suppliers;

namespace Invenio.Admin.Extensions
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        #region Supplier

        public static SupplierModel ToModel(this Supplier entity)
        {
            return entity.MapTo<Supplier, SupplierModel>();
        }

        public static Supplier ToEntity(this SupplierModel model)
        {
            return model.MapTo<SupplierModel, Supplier>();
        }

        public static Supplier ToEntity(this SupplierModel model, Supplier destination)
        {
            return model.MapTo(destination);
        }


        #endregion

        #region Customer

        public static CustomerModel ToModel(this Customer entity)
        {
            return entity.MapTo<Customer, CustomerModel>();
        }

        public static Customer ToEntity(this CustomerModel model)
        {
            return model.MapTo<CustomerModel, Customer>();
        }

        public static Customer ToEntity(this CustomerModel model, Customer destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Order attributes

        public static OrderAttributeModel ToModel(this OrderAttribute entity)
        {
            return entity.MapTo<OrderAttribute, OrderAttributeModel>();
        }

        public static OrderAttribute ToEntity(this OrderAttributeModel model)
        {
            return model.MapTo<OrderAttributeModel, OrderAttribute>();
        }

        public static OrderAttribute ToEntity(this OrderAttributeModel model, OrderAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region User attributes

        //attributes
        public static UserAttributeModel ToModel(this UserAttribute entity)
        {
            return entity.MapTo<UserAttribute, UserAttributeModel>();
        }

        public static UserAttribute ToEntity(this UserAttributeModel model)
        {
            return model.MapTo<UserAttributeModel, UserAttribute>();
        }

        public static UserAttribute ToEntity(this UserAttributeModel model, UserAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Address attributes

        //attributes
        public static AddressAttributeModel ToModel(this AddressAttribute entity)
        {
            return entity.MapTo<AddressAttribute, AddressAttributeModel>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model)
        {
            return model.MapTo<AddressAttributeModel, AddressAttribute>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model, AddressAttribute destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Languages

        public static LanguageModel ToModel(this Language entity)
        {
            return entity.MapTo<Language, LanguageModel>();
        }

        public static Language ToEntity(this LanguageModel model)
        {
            return model.MapTo<LanguageModel, Language>();
        }

        public static Language ToEntity(this LanguageModel model, Language destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Email account

        public static EmailAccountModel ToModel(this EmailAccount entity)
        {
            return entity.MapTo<EmailAccount, EmailAccountModel>();
        }

        public static EmailAccount ToEntity(this EmailAccountModel model)
        {
            return model.MapTo<EmailAccountModel, EmailAccount>();
        }

        public static EmailAccount ToEntity(this EmailAccountModel model, EmailAccount destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Message templates

        public static MessageTemplateModel ToModel(this MessageTemplate entity)
        {
            return entity.MapTo<MessageTemplate, MessageTemplateModel>();
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model)
        {
            return model.MapTo<MessageTemplateModel, MessageTemplate>();
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model, MessageTemplate destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Queued email

        public static QueuedEmailModel ToModel(this QueuedEmail entity)
        {
            return entity.MapTo<QueuedEmail, QueuedEmailModel>();
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model)
        {
            return model.MapTo<QueuedEmailModel, QueuedEmail>();
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model, QueuedEmail destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Log

        public static LogModel ToModel(this Log entity)
        {
            return entity.MapTo<Log, LogModel>();
        }

        public static Log ToEntity(this LogModel model)
        {
            return model.MapTo<LogModel, Log>();
        }

        public static Log ToEntity(this LogModel model, Log destination)
        {
            return model.MapTo(destination);
        }

        public static ActivityLogTypeModel ToModel(this ActivityLogType entity)
        {
            return entity.MapTo<ActivityLogType, ActivityLogTypeModel>();
        }

        public static ActivityLogModel ToModel(this ActivityLog entity)
        {
            return entity.MapTo<ActivityLog, ActivityLogModel>();
        }

        #endregion

        #region Orders
        public static OrderModel ToModel(this Order entity)
        {
            return entity.MapTo<Order, OrderModel>();
        }

        public static Order ToEntity(this OrderModel model)
        {
            return model.MapTo<OrderModel, Order>();
        }

        public static Order ToEntity(this OrderModel model, Order destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Currencies

        public static CurrencyModel ToModel(this Currency entity)
        {
            return entity.MapTo<Currency, CurrencyModel>();
        }

        public static Currency ToEntity(this CurrencyModel model)
        {
            return model.MapTo<CurrencyModel, Currency>();
        }

        public static Currency ToEntity(this CurrencyModel model, Currency destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Address

        public static AddressModel ToModel(this Address entity)
        {
            return entity.MapTo<Address, AddressModel>();
        }

        public static Address ToEntity(this AddressModel model)
        {
            return model.MapTo<AddressModel, Address>();
        }

        public static Address ToEntity(this AddressModel model, Address destination)
        {
            return model.MapTo(destination);
        }

        public static void PrepareCustomAddressAttributes(this AddressModel model,
            Address address,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            //this method is very similar to the same one in Invenio.Web project
            if (addressAttributeService == null)
                throw new ArgumentNullException("addressAttributeService");

            if (addressAttributeParser == null)
                throw new ArgumentNullException("addressAttributeParser");

            var attributes = addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressModel.AddressAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = addressAttributeService.GetAddressAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressModel.AddressAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                var selectedAddressAttributes = address != null ? address.CustomAttributes : null;
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
                                var selectedValues = addressAttributeParser.ParseAddressAttributeValues(selectedAddressAttributes);
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
                                var enteredText = addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
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

        #region Reports

        public static ReportModel ToModel(this Report entity)
        {
            return entity.MapTo<Report, ReportModel>();
        }

        public static Report ToEntity(this ReportModel model)
        {
            return model.MapTo<ReportModel, Report>();
        }

        #endregion

        #region User roles

        //User roles
        public static UserRoleModel ToModel(this UserRole entity)
        {
            return entity.MapTo<UserRole, UserRoleModel>();
        }

        public static UserRole ToEntity(this UserRoleModel model)
        {
            return model.MapTo<UserRoleModel, UserRole>();
        }

        public static UserRole ToEntity(this UserRoleModel model, UserRole destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Countries / states

        public static CountryModel ToModel(this Country entity)
        {
            return entity.MapTo<Country, CountryModel>();
        }

        public static Country ToEntity(this CountryModel model)
        {
            return model.MapTo<CountryModel, Country>();
        }

        public static Country ToEntity(this CountryModel model, Country destination)
        {
            return model.MapTo(destination);
        }

        public static StateProvinceModel ToModel(this StateProvince entity)
        {
            return entity.MapTo<StateProvince, StateProvinceModel>();
        }

        public static StateProvince ToEntity(this StateProvinceModel model)
        {
            return model.MapTo<StateProvinceModel, StateProvince>();
        }

        public static StateProvince ToEntity(this StateProvinceModel model, StateProvince destination)
        {
            return model.MapTo(destination);
        }


        #endregion

        #region Settings
        //User/user settings
        public static SupplierUserSettingsModel.UserSettingsModel ToModel(this UserSettings entity)
        {
            return entity.MapTo<UserSettings, SupplierUserSettingsModel.UserSettingsModel>();
        }
        public static UserSettings ToEntity(this SupplierUserSettingsModel.UserSettingsModel model, UserSettings destination)
        {
            return model.MapTo(destination);
        }
        public static SupplierUserSettingsModel.AddressSettingsModel ToModel(this AddressSettings entity)
        {
            return entity.MapTo<AddressSettings, SupplierUserSettingsModel.AddressSettingsModel>();
        }
        public static AddressSettings ToEntity(this SupplierUserSettingsModel.AddressSettingsModel model, AddressSettings destination)
        {
            return model.MapTo(destination);
        }



        //general (captcha) settings
        public static GeneralCommonSettingsModel.CaptchaSettingsModel ToModel(this CaptchaSettings entity)
        {
            return entity.MapTo<CaptchaSettings, GeneralCommonSettingsModel.CaptchaSettingsModel>();
        }
        public static CaptchaSettings ToEntity(this GeneralCommonSettingsModel.CaptchaSettingsModel model, CaptchaSettings destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Stores

        public static StoreModel ToModel(this Store entity)
        {
            return entity.MapTo<Store, StoreModel>();
        }

        public static Store ToEntity(this StoreModel model)
        {
            return model.MapTo<StoreModel, Store>();
        }

        public static Store ToEntity(this StoreModel model, Store destination)
        {
            return model.MapTo(destination);
        }

        #endregion
    }

}
