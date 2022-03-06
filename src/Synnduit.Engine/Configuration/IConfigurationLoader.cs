namespace Synnduit.Configuration
{
    /// <summary>
    /// Loads individual application configuration files.
    /// </summary>
    internal interface IConfigurationLoader
    {
        /// <summary>
        /// Loads the specified application configuration.
        /// </summary>
        /// <param name="name">The name of the application configuration to load.</param>
        /// <returns>The specified application configuration.</returns>
        ApplicationConfiguration LoadApplicationConfiguration(string name);
    }
}
