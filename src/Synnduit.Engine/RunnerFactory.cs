using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Creates <see cref="IRunner" /> instances.
    /// </summary>
    public static class RunnerFactory
    {
        /// <summary>
        /// Creates a new <see cref="IRunner" /> instance for the specified configuration
        /// and run.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> instance representing the application configuration.
        /// </param>
        /// <param name="runName">
        /// The name of the run to configure the <see cref="IRunner" /> instance for; must
        /// exist within the specified configuration (file).
        /// </param>
        /// <param name="explicitAssemblies">
        /// The collection of assemblies whose exported types should be added to the dependency
        /// injection container (explicitly, in addition to those loaded from the
        /// <see cref="Configuration.IApplicationConfiguration.BinaryFilesDirectoryPath"/> folder.)
        /// </param>
        /// <returns>
        /// The <see cref="IRunner" /> instance created for the specified configuration
        /// and run.
        /// </returns>
        public static IRunner CreateRunner(
            IConfiguration configuration,
            string runName,
            params Assembly[] explicitAssemblies)
        {
            ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
            ArgumentValidator.EnsureArgumentNotNullOrWhiteSpace(runName, nameof(runName));
            ArgumentValidator.EnsureArgumentsNotNullAndUnique(
                explicitAssemblies, nameof(explicitAssemblies));
            return new Runner(
                new BootstrappedConfigurationProvider(configuration),
                BootstrapperFactory.Instance,
                BridgeFactory.Instance,
                runName,
                explicitAssemblies);
        }
    }
}
