using System;

namespace Synnduit
{
    /// <summary>
    /// Represents an entity identifier within a source system.
    /// </summary>
    internal class SourceSystemEntityIdentifier
    {
        public SourceSystemEntityIdentifier(
            Guid sourceSystemId, EntityIdentifier entityId)
        {
            this.SourceSystemId = sourceSystemId;
            this.EntityId = entityId;
        }

        /// <summary>
        /// Gets the ID of the source system.
        /// </summary>
        public Guid SourceSystemId { get; }

        /// <summary>
        /// Gets the ID that uniquely identifies the entity within the source system.
        /// </summary>
        public EntityIdentifier EntityId { get; }

        public override int GetHashCode()
        {
            return
                string.Format("{0:d}_{1}", this.SourceSystemId, this.EntityId)
                .GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            if(obj is SourceSystemEntityIdentifier other)
            {
                equals =
                    this.SourceSystemId == other.SourceSystemId &&
                    this.EntityId == other.EntityId;
            }
            return equals;
        }
    }
}
