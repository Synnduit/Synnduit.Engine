using System;
using Synnduit.Persistence;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Represents the identity that uniquely identifies a source system entity.
    /// </summary>
    internal class SourceSystemEntityIdentity : ISourceSystemEntityIdentity
    {
        private readonly Guid entityTypeId;

        private readonly Guid sourceSystemId;

        private readonly EntityIdentifier sourceSystemEntityId;

        public SourceSystemEntityIdentity(
            Guid entityTypeId,
            Guid sourceSystemId,
            EntityIdentifier sourceSystemEntityId)
        {
            this.entityTypeId = entityTypeId;
            this.sourceSystemId = sourceSystemId;
            this.sourceSystemEntityId = sourceSystemEntityId;
        }

        /// <summary>
        /// Gets the ID of the entity type.
        /// </summary>
        public Guid EntityTypeId
        {
            get { return this.entityTypeId; }
        }

        /// <summary>
        /// Gets the ID of the source (external) system.
        /// </summary>
        public Guid SourceSystemId
        {
            get { return this.sourceSystemId; }
        }

        /// <summary>
        /// Gets the ID that uniquely identifies the entity in the source system.
        /// </summary>
        public string SourceSystemEntityId
        {
            get { return this.sourceSystemEntityId; }
        }

        public override int GetHashCode()
        {
            return (
                $"{this.entityTypeId:d}_" +
                $"{this.sourceSystemId:d}_" +
                $"{this.sourceSystemEntityId.ToString().ToLower()}"
                ).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            if(obj is SourceSystemEntityIdentity other)
            {
                equals = this.entityTypeId.Equals(other.entityTypeId)
                    && this.sourceSystemId.Equals(other.sourceSystemId)
                    && this.sourceSystemEntityId.Equals(other.sourceSystemEntityId);
            }
            return equals;
        }
    }
}
