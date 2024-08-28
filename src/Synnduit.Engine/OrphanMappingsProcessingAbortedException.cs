namespace Synnduit
{
    /// <summary>
    /// The exception thrown when the processing of orphan mappings is aborted because the
    /// percentage of mappings identified for deactivation/removal exceeds the configured
    /// threshold.
    /// </summary>
    internal class OrphanMappingsProcessingAbortedException : PercentageThresholdAbortException
    {
        public OrphanMappingsProcessingAbortedException(
            double threshold, double percentage, bool abortRun)
            : base(threshold, percentage, abortRun)
        { }
    }
}
