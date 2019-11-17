using FluentValidation.Attributes;
using Invenio.Admin.Validators.Orders;
using Invenio.Core.Domain.Orders;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Localization;
using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Orders
{
    [Validator(typeof(OrderValidator))]
    public class OrderModel : BaseNopEntityModel
    {
        public OrderModel()
        {
            AvailableSuppliers = new List<SelectListItem>();
            AvailableDeliveryNumbers = new List<SelectListItem>();
            AvailablePartNumbers = new List<SelectListItem>();
            AvailableOrderAttributes = new List<OrderAttributeModel>();
        }

        //public int Id { get; set; }

        public string TabReload { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.Number")]
        public string Number { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.PartsPerHour")]
        public int PartsPerHour { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.TotalPartsQuantity")]
        public int TotalPartsQuantity { get; set; }

        public bool IsChargeNumberQuantityAvailable { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Orders.Fields.CheckedPartsQuantity")]
        public long CheckedPartsQuantity { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.StartDate")]
        public DateTime? StartDate { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.EndDate")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.AvailableSuppliers")]
        public int supplierId { get; set; }
        public IList<SelectListItem> AvailableSuppliers { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.PartName")]
        public string PartName { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.PartSerNumer")]
        public string PartSerNumer { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Orders.Fields.OrderStatus")]
        public string OrderStatus { get; set; }
        public int OrderStatusId { get; set; }

        public bool ShouldHaveValues { get; set; }
        public int TotalValues { get; set; }

        [NopResourceDisplayName("Admin.Orders.Criteria.Fields.Description")]
        [AllowHtml]
        public string AddBlockedCriteriaDescription { get; set; }

        [NopResourceDisplayName("Admin.Orders.Criteria.Fields.Description")]
        [AllowHtml]
        public string AddReworkedCriteriaDescription { get; set; }

        [NopResourceDisplayName("Admin.Orders.Parts.Fields.SerNumber")]
        [AllowHtml]
        public string AddOrderPartsSerNumber { get; set; }

        [NopResourceDisplayName("Admin.Orders.Parts.Fields.DeliverNumber")]
        [AllowHtml]
        public string AddOrderDeliveryNumber { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.AvailablePartNumber")]
        public int PartNumberId { get; set; }
        public IList<SelectListItem> AvailablePartNumbers { get; set; }

        public IList<OrderAttributeModel> AvailableOrderAttributes { get; set; }

        [NopResourceDisplayName("Admin.Orders.Parts.Fields.ChargeNumber")]
        [AllowHtml]
        public string AddOrderChargeNumber { get; set; }

        [NopResourceDisplayName("Admin.Orders.Parts.Fields.ChargeNumberQuantity")]
        public int AddOrderChargeNumberQuantity { get; set; }

        [NopResourceDisplayName("Admin.Suppliers.Order.Fields.AvailableDeliveryNumber")]
        public int DeliveryNumberId { get; set; }
        public IList<SelectListItem> AvailableDeliveryNumbers { get; set; }

        public int PartId { get; set; }

        public partial class OrderAttributeMappingModel : BaseNopEntityModel
        {
            public int OrderId { get; set; }

            public int OrderAttributeId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Fields.Attribute")]
            public string OrderAttribute { get; set; }

            public int ParentOrderAttributeId { get; set; }
            public IList<OrderAttribute> OrderAttributes { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Fields.TextPrompt")]
            [AllowHtml]
            public string TextPrompt { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Fields.IsRequired")]
            public bool IsRequired { get; set; }

            public int AttributeControlTypeId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Fields.AttributeControlType")]
            public string AttributeControlType { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public bool ShouldHaveValues { get; set; }
            public int TotalValues { get; set; }

            //validation fields
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules")]
            public bool ValidationRulesAllowed { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.MinLength")]
            [UIHint("Int32Nullable")]
            public int? ValidationMinLength { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.MaxLength")]
            [UIHint("Int32Nullable")]
            public int? ValidationMaxLength { get; set; }
            [AllowHtml]
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.FileAllowedExtensions")]
            public string ValidationFileAllowedExtensions { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.FileMaximumSize")]
            [UIHint("Int32Nullable")]
            public int? ValidationFileMaximumSize { get; set; }
            [AllowHtml]
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.ValidationRules.DefaultValue")]
            public string DefaultValue { get; set; }
            public string ValidationRulesString { get; set; }

            //condition
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Condition")]
            public bool ConditionAllowed { get; set; }
            public string ConditionString { get; set; }
        }
        public partial class OrderAttributeValueListModel : BaseNopModel
        {
            public int OrderId { get; set; }

            public string OrderName { get; set; }

            public int OrderAttributeMappingId { get; set; }

            public string OrderAttributeName { get; set; }
        }

        //[Validator(typeof(OrderAttributeValueModelValidator))]
        public partial class OrderAttributeValueModel : BaseNopEntityModel, ILocalizedModel<OrderAttributeValueLocalizedModel>
        {
            public OrderAttributeValueModel()
            {
                Locales = new List<OrderAttributeValueLocalizedModel>();
                ParentAttributeValues = new List<SelectListItem>();
            }

            public int OrderAttributeMappingId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Values.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }
            
            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Values.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public string ParentAttributeName { get; set; }

            public int ParentOrderAttributeMappingId { get; set; }

            public int? ParentAttributeValueId { get; set; }
            public IList<SelectListItem> ParentAttributeValues { get; set; }

            public IList<OrderAttributeValueLocalizedModel> Locales { get; set; }
        }

        public partial class OrderAttributeValueLocalizedModel : ILocalizedModelLocal
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Orders.OrderAttributes.Attributes.Values.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }
        }
    }
}