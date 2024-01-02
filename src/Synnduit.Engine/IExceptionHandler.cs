namespace Synnduit
{
    /// <summary>
    /// Handles entity-processing (migration/garbage collection) exceptions.
    /// </summary>
    internal interface IExceptionHandler
    {
        /// <summary>
        /// Processes the specified entity transaction outcome.
        /// </summary>
        /// <param name="outcome">The entity transaction outcome.</param>
        /// <param name="segmentExceptionCount">
        /// The number of exceptions thrown in the current segment; will be incremented if
        /// <paramref name="outcome"/> is <see cref="EntityTransactionOutcome.ExceptionThrown"/>.
        /// </param>
        void ProcessEntityTransactionOutcome(
            EntityTransactionOutcome outcome, ref int segmentExceptionCount);

        /// <summary>
        /// Processes the specified entity deletion outcome.
        /// </summary>
        /// <param name="outcome">The entity deletion outcome.</param>
        /// <param name="segmentExceptionCount">
        /// The number of exceptions thrown in the current segment; will be incremented if
        /// <paramref name="outcome"/> is <see cref="EntityDeletionOutcome.ExceptionThrown"/>.
        /// </param>
        void ProcessEntityDeletionOutcome(
            EntityDeletionOutcome outcome, ref int segmentExceptionCount);
    }
}
