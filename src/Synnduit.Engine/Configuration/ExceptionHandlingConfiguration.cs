namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the application's exception handling behavior configuration.
    /// </summary>
    public class ExceptionHandlingConfiguration : IExceptionHandlingConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ExceptionHandlingConfiguration()
        { }

        /// <summary>
        /// Gets the number of exceptions that will trigger a segment abort.
        /// </summary>
        public int? PerSegmentAbortThreshold { get; set; }

        /// <summary>
        /// Gets the number of exceptions that will trigger a run abort.
        /// </summary>
        public int? PerRunAbortThreshold { get; set; }
    }
}
