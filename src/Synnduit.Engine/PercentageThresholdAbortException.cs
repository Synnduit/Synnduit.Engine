namespace Synnduit
{
    /// <summary>
    /// The base class for exceptions thrown when percentage threshold-based aborts are triggered.
    /// </summary>
    internal abstract class PercentageThresholdAbortException : Exception
    {
        protected PercentageThresholdAbortException(
            double threshold, double percentage, bool abortRun)
        {
            this.Threshold = threshold;
            this.Percentage = percentage;
            this.AbortRun = abortRun;
        }

        /// <summary>
        /// Gets the percentage that will trigger an abort.
        /// </summary>
        public double Threshold { get; }

        /// <summary>
        /// Gets the percentage actually identified.
        /// </summary>
        public double Percentage { get; }

        /// <summary>
        /// Gets a value indicating whether the run should be aborted; if false, only the current
        /// segment shall be aborted.
        /// </summary>
        public bool AbortRun { get; }
    }
}
