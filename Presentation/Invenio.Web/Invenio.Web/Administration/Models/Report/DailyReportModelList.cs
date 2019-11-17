using Invenio.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Report
{
    public class DailyReportModelList : BaseNopEntityModel
    {
        public DailyReportModelList()
        {
            Data = new List<object>();
        }

        public int OrderId { get; set; }
        public IList<Object> Data { get; set; }
        public string DataString { get; set; }
        public JsonResult DataJson { get; set; }
    }
}