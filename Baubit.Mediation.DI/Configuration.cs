using Baubit.DI;
using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Mediation.DI
{
    public class Configuration : AConfiguration
    {
        public string CacheRegistrationKey { get; set; } = null;
        public string RegistrationKey { get; set; }
        public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;
    }
}
