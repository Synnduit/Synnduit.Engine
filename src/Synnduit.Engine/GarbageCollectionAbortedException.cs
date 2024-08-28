namespace Synnduit
{
    /// <summary>
    /// The exception thrown when a garbage collection segment is aborted because the percentage of
    /// entities identified for garbage collection exceeds the configured threshold.
    /// </summary>
    internal class GarbageCollectionAbortedException : PercentageThresholdAbortException
    {
        public GarbageCollectionAbortedException(
            double threshold, double percentage, bool abortRun)
            : base(threshold, percentage, abortRun)
        { }
    }
}
