namespace Synnduit
{
    /// <summary>
    /// The base class for exceptions thrown when the configurable exception threshold to abort a
    /// segment/the run is reached.
    /// </summary>
    internal abstract class ThresholdReachedException : Exception
    {
        protected ThresholdReachedException(int threshold)
        {
            this.Threshold = threshold;
        }

        /// <summary>
        /// Gets the number of exceptions that triggered the abort.
        /// </summary>
        public int Threshold { get; }
    }
}
