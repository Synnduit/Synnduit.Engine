namespace Synnduit.Deduplication
{
    /// <summary>
    /// Represents possible statuses of the process of deduplicating a single entity.
    /// </summary>
    internal enum DeduplicationStatus
    {
        /// <summary>
        /// The entity is not a duplicate of an existing entity in the desination system,
        /// and a new destination system entity instance should be created for it.
        /// </summary>
        NewEntity = 1,

        /// <summary>
        /// The entity is a duplicate of an existing entity in the destination system.
        /// </summary>
        DuplicateFound,

        /// <summary>
        /// The entity is potentially the duplicate of one or more existing entities in the
        /// destination system, and it should be referred for manual inspection.
        /// </summary>
        ManualInspectionRequired
    }
}
