using System;
using System.Collections.Generic;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Home
{
    public partial class SystemNewsModel : BaseNopModel
    {
        public SystemNewsModel()
        {
            Items = new List<NewsDetailsModel>();
        }

        public List<NewsDetailsModel> Items { get; set; }
        public bool HasNewItems { get; set; }
        public bool HideAdvertisements { get; set; }

        public class NewsDetailsModel : BaseNopModel
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string Summary { get; set; }
            public DateTimeOffset PublishDate { get; set; }
        }
    }
}