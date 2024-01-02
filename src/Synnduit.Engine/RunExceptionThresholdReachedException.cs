namespace Synnduit
{
    /// <summary>
    /// The exception thrown when the configurable exception threshold to abort the run is reached.
    /// </summary>
    internal class RunExceptionThresholdReachedException : ThresholdReachedException
    {
        public RunExceptionThresholdReachedException(int threshold)
            : base(threshold)
        { }
    }
}
