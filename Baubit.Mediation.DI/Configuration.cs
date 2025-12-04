using Baubit.DI;
using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Mediation.DI
{
    /// <summary>
    /// Configuration class for the Mediation DI module.
    /// Specifies how the <see cref="IMediator"/> instance should be registered in the service collection.
    /// </summary>
    public class Configuration : AConfiguration
    {
        /// <summary>
        /// Gets or sets the registration key for retrieving the <see cref="Baubit.Caching.IOrderedCache{T}"/> dependency.
        /// When null, the unkeyed <see cref="Baubit.Caching.IOrderedCache{T}"/> service is resolved.
        /// </summary>
        public string CacheRegistrationKey { get; set; } = null;

        /// <summary>
        /// Gets or sets the registration key for the <see cref="IMediator"/> service.
        /// When null, the <see cref="IMediator"/> is registered without a key.
        /// When specified, the <see cref="IMediator"/> is registered as a keyed service.
        /// </summary>
        public string RegistrationKey { get; set; }

        /// <summary>
        /// Gets or sets the service lifetime for the <see cref="IMediator"/> registration.
        /// Defaults to <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;
    }
}
