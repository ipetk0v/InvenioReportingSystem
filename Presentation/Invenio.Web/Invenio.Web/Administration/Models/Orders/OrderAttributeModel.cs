using Invenio.Web.Framework;
using Invenio.Web.Framework.Localization;
using Invenio.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Invenio.Admin.Models.Orders
{
    public class OrderAttributeModel : BaseNopEntityModel, ILocalizedModel<OrderAttributeLocalizedModel>
    {
        public OrderAttributeModel()
        {
            Locales = new List<OrderAttributeLocalizedModel>();
            ParentOrderAttributeList = new List<SelectListItem>();
            Parts = new List<OrderPartsModel>();
        }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        //[NopResourceDisplayName("Admin.Catalog.Attributes.ÒrderAttributes.Fields.Description")]
        //[AllowHtml]
        //public string Description { get; set; }

        public int PartId { get; set; }
        public IList<OrderPartsModel> Parts { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.UsedByOrders.ParentOrderAttributeId")]
        public int? ParentOrderAttributeId { get; set; }
        public IList<SelectListItem> ParentOrderAttributeList { get; set; }

        public IList<OrderAttributeLocalizedModel> Locales { get; set; }

        public partial class UsedByOrderModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.UsedByOrders.Order")]
            public string OrderNumber { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.UsedByOrders.Published")]
            public bool Published { get; set; }
        }
    }

    public partial class OrderAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }
    }

    public partial class PredefinedOrderAttributeValueModel : BaseNopEntityModel, ILocalizedModel<PredefinedOrderAttributeValueLocalizedModel>
    {
        public PredefinedOrderAttributeValueModel()
        {
            Locales = new List<PredefinedOrderAttributeValueLocalizedModel>();
        }

        public int OrderAttributeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.PredefinedValues.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.PredefinedValues.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<PredefinedOrderAttributeValueLocalizedModel> Locales { get; set; }
    }

    public partial class PredefinedOrderAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.OrderAttributes.PredefinedValues.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}