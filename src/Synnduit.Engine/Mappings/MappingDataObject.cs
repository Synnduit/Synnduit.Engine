using System;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Encapsulates information related to the mapping of a single source system entity
    /// to its destination system counterpart.
    /// </summary>
    internal class MappingDataObject
    {
        public MappingDataObject(
            Guid mappingId,
            EntityIdentifier sourceSystemEntityId,
            EntityIdentifier destinationSystemEntityId,
            string serializedEntityHash,
            MappingOrigin origin,
            MappingState state)
        {
            this.MappingId = mappingId;
            this.SourceSystemEntityId = sourceSystemEntityId;
            this.DestinationSystemEntityId = destinationSystemEntityId;
            this.SerializedEntityHash = serializedEntityHash;
            this.Origin = origin;
            this.State = state;
        }

        /// <summary>
        /// Gets the ID of the mapping.
        /// </summary>
        public Guid MappingId { get; }

        /// <summary>
        /// Gets the ID that uniquely identifies the entity in the source system.
        /// </summary>
        public EntityIdentifier SourceSystemEntityId { get; }

        /// <summary>
        /// Gets the ID that uniquely identifies the entity in the destination system.
        /// </summary>
        public EntityIdentifier DestinationSystemEntityId { get; }

        /// <summary>
        /// Gets or sets the hash of the serialized entity data.
        /// </summary>
        public string SerializedEntityHash { get; set; }

        /// <summary>
        /// Gets the origin of the mapping.
        /// </summary>
        public MappingOrigin Origin { get; }

        /// <summary>
        /// Gets or sets the state of the mapping.
        /// </summary>
        public MappingState State { get; set; }
    }
}
