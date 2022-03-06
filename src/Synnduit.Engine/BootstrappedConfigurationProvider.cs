using Microsoft.Extensions.Configuration;
using Synnduit.Configuration;
using IConfigurationProvider = Synnduit.Configuration.IConfigurationProvider;

namespace Synnduit
{
    /// <summary>
    /// Provides access to application configuration; this implementation is intended to be
    /// instantiated directly (as opposed to by the dependency injection container) and then passed
    /// to the (dependency-injected) <see cref="IWritableConfigurationProvider"/> (singleton)
    /// instance.
    /// </summary>
    internal class BootstrappedConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfiguration configuration;

        private readonly Lazy<ApplicationConfiguration> applicationConfiguration;

        public BootstrappedConfigurationProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.applicationConfiguration =
                new Lazy<ApplicationConfiguration>(this.GetApplicationConfiguration);
        }

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> instance that provides access to all
        /// configuration information.
        /// </summary>
        public IConfiguration Configuration => this.configuration;

        /// <summary>
        /// Gets the current application configuration.
        /// </summary>
        public IApplicationConfiguration ApplicationConfiguration =>
            this.applicationConfiguration.Value;

        private ApplicationConfiguration GetApplicationConfiguration() =>
            this.configuration.Get<ApplicationConfiguration>()
            ?? new ApplicationConfiguration();
    }
}
