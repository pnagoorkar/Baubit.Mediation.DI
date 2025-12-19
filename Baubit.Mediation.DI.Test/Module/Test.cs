using Baubit.Caching;
using Baubit.DI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Baubit.Mediation.DI.Test.Module
{
    /// <summary>
    /// Unit tests for <see cref="DI.Module"/>
    /// </summary>
    public class Test
    {
        /// <summary>
        /// Helper method to create a ServiceCollection with cache dependencies
        /// </summary>
        /// <param name="cacheKey">Optional registration key for the cache. When null, cache is registered without a key.</param>
        private ServiceCollection CreateServicesWithCacheDependencies(string? cacheKey = null)
        {
            var services = new ServiceCollection();
            
            // Add logger factory
            services.AddLogging();
            
            // Register mock cache for testing
            var mockCache = Substitute.For<IOrderedCache<object>>();
            
            if (cacheKey == null)
            {
                services.AddSingleton<IOrderedCache<object>>(mockCache);
            }
            else
            {
                services.AddKeyedSingleton<IOrderedCache<object>>(cacheKey, mockCache);
            }
            
            return services;
        }

        [Fact]
        public void Constructor_WithIConfiguration_CreatesModule()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RegistrationKey", "test-key" }
            });
            var config = configBuilder.Build();

            // Act
            var module = new DI.Module(config);

            // Assert
            Assert.NotNull(module);
            Assert.NotNull(module.Configuration);
        }

        [Fact]
        public void Constructor_WithConfiguration_CreatesModule()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                RegistrationKey = "test-key",
                ServiceLifetime = ServiceLifetime.Transient
            };

            // Act
            var module = new DI.Module(configuration);

            // Assert
            Assert.NotNull(module);
            Assert.Equal("test-key", module.Configuration.RegistrationKey);
            Assert.Equal(ServiceLifetime.Transient, module.Configuration.ServiceLifetime);
        }

        [Fact]
        public void Constructor_WithNestedModules_CreatesModule()
        {
            // Arrange
            var configuration = new DI.Configuration();
            var nestedModules = new List<Baubit.DI.IModule>();

            // Act
            var module = new DI.Module(configuration, nestedModules);

            // Assert
            Assert.NotNull(module);
            Assert.NotNull(module.NestedModules);
        }

        [Fact]
        public void Load_RegistersSingletonWithoutKey_WhenRegistrationKeyIsNull()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                RegistrationKey = null,
                ServiceLifetime = ServiceLifetime.Singleton
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator);
        }

        [Fact]
        public void Load_RegistersSingletonWithKey_WhenRegistrationKeyIsProvided()
        {
            // Arrange
            var registrationKey = "mediator-key";
            var configuration = new DI.Configuration
            {
                RegistrationKey = registrationKey,
                ServiceLifetime = ServiceLifetime.Singleton
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetKeyedService<IMediator>(registrationKey);
            Assert.NotNull(mediator);
        }

        [Fact]
        public void Load_RegistersTransientWithoutKey_WhenServiceLifetimeIsTransient()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                RegistrationKey = null,
                ServiceLifetime = ServiceLifetime.Transient
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator1 = serviceProvider.GetService<IMediator>();
            var mediator2 = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator1);
            Assert.NotNull(mediator2);
            Assert.NotSame(mediator1, mediator2);
        }

        [Fact]
        public void Load_RegistersTransientWithKey_WhenRegistrationKeyIsProvided()
        {
            // Arrange
            var registrationKey = "mediator-key";
            var configuration = new DI.Configuration
            {
                RegistrationKey = registrationKey,
                ServiceLifetime = ServiceLifetime.Transient
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator1 = serviceProvider.GetKeyedService<IMediator>(registrationKey);
            var mediator2 = serviceProvider.GetKeyedService<IMediator>(registrationKey);
            Assert.NotNull(mediator1);
            Assert.NotNull(mediator2);
            Assert.NotSame(mediator1, mediator2);
        }

        [Fact]
        public void Load_RegistersScopedWithoutKey_WhenServiceLifetimeIsScoped()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                RegistrationKey = null,
                ServiceLifetime = ServiceLifetime.Scoped
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            using (var scope1 = serviceProvider.CreateScope())
            using (var scope2 = serviceProvider.CreateScope())
            {
                var mediator1 = scope1.ServiceProvider.GetService<IMediator>();
                var mediator2 = scope2.ServiceProvider.GetService<IMediator>();
                Assert.NotNull(mediator1);
                Assert.NotNull(mediator2);
                Assert.NotSame(mediator1, mediator2);
            }
        }

        [Fact]
        public void Load_RegistersScopedWithKey_WhenRegistrationKeyIsProvided()
        {
            // Arrange
            var registrationKey = "mediator-key";
            var configuration = new DI.Configuration
            {
                RegistrationKey = registrationKey,
                ServiceLifetime = ServiceLifetime.Scoped
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            using (var scope1 = serviceProvider.CreateScope())
            using (var scope2 = serviceProvider.CreateScope())
            {
                var mediator1 = scope1.ServiceProvider.GetKeyedService<IMediator>(registrationKey);
                var mediator2 = scope2.ServiceProvider.GetKeyedService<IMediator>(registrationKey);
                Assert.NotNull(mediator1);
                Assert.NotNull(mediator2);
                Assert.NotSame(mediator1, mediator2);
            }
        }

        [Fact]
        public void Load_ResolvesUnkeyedCache_WhenCacheRegistrationKeyIsNull()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                CacheRegistrationKey = null,
                ServiceLifetime = ServiceLifetime.Singleton
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator);
        }

        [Fact]
        public void Load_ResolvesKeyedCache_WhenCacheRegistrationKeyIsProvided()
        {
            // Arrange
            var cacheKey = "cache-key";
            var configuration = new DI.Configuration
            {
                CacheRegistrationKey = cacheKey,
                ServiceLifetime = ServiceLifetime.Singleton
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies(cacheKey);

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator = serviceProvider.GetService<IMediator>();
            Assert.NotNull(mediator);
        }

        [Fact]
        public void Load_SingletonReturnsSameInstance_WhenResolvedMultipleTimes()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                ServiceLifetime = ServiceLifetime.Singleton
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mediator1 = serviceProvider.GetService<IMediator>();
            var mediator2 = serviceProvider.GetService<IMediator>();
            Assert.Same(mediator1, mediator2);
        }

        [Fact]
        public void Load_ScopedReturnsSameInstanceInSameScope_WhenResolvedMultipleTimes()
        {
            // Arrange
            var configuration = new DI.Configuration
            {
                ServiceLifetime = ServiceLifetime.Scoped
            };
            var module = new DI.Module(configuration);
            var services = CreateServicesWithCacheDependencies();

            // Act
            module.Load(services);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            using (var scope = serviceProvider.CreateScope())
            {
                var mediator1 = scope.ServiceProvider.GetService<IMediator>();
                var mediator2 = scope.ServiceProvider.GetService<IMediator>();
                Assert.Same(mediator1, mediator2);
            }
        }

        [Fact]
        public void Module_IsSubclassOfModule()
        {
            // Arrange
            var configuration = new DI.Configuration();

            // Act
            var module = new DI.Module(configuration);

            // Assert
            Assert.IsAssignableFrom<Baubit.DI.Module<DI.Configuration>>(module);
        }
    }
}

