using Synnduit.Configuration;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Creates instances of the <see cref="IBootstrapper" /> implementation.
    /// </summary>
    internal interface IBootstrapperFactory
    {
        /// <summary>
        /// Creates a new <see cref="IBootstrapper" /> implementation instance.
        /// </summary>
        /// <param name="configurationProvider">
        /// The <see cref="IConfigurationProvider"/> instance providing access to the application
        /// configuration.
        /// </param>
        /// <param name="explicitAssemblies">
        /// The collection of assemblies whose exported types should be added to the dependency
        /// injection container (explicitly, in addition to those loaded from the
        /// <see cref="IApplicationConfiguration.BinaryFilesDirectoryPath"/> folder.)
        /// </param>
        /// <returns>
        /// A newly created <see cref="IBootstrapper" /> implementation instance.
        /// </returns>
        IBootstrapper CreateBootstrapper(
            IConfigurationProvider configurationProvider,
            IEnumerable<Assembly> explicitAssemblies);
    }
}
