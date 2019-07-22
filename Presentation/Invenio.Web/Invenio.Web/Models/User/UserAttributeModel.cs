using System.Collections.Generic;
using Invenio.Core.Domain.Catalog;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Web.Models.User
{
    public partial class UserAttributeModel : BaseNopEntityModel
    {
        public UserAttributeModel()
        {
            Values = new List<UserAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value for textboxes
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<UserAttributeValueModel> Values { get; set; }

    }

    public partial class UserAttributeValueModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}