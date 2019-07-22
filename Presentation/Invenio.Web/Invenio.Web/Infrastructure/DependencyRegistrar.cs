using Autofac;
using Invenio.Core.Configuration;
using Invenio.Core.Infrastructure;
using Invenio.Core.Infrastructure.DependencyManagement;
using Invenio.Web.Infrastructure.Installation;
using System;
using Autofac.Core;
using Invenio.Core.Caching;
using Invenio.Web.Controllers;
using Invenio.Web.Factories;

namespace Invenio.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistar
    {
        public int Order => 2;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerLifetimeScope();

            //factories (we cache presentation models between HTTP requests)
            builder.RegisterType<CommonModelFactory>().As<ICommonModelFactory>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"))
                .InstancePerLifetimeScope();

            //factories (we cache presentation models between HTTP requests)
            builder.RegisterType<AddressModelFactory>().As<IAddressModelFactory>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CommonModelFactory>().As<ICommonModelFactory>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"))
                .InstancePerLifetimeScope();

            //builder.RegisterType<CountryModelFactory>().As<ICountryModelFactory>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"))
            //    .InstancePerLifetimeScope();

            builder.RegisterType<UserModelFactory>().As<IUserModelFactory>()
                .InstancePerLifetimeScope();
        }
    }
}