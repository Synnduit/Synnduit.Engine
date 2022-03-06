using System;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Represents the mapping of a source system entity to its destination system
    /// counterpart.
    /// </summary>
    internal interface IMapping<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the ID of the mapping.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the ID that uniquely identifies the entity in the source system.
        /// </summary>
        EntityIdentifier SourceSystemEntityId { get; }

        /// <summary>
        /// Gets the ID that uniquely identifies the entity in the destination system.
        /// </summary>
        EntityIdentifier DestinationSystemEntityId { get; }

        /// <summary>
        /// Gets the hash of the serialized source system entity in the database.
        /// </summary>
        string SerializedEntityHash { get; }

        /// <summary>
        /// Gets the origin of the mapping.
        /// </summary>
        MappingOrigin Origin { get; }

        /// <summary>
        /// Gets the state of the mapping.
        /// </summary>
        MappingState State { get; }

        /// <summary>
        /// Loads the most recent known version of the source system entity from the
        /// database.
        /// </summary>
        /// <returns>The most recent known version of the source system entity.</returns>
        TEntity LoadEntity();

        /// <summary>
        /// Saves the specified entity in the database as the new most recent known version
        /// of the source system entity.
        /// </summary>
        /// <param name="entity">
        /// The entity to be saved as the new most recent known version of the source
        /// system entity.
        /// </param>
        void UpdateEntity(TEntity entity);

        /// <summary>
        /// Sets the state of the mapping.
        /// </summary>
        /// <param name="state">The new state.</param>
        void SetState(MappingState state);
    }
}
