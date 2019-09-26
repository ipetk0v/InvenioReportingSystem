using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Invenio.Web.Framework;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Orders
{
    public class OrderListModel : BaseNopModel
    {
        public OrderListModel()
        {
            AvailableStatus = new List<SelectListItem>();
            AvailablePublished = new List<SelectListItem>();
            AvailableSuppliers = new List<SelectListItem>();
            AvailableCustomers = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Order.List.Search.Number")]
        public string SearchOrderNumber { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.SearchByPartName")]
        public string SearchByPartName { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.Status")]
        public int? Status { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.SupplierId")]
        public int? SupplierId { get; set; }
        public IList<SelectListItem> AvailableSuppliers { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.CustomerId")]
        public int? CustomerId { get; set; }
        public IList<SelectListItem> AvailableCustomers { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.CreateDate")]
        [UIHint("DateNullable")]
        public DateTime? CreateDate { get; set; }

        [NopResourceDisplayName("Admin.Order.List.Search.Published")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublished { get; set; }
    }
}