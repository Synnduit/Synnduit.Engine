namespace Synnduit
{
    /// <summary>
    /// The base class for exceptions thrown when count threshold-based aborts are triggered.
    /// </summary>
    internal abstract class CountThresholdAbortException : Exception
    {
        protected CountThresholdAbortException(int threshold)
        {
            this.Threshold = threshold;
        }

        /// <summary>
        /// Gets the number of exceptions that triggered the abort.
        /// </summary>
        public int Threshold { get; }
    }
}
