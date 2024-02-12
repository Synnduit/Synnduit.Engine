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
        public int? SegmentAbortThreshold { get; set; }

        /// <summary>
        /// Gets the number of exceptions that will trigger a run abort.
        /// </summary>
        public int? RunAbortThreshold { get; set; }

        /// <summary>
        /// Gets the percentage of orphan mappings that will trigger an abort.
        /// </summary>
        public double? OrphanMappingPercentageAbortThreshold { get; set; }

        /// <summary>
        /// Gets the percentage of entities identified for garbage collection that will trigger an
        /// abort.
        /// </summary>
        public double? GarbageCollectionPercentageAbortThreshold { get; set; }
    }
}
