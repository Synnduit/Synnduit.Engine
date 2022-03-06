using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Creates <see cref="IDeploymentExecutor" /> instances.
    /// </summary>
    public static class DeploymentExecutorFactory
    {
        /// <summary>
        /// Creates a new <see cref="IDeploymentExecutor" /> instance.
        /// </summary>
        /// <returns>The <see cref="IDeploymentExecutor" /> instance created.</returns>
        public static IDeploymentExecutor CreateDeploymentExecutor(
            IConfiguration configuration, params Assembly[] explicitAssemblies)
        {
            ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
            ArgumentValidator.EnsureArgumentsNotNullAndUnique(
                explicitAssemblies, nameof(explicitAssemblies));
            return new DeploymentExecutor(
                new BootstrappedConfigurationProvider(configuration),
                BootstrapperFactory.Instance,
                explicitAssemblies);
        }
    }
}
