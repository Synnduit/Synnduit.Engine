using Microsoft.Extensions.Configuration;
using Synnduit.Properties;
using System.ComponentModel.Composition;

namespace Synnduit.Configuration
{
    /// <summary>
    /// Provides read-write access to application configuration.
    /// </summary>
    [Export(typeof(IWritableConfigurationProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class WritableConfigurationProvider : IWritableConfigurationProvider
    {
        private IConfigurationProvider configurationProvider;

        [ImportingConstructor]
        public WritableConfigurationProvider()
        {
            this.configurationProvider = null;
        }

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> instance that provides access to all
        /// configuration information.
        /// </summary>
        public IConfiguration Configuration => this.ConfigurationProvider.Configuration;

        /// <summary>
        /// Gets the current application configuration.
        /// </summary>
        public IApplicationConfiguration ApplicationConfiguration =>
            this.ConfigurationProvider.ApplicationConfiguration;

        /// <summary>
        /// Sets the underlying <see cref="IConfigurationProvider" /> implementation instance; the
        /// configuration data will be retrieved from this instance.
        /// </summary>
        /// <param name="configurationProvider">
        /// The underlying <see cref="IConfigurationProvider" /> implementation instance.
        /// </param>
        public void SetConfigurationProvider(IConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        private IConfigurationProvider ConfigurationProvider
        {
            get
            {
                if (this.configurationProvider == null)
                {
                    throw new InvalidOperationException(Resources.ConfigurationNotAvailable);
                }
                return this.configurationProvider;
            }
        }
    }
}
