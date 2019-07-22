using System.Collections.Generic;
using Invenio.Admin.Models.Localization;
using Invenio.Web.Framework.Mvc;

namespace Invenio.Admin.Models.Common
{
    public partial class LanguageSelectorModel : BaseNopModel
    {
        public LanguageSelectorModel()
        {
            AvailableLanguages = new List<LanguageModel>();
        }

        public IList<LanguageModel> AvailableLanguages { get; set; }

        public LanguageModel CurrentLanguage { get; set; }
    }
}