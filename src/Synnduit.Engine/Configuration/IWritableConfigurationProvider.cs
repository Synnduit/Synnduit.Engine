namespace Synnduit.Configuration
{
    /// <summary>
    /// Provides read-write access to application configuration.
    /// </summary>
    internal interface IWritableConfigurationProvider : IConfigurationProvider
    {
        /// <summary>
        /// Sets the underlying <see cref="IConfigurationProvider" /> implementation instance; the
        /// configuration data will be retrieved from this instance.
        /// </summary>
        /// <param name="configurationProvider">
        /// The underlying <see cref="IConfigurationProvider" /> implementation instance.
        /// </param>
        void SetConfigurationProvider(IConfigurationProvider configurationProvider);
    }
}
