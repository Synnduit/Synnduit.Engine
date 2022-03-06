using System;
using System.Collections.Generic;
using Synnduit.Events;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Manages mapping data objects for individual entity mappings.
    /// </summary>
    internal interface IMappingDataRepository
    {
        /// <summary>
        /// Creates an <see cref="IInitializable" /> instance to be used to load mappings
        /// from the database into the in-memory cache.
        /// </summary>
        /// <param name="eventDispatcher">
        /// The <see cref="IEventDispatcher" /> instance to use.
        /// </param>
        /// <returns>
        /// The <see cref="IInitializable" /> instance to register with the initializer
        /// (<see cref="IInitializer" />).
        /// </returns>
        IInitializable CreateInitializer(IEventDispatcher eventDispatcher);

        /// <summary>
        /// Gets mapping data for the specified entity's mapping.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the entity's source system.</param>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in the source system.
        /// </param>
        /// <returns>
        /// Mapping data for the specified entity's mapping; if no mapping for the
        /// specified entity exists, null will be returned.
        /// </returns>
        MappingDataObject GetMappingData(
            Guid entityTypeId,
            Guid sourceSystemId,
            EntityIdentifier sourceSystemEntityId);

        /// <summary>
        /// Gets mapping data for all entities of the current type originating in the
        /// current source system.
        /// </summary>
        /// <param name="mappingSet">The requested set.</param>
        /// <returns>
        /// The collection of mapping data objects for all entities of the specified type
        /// originating in the specified source system.
        /// </returns>
        IEnumerable<MappingDataObject> GetMappingData(MappingSet mappingSet);

        /// <summary>
        /// Creates a new mapping for the specified source system entity of the type that's
        /// currently being processed.
        /// </summary>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in its source system.
        /// </param>
        /// <param name="destinationSystemEntityId">
        /// The ID that uniquely identifies the entity in the destination system.
        /// </param>
        /// <param name="origin">The origin of the mapping.</param>
        /// <param name="serializedEntity">The serialized source system entity.</param>
        /// <returns>Mapping data for the mapping created.</returns>
        MappingDataObject CreateMapping(
            EntityIdentifier sourceSystemEntityId,
            EntityIdentifier destinationSystemEntityId,
            MappingOrigin origin,
            Persistence.ISerializedEntity serializedEntity);
    }
}
