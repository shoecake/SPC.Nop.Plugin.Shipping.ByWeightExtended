using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.ByWeightExtended.Data;
using Nop.Plugin.Shipping.ByWeightExtended.Domain;
using Nop.Plugin.Shipping.ByWeightExtended.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByWeightExtended
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ShippingByWeightExtendedService>().As<IShippingByWeightExtendedService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<ShippingByWeightExtendedObjectContext>(builder, "nop_object_context_shipping_weight_zip");
            this.RegisterPluginDataContext<ProductMapToShippingMethodObjectContext>(builder, "nop_object_context_shipping_weight_product");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ShippingByWeightExtendedRecord>>()
                .As<IRepository<ShippingByWeightExtendedRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_shipping_weight_zip"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ProductMapToShippingMethodRecord>>()
               .As<IRepository<ProductMapToShippingMethodRecord>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_shipping_weight_product"))
               .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
