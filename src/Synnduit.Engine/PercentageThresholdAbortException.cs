namespace Synnduit
{
    /// <summary>
    /// The base class for exceptions thrown when percentage threshold-based aborts are triggered.
    /// </summary>
    internal abstract class PercentageThresholdAbortException : Exception
    {
        protected PercentageThresholdAbortException(double threshold, double percentage)
        {
            this.Threshold = threshold;
            this.Percentage = percentage;
        }

        /// <summary>
        /// Gets the percentage that will trigger an abort.
        /// </summary>
        public double Threshold { get; }

        /// <summary>
        /// Gets the percentage actually identified.
        /// </summary>
        public double Percentage { get; }
    }
}
