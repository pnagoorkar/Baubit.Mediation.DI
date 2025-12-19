using Microsoft.Extensions.DependencyInjection;

namespace Baubit.Mediation.DI.Test.Configuration
{
    /// <summary>
    /// Unit tests for <see cref="DI.Configuration"/>
    /// </summary>
    public class Test
    {
        [Fact]
        public void Constructor_SetsDefaultValues_WhenCreated()
        {
            // Arrange & Act
            var configuration = new DI.Configuration();

            // Assert
            Assert.Null(configuration.CacheRegistrationKey);
            Assert.Null(configuration.RegistrationKey);
            Assert.Equal(ServiceLifetime.Singleton, configuration.ServiceLifetime);
        }

        [Fact]
        public void CacheRegistrationKey_CanBeSet_WhenProvided()
        {
            // Arrange
            var configuration = new DI.Configuration();
            var expectedKey = "cache-key";

            // Act
            configuration.CacheRegistrationKey = expectedKey;

            // Assert
            Assert.Equal(expectedKey, configuration.CacheRegistrationKey);
        }

        [Fact]
        public void RegistrationKey_CanBeSet_WhenProvided()
        {
            // Arrange
            var configuration = new DI.Configuration();
            var expectedKey = "mediator-key";

            // Act
            configuration.RegistrationKey = expectedKey;

            // Assert
            Assert.Equal(expectedKey, configuration.RegistrationKey);
        }

        [Theory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped)]
        public void ServiceLifetime_CanBeSet_WhenProvided(ServiceLifetime lifetime)
        {
            // Arrange
            var configuration = new DI.Configuration();

            // Act
            configuration.ServiceLifetime = lifetime;

            // Assert
            Assert.Equal(lifetime, configuration.ServiceLifetime);
        }

        [Fact]
        public void Configuration_IsSubclassOfConfiguration()
        {
            // Arrange & Act
            var configuration = new DI.Configuration();

            // Assert
            Assert.IsAssignableFrom<Baubit.DI.Configuration>(configuration);
        }
    }
}
