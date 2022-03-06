using System.Collections.Generic;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// Encapsulates the result of the process of deduplicating a single entity.
    /// </summary>
    internal class DeduplicationResult
    {
        public DeduplicationResult(DeduplicationStatus status)
            : this(status, null, null)
        { }

        public DeduplicationResult(
            DeduplicationStatus status,
            EntityIdentifier duplicateId)
            : this(status, duplicateId, null)
        { }

        public DeduplicationResult(
            DeduplicationStatus status,
            IEnumerable<Duplicate> candidateDuplicates)
            : this(status, null, candidateDuplicates)
        { }

        private DeduplicationResult(
            DeduplicationStatus status,
            EntityIdentifier duplicateId,
            IEnumerable<Duplicate> candidateDuplicates)
        {
            this.Status = status;
            this.DuplicateId = duplicateId;
            this.CandidateDuplicates = candidateDuplicates;
        }

        /// <summary>
        /// Gets the status of the deduplication process.
        /// </summary>
        public DeduplicationStatus Status { get; }

        /// <summary>
        /// Gets the destination system ID of the entity that the deduplicated entity is
        /// a duplicate of; this value will only be set if Status is DuplicateFound.
        /// </summary>
        public EntityIdentifier DuplicateId { get; }

        /// <summary>
        /// Gets the collection of destination system entities that the deduplicated entity
        /// might be a duplicate of; these entities should be referred for manual
        /// inspection; this value will only be set if Status is CandidateDuplicatesFound.
        /// </summary>
        public IEnumerable<Duplicate> CandidateDuplicates { get; }
    }
}
