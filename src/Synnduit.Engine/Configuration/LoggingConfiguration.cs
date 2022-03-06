namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the application's logging behavior configuration.
    /// </summary>
    public class LoggingConfiguration : ILoggingConfiguration
    {
        private MigrationLoggingConfiguration migration = null;

        private GarbageCollectionLoggingConfiguration garbageCollection = null;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public LoggingConfiguration()
        { }

        /// <summary>
        /// Gets or sets the application's migration logging behavior configuration.
        /// </summary>
        public MigrationLoggingConfiguration Migration
        {
            get => this.migration ?? new MigrationLoggingConfiguration();
            set => this.migration = value;
        }

        /// <summary>
        /// Gets or sets the application's garbage collection logging behavior configuration.
        /// </summary>
        public GarbageCollectionLoggingConfiguration GarbageCollection
        {
            get => this.garbageCollection ?? new GarbageCollectionLoggingConfiguration();
            set => this.garbageCollection = value;
        }

        IMigrationLoggingConfiguration ILoggingConfiguration.Migration => this.Migration;

        IGarbageCollectionLoggingConfiguration ILoggingConfiguration.GarbageCollection =>
            this.GarbageCollection;
    }
}
