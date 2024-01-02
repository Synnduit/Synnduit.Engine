namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the application's configuration.
    /// </summary>
    public class ApplicationConfiguration : ConfigurationBase, IApplicationConfiguration
    {
        /// <summary>
        /// The default name of the connection string to be used by the application for its
        /// database.
        /// </summary>
        public const string DefaultConnectionStringName = "Synnduit";

        private string connectionStringName = null;

        private LoggingConfiguration loggingConfiguration = null;

        private ExceptionHandlingConfiguration exceptionHandlingConfiguration = null;

        private RunConfiguration[] runs = null;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ApplicationConfiguration()
        { }

        /// <summary>
        /// Gets or sets the name of the connection string to be used by the application for its
        /// database.
        /// </summary>
        public string ConnectionStringName
        {
            get => this.connectionStringName ?? DefaultConnectionStringName;
            set => this.connectionStringName = value;
        }

        /// <summary>
        /// Gets or sets the binary files directory path; the exports from the DLL files
        /// in this directory shall be loaded.
        /// </summary>
        public string BinaryFilesDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the application's logging behavior configuration.
        /// </summary>
        public LoggingConfiguration Logging
        {
            get => this.loggingConfiguration ?? new LoggingConfiguration();
            set => this.loggingConfiguration = value;
        }

        /// <summary>
        /// Gets the application's exception handling behavior configuration.
        /// </summary>
        public ExceptionHandlingConfiguration ExceptionHandling
        {
            get => this.exceptionHandlingConfiguration ?? new ExceptionHandlingConfiguration();
            set => this.exceptionHandlingConfiguration = value;
        }

        /// <summary>
        /// Gets or sets the array of supported runs.
        /// </summary>
        public RunConfiguration[] Runs
        {
            get => this.runs.SafeToArray();
            set => this.runs = value;
        }

        ILoggingConfiguration IApplicationConfiguration.Logging => this.Logging;

        IExceptionHandlingConfiguration IApplicationConfiguration.ExceptionHandling =>
            this.ExceptionHandling;

        IEnumerable<IRunConfiguration> IApplicationConfiguration.Runs => this.Runs;
    }
}
