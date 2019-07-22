using FluentValidation.Attributes;
using Invenio.Admin.Validators.Orders;
using Invenio.Web.Framework;
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
            AvailableCustomers = new List<SelectListItem>();
            AvailableDeliveryNumbers = new List<SelectListItem>();
            AvailablePartNumbers = new List<SelectListItem>();
        }

        //public int Id { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.Number")]
        public string Number { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.PartsPerHour")]
        public int PartsPerHour { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.TotalPartsQuantity")]
        public int TotalPartsQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Orders.Fields.CheckedPartsQuantity")]
        public long CheckedPartsQuantity { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Order.Fields.StartDate")]
        public DateTime? StartDate { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Order.Fields.EndDate")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.AvailableCustomers")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> AvailableCustomers { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.PartName")]
        public string PartName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.PartSerNumer")]
        public string PartSerNumer { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Orders.Fields.OrderStatus")]
        public string OrderStatus { get; set; }
        public int OrderStatusId { get; set; }

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

        [NopResourceDisplayName("Admin.Customers.Order.Fields.AvailablePartNumber")]
        public int PartNumberId { get; set; }
        public IList<SelectListItem> AvailablePartNumbers { get; set; }

        [NopResourceDisplayName("Admin.Orders.Parts.Fields.ChargeNumber")]
        [AllowHtml]
        public string AddOrderChargeNumber { get; set; }

        [NopResourceDisplayName("Admin.Customers.Order.Fields.AvailableDeliveryNumber")]
        public int DeliveryNumberId { get; set; }
        public IList<SelectListItem> AvailableDeliveryNumbers { get; set; }

        public int PartId { get; set; }
    }
}