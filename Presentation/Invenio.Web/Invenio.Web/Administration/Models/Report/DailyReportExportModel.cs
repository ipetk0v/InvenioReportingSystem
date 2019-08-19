using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Invenio.Core.Domain.Criterias;
using Invenio.Core.Domain.Orders;

namespace Invenio.Admin.Models.Report
{
    public class DailyReportExportModel
    {

        public DailyReportExportModel(IEnumerable<Core.Domain.Criterias.Criteria> criterias,
            IOrderedEnumerable<DailyReportModel> items, Order order)
        {
            _criterias = criterias;
            Items = items;
            _order = order;
        }

        public IEnumerable<DailyReportModel> MainTableItems { get; set; }

        private readonly IEnumerable<Core.Domain.Criterias.Criteria> _criterias;
        public IOrderedEnumerable<DailyReportModel> Items { get; set; }
        private readonly Order _order;


        public List<Core.Domain.Criterias.Criteria> BlockedCriterias
        {
            get
            {
                return _criterias.Where(x => x.CriteriaType == CriteriaType.BlockedParts).OrderBy(x => x.Id).ToList();
            }
        }

        public List<Core.Domain.Criterias.Criteria> ReworkedCriterias
        {
            get
            {
                return _criterias.Where(x => x.CriteriaType == CriteriaType.ReworkParts).OrderBy(x => x.Id).ToList();
            }
        }

        public string DescriptionOfBlockedParts
        {
            get
            {
                var j = 1;
                return string.Join("\r\n", _criterias.OrderBy(w => w.Id).Select(s => j++ + ". " + s.Description));

            }
        }

        public string PartNumber
        {
            get
            {
                return string.Join(", ", Items.Select(s => s.PartNumber).Distinct().ToList());
            }
        }

        public string OrderNo
        {
            get { return _order.Number; }
        }

        public int QuantityToCheck
        {
            get { return _order.TotalPartsQuantity; }
        }

        public string SupplierName
        {
            get { return _order.Supplier.Name; }
        }

        public long TotalChecked
        {
            get { return Items.Sum(w => w.Quantity); }
        }

        public long TotalOk
        {
            get { return Items.Sum(w => w.FirstRunOkParts); }
        }

        public long TotalBlocked
        {
            get { return Items.Sum(w => w.BlockedParts); }
        }

        public long TotalNok
        {
            get { return Items.Sum(w => w.NokParts); }
        }

        public long TotalReworked
        {
            get { return Items.Sum(w => w.ReworkedParts); }
        }
    }
}