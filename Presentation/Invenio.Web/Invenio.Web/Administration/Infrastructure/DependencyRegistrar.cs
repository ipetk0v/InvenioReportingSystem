using Autofac;
using Autofac.Core;
using Invenio.Admin.Controllers;
using Invenio.Core.Caching;
using Invenio.Core.Configuration;
using Invenio.Core.Infrastructure;
using Invenio.Core.Infrastructure.DependencyManagement;

namespace Invenio.Admin.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            ////we cache presentation models between requests
            //builder.RegisterType<CategoryController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            builder.RegisterType<UserController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            builder.RegisterType<UserRoleController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            //builder.RegisterType<DiscountController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            builder.RegisterType<HomeController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            builder.RegisterType<ManufacturerController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            //builder.RegisterType<OrderController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            //builder.RegisterType<ProductController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 2; }
        }
    }
}
