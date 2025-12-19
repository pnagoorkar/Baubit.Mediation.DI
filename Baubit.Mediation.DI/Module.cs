using Baubit.Caching;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Mediation.DI
{
    /// <summary>
    /// DI module for registering <see cref="IMediator"/> with Microsoft.Extensions.DependencyInjection.
    /// Configures the mediator with specified service lifetime and optional keyed registration.
    /// </summary>
    public class Module : Baubit.DI.Module<Configuration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class from configuration.
        /// </summary>
        /// <param name="configuration">The configuration section to bind settings from.</param>
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class with strongly-typed configuration.
        /// </summary>
        /// <param name="configuration">The strongly-typed configuration for this module.</param>
        /// <param name="nestedModules">Optional list of nested modules this module depends on.</param>
        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        /// <summary>
        /// Registers the <see cref="IMediator"/> service with the specified service collection.
        /// The registration respects the configured <see cref="Configuration.ServiceLifetime"/> and <see cref="Configuration.RegistrationKey"/>.
        /// </summary>
        /// <param name="services">The service collection to register services with.</param>
        public override void Load(IServiceCollection services)
        {
            switch (Configuration.ServiceLifetime)
            {
                case ServiceLifetime.Singleton:
                    if (Configuration.RegistrationKey == null)
                    {
                        services.AddSingleton(serviceProvider => CreateMediator(serviceProvider));
                    }
                    else
                    {
                        services.AddKeyedSingleton(Configuration.RegistrationKey, (serviceProvider, _) => CreateMediator(serviceProvider));
                    }
                    break;
                case ServiceLifetime.Transient:
                    if (Configuration.RegistrationKey == null)
                    {
                        services.AddTransient(serviceProvider => CreateMediator(serviceProvider));
                    }
                    else
                    {
                        services.AddKeyedTransient(Configuration.RegistrationKey, (serviceProvider, _) => CreateMediator(serviceProvider));
                    }
                    break;
                case ServiceLifetime.Scoped:
                    if (Configuration.RegistrationKey == null)
                    {
                        services.AddScoped(serviceProvider => CreateMediator(serviceProvider));
                    }
                    else
                    {
                        services.AddKeyedScoped(Configuration.RegistrationKey, (serviceProvider, _) => CreateMediator(serviceProvider));
                    }
                    break;
            }
            base.Load(services);
        }

        private IMediator CreateMediator(IServiceProvider serviceProvider)
        {
            return new Mediator(CreateOrderedCache(serviceProvider), serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        private IOrderedCache<object> CreateOrderedCache(IServiceProvider serviceProvider)
        {
            return Configuration.CacheRegistrationKey == null ?
                   serviceProvider.GetRequiredService<IOrderedCache<object>>() :
                   serviceProvider.GetRequiredKeyedService<IOrderedCache<object>>(Configuration.CacheRegistrationKey);
        }
    }
}
