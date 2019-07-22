using System;
using System.Collections;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

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
            Customers = new List<SelectListItem>();
            Orders = new List<SelectListItem>();
            DeliveryNumbers = new List<SelectListItem>();
            ChargeNumbers = new List<SelectListItem>();
            BlockedParts = new List<SelectListItem>();
            ReworkedParts = new List<SelectListItem>();
            NokCriteria = new List<CriteriaModel>();
            ReworkedCriteria = new List<CriteriaModel>();
            Parts = new List<SelectListItem>(); 
        }

        public int WorkShiftId { get; set; }
        public IList<SelectListItem> WorkShifts { get; set; }
        public DateTime ReportDate { get; set; }

        public int CustomerId { get; set; }
        public IList<SelectListItem> Customers { get; set; }

        public int PartId { get; set; }
        public IList<SelectListItem> Parts { get; set; }

        public int OrderId { get; set; }
        public IList<SelectListItem> Orders { get; set; }

        public int DeliveryNumberId { get; set; }
        public IList<SelectListItem> DeliveryNumbers { get; set; }

        public int ChargeNumberId { get; set; }
        public IList<SelectListItem> ChargeNumbers { get; set; }

        public int CheckedQuantity { get; set; }

        public int BlockedPartId { get; set; }
        public IList<SelectListItem> BlockedParts { get; set; }

        public int ReworkedPartId { get; set; }
        public IList<SelectListItem> ReworkedParts { get; set; }

        public IList<CriteriaModel> NokCriteria { get; set; }
        public IList<CriteriaModel> ReworkedCriteria { get; set; }
    }

    public class CriteriaModel : BaseNopModel
    {
        public int CriteriaId { get; set; }
        public int Quantity { get; set; }
    }
}