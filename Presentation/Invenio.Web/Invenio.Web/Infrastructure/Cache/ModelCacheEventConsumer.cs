using Invenio.Core.Caching;
//using Invenio.Core.Domain.Blogs;
using Invenio.Core.Domain.Catalog;
using Invenio.Core.Domain.Configuration;
using Invenio.Core.Domain.Directory;
using Invenio.Core.Domain.Localization;
using Invenio.Core.Domain.Media;
//using Invenio.Core.Domain.News;
//using Invenio.Core.Domain.Orders;
//using Invenio.Core.Domain.Polls;
//using Invenio.Core.Domain.Topics;
//using Invenio.Core.Domain.Vendors;
using Invenio.Core.Events;
using Invenio.Core.Infrastructure;
using Invenio.Services.Events;

namespace Invenio.Web.Infrastructure.Cache
{
    public class ModelCacheEventConsumer
        : IConsumer<Setting>
    {
        private readonly ICacheManager _cacheManager;
        //private readonly CatalogSettings _catalogSettings;

        public ModelCacheEventConsumer(/*CatalogSettings catalogSettings*/)
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            //this._catalogSettings = catalogSettings;
        }

        /// <summary>
        /// Key for logo
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : current theme
        /// {2} : is connection SSL secured (included in a picture URL)
        /// </remarks>
        public const string STORE_LOGO_PATH = "Nop.pres.logo-{0}-{1}-{2}";
        public const string STORE_LOGO_PATH_PATTERN_KEY = "Nop.pres.logo";

        public void HandleEvent(Setting eventMessage)
        {
            _cacheManager.RemoveByPattern(STORE_LOGO_PATH_PATTERN_KEY); //depends on StoreInformationSettings.LogoPictureId
        }
    }
}