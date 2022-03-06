namespace Synnduit.Configuration
{
    /// <summary>
    /// The base class for classes representing the application/run/segment configuration.
    /// </summary>
    public abstract class ConfigurationBase : IConfigurationBase
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected ConfigurationBase()
        { }

        /// <summary>
        /// Gets or sets the name of the source system.
        /// </summary>
        public string SourceSystem { get; set; }
    }
}
