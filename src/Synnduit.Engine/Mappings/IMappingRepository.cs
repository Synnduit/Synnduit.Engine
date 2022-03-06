using System.Collections.Generic;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Manages mappings between source system entities and their destination system
    /// counterparts.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IMappingRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the specified entity's (of the type represented by this instance) mapping.
        /// </summary>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in its source system.
        /// </param>
        /// <returns>
        /// The specified entity's mapping; if no mapping for the specified entity exists,
        /// null will be returned.
        /// </returns>
        IMapping<TEntity> GetMapping(EntityIdentifier sourceSystemEntityId);

        /// <summary>
        /// Gets mappings for all entities of the current type originating in the current
        /// source system.
        /// </summary>
        /// <param name="mappingSet">The requested set.</param>
        /// <returns>
        /// The collection of mappings for all entities of the current type originating in
        /// the current source system.
        /// </returns>
        IEnumerable<IMapping<TEntity>> GetMappings(MappingSet mappingSet);

        /// <summary>
        /// Creates a new mapping for the specified source system entity.
        /// </summary>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in its source system.
        /// </param>
        /// <param name="destinationSystemEntityId">
        /// The ID that uniquely identifies the entity in the destination system.
        /// </param>
        /// <param name="origin">The origin of the mapping.</param>
        /// <param name="entity">The source system entity.</param>
        /// <returns>The mapping created.</returns>
        IMapping<TEntity> CreateMapping(
            EntityIdentifier sourceSystemEntityId,
            EntityIdentifier destinationSystemEntityId,
            MappingOrigin origin,
            TEntity entity);
    }
}
