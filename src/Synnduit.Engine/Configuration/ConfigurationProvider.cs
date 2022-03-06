using Microsoft.Extensions.Configuration;
using System.ComponentModel.Composition;

namespace Synnduit.Configuration
{
    /// <summary>
    /// Provides access to application configuration.
    /// </summary>
    [Export(typeof(IConfigurationProvider))]
    internal class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IWritableConfigurationProvider writableConfigurationProvider;

        [ImportingConstructor]
        public ConfigurationProvider(IWritableConfigurationProvider writableConfigurationProvider)
        {
            this.writableConfigurationProvider = writableConfigurationProvider;
        }

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> instance that provides access to all
        /// configuration information.
        /// </summary>
        public IConfiguration Configuration => this.writableConfigurationProvider.Configuration;

        /// <summary>
        /// Gets the current application configuration.
        /// </summary>
        public IApplicationConfiguration ApplicationConfiguration =>
            this.writableConfigurationProvider.ApplicationConfiguration;
    }
}
