using System;
using System.Collections;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;
using Invenio.Core.Domain.Orders;

namespace Invenio.Web.Models.Home
{
    public class ReportViewModel : BaseNopModel
    {
        public ReportViewModel()
        {
            Reports = new List<ReportModel>();
        }

        public IList<ReportModel> Reports { get; set; }
    }

    public class ReportModel : BaseNopModel
    {
        public ReportModel()
        {
            WorkShifts = new List<SelectListItem>();
            Suppliers = new List<SelectListItem>();
            Orders = new List<SelectListItem>();
            BlockedParts = new List<SelectListItem>();
            ReworkedParts = new List<SelectListItem>();
            NokCriteria = new List<CriteriaModel>();
            ReworkedCriteria = new List<CriteriaModel>();
            Attributes = new List<OrderAttributeModel>();
            PostedAttributes = new List<PostedAttributesModel>();
        }

        public int WorkShiftId { get; set; }
        public IList<SelectListItem> WorkShifts { get; set; }
        public DateTime ReportDate { get; set; }

        public int supplierId { get; set; }
        public IList<SelectListItem> Suppliers { get; set; }

        public int OrderId { get; set; }
        public IList<SelectListItem> Orders { get; set; }

        public IList<OrderAttributeModel> Attributes { get; set; }

        public int CheckedQuantity { get; set; }

        public int InputTime { get; set; }

        public int BlockedPartId { get; set; }
        public IList<SelectListItem> BlockedParts { get; set; }

        public int ReworkedPartId { get; set; }
        public IList<SelectListItem> ReworkedParts { get; set; }

        public IList<CriteriaModel> NokCriteria { get; set; }
        public IList<CriteriaModel> ReworkedCriteria { get; set; }
        public IList<PostedAttributesModel> PostedAttributes { get; set; }
    }

    public class PostedAttributesModel
    {
        public int AttributeId { get; set; }

        public int AttributeValueId { get; set; }
    }

    public class OrderAttributeMappingModel
    {
        public OrderAttributeMappingModel()
        {
            OrderAttributes = new List<OrderAttributeModel>();
        }

        public IList<OrderAttributeModel> OrderAttributes { get; set; }
    }

    public class OrderAttributeModel
    {
        public OrderAttributeModel()
        {
            OrderAttributeValues = new List<OrderAttributeValue>();
        }

        public string OrderAttribute { get; set; }
        public int OrderAttributeId { get; set; }
        public IList<OrderAttributeValue> OrderAttributeValues { get; set; }
    }
    
    public class CriteriaModel : BaseNopModel
    {
        public int CriteriaId { get; set; }
        public int Quantity { get; set; }
    }
}