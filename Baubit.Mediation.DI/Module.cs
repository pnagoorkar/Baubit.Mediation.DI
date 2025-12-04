using Baubit.Caching;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Baubit.Mediation.DI
{
    public class Module : AModule<Configuration>
    {
        public Module(IConfiguration configuration) : base(configuration)
        {
        }

        public Module(Configuration configuration, List<IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

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
