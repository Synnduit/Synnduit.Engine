using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Events;
using Synnduit.Persistence;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Manages mapping data objects for individual entity mappings.
    /// </summary>
    [Export(typeof(IMappingDataRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class MappingDataRepository : IMappingDataRepository
    {
        private static Guid? mappingsDestinationSystemId;

        private static IDictionary<SourceSystemEntityIdentity, MappingDataObject> mappings;

        static MappingDataRepository()
        {
            mappingsDestinationSystemId = null;
            mappings = null;
        }

        private readonly IContext context;

        private readonly IOperationExecutive operationExecutive;

        private readonly ISafeRepository safeRepository;

        [ImportingConstructor]
        public MappingDataRepository(
            IContext context,
            IOperationExecutive operationExecutive,
            ISafeRepository safeRepository)
        {
            this.context = context;
            this.operationExecutive = operationExecutive;
            this.safeRepository = safeRepository;
        }

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
        public IInitializable CreateInitializer(IEventDispatcher eventDispatcher)
        {
            return new Initializer(this, eventDispatcher);
        }

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
        public MappingDataObject GetMappingData(
            Guid entityTypeId,
            Guid sourceSystemId,
            EntityIdentifier sourceSystemEntityId)
        {
            mappings.TryGetValue(
                new SourceSystemEntityIdentity(
                    entityTypeId,
                    sourceSystemId,
                    sourceSystemEntityId),
                out MappingDataObject mappingData);
            return mappingData;
        }

        /// <summary>
        /// Gets mapping data for all entities of the current type originating in the
        /// current source system.
        /// </summary>
        /// <param name="mappingSet">The requested set.</param>
        /// <returns>
        /// The collection of mapping data objects for all entities of the specified type
        /// originating in the specified source system.
        /// </returns>
        public IEnumerable<MappingDataObject> GetMappingData(MappingSet mappingSet)
        {
            return
                mappings
                .Keys
                .Where(key =>
                    key.EntityTypeId == this.context.EntityType.Id &&
                    key.SourceSystemId == this.context.SourceSystem.Id &&
                    (mappingSet == MappingSet.All ||
                     mappings[key].State == MappingState.Active))
                .Select(key => mappings[key])
                .ToArray();
        }

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
        public MappingDataObject CreateMapping(
            EntityIdentifier sourceSystemEntityId,
            EntityIdentifier destinationSystemEntityId,
            MappingOrigin origin,
            Persistence.ISerializedEntity serializedEntity)
        {
            var mappingDataObject = new MappingDataObject(
                Guid.NewGuid(),
                sourceSystemEntityId,
                destinationSystemEntityId,
                serializedEntity.DataHash,
                origin,
                MappingState.Active);
            this.safeRepository.CreateMapping(
                new Mapping(
                    mappingDataObject.MappingId,
                    this.operationExecutive.CurrentOperation,
                    serializedEntity,
                    this.context.EntityType.Id,
                    this.context.SourceSystem.Id,
                    mappingDataObject.SourceSystemEntityId,
                    mappingDataObject.DestinationSystemEntityId,
                    (int) mappingDataObject.Origin,
                    (int) mappingDataObject.State));
            mappings.Add(
                new SourceSystemEntityIdentity(
                    this.context.EntityType.Id,
                    this.context.SourceSystem.Id,
                    mappingDataObject.SourceSystemEntityId),
                mappingDataObject);
            return mappingDataObject;
        }

        private class Initializer : IInitializable
        {
            private readonly MappingDataRepository parent;

            private readonly IEventDispatcher eventDispatcher;

            public Initializer(
                MappingDataRepository parent,
                IEventDispatcher eventDispatcher)
            {
                this.parent = parent;
                this.eventDispatcher = eventDispatcher;
            }

            public void Initialize(IInitializationContext context)
            {
                // reload all mappings, unless the current destination system matches that
                // of the mappings currently in the repository, in which case no action is
                // required; effectively, this ensures that the cached mappings are reused,
                // as long as the current segment's destination system is the same as that
                // of the previous one
                if(this.parent.context.DestinationSystem.Id != mappingsDestinationSystemId)
                {
                    this.eventDispatcher.MappingsCaching(new MappingsCachingArgs());
                    mappings =
                        this
                        .parent
                        .safeRepository
                        .GetEntityMappings(
                            this.parent.context.DestinationSystem.Id,
                            (int) MappingState.Active,
                            (int) MappingState.Deactivated)
                        .ToDictionary(
                            em => new SourceSystemEntityIdentity(
                                em.EntityTypeId,
                                em.SourceSystemId,
                                em.SourceSystemEntityId),
                            em => new MappingDataObject(
                                em.Id,
                                em.SourceSystemEntityId,
                                em.DestinationSystemEntityId,
                                em.SerializedEntityHash,
                                (MappingOrigin) em.Origin,
                                (MappingState) em.State));
                    mappingsDestinationSystemId = this.parent.context.DestinationSystem.Id;
                    this.eventDispatcher.MappingsCached(
                        new MappingsCachedArgs(mappings.Count));
                }
            }
        }

        private class Mapping : IMapping
        {
            public Mapping(
                Guid id,
                IOperation operation,
                Persistence.ISerializedEntity sourceSystemEntity,
                Guid entityTypeId,
                Guid sourceSystemId,
                string sourceSystemEntityId,
                string destinationSystemEntityId,
                int origin,
                int state)
            {
                this.Id = id;
                this.Operation = operation;
                this.SourceSystemEntity = sourceSystemEntity;
                this.EntityTypeId = entityTypeId;
                this.SourceSystemId = sourceSystemId;
                this.SourceSystemEntityId = sourceSystemEntityId;
                this.DestinationSystemEntityId = destinationSystemEntityId;
                this.Origin = origin;
                this.State = state;
            }

            public Guid Id { get; }

            public IOperation Operation { get; }

            public Persistence.ISerializedEntity SourceSystemEntity { get; }

            public Guid EntityTypeId { get; }

            public Guid SourceSystemId { get; }

            public string SourceSystemEntityId { get; }

            public string DestinationSystemEntityId { get; }

            public int Origin { get; }

            public int State { get; }
        }

        private class MappingsCachingArgs : IMappingsCachingArgs
        { }

        private class MappingsCachedArgs : IMappingsCachedArgs
        {
            private readonly int count;

            public MappingsCachedArgs(int count)
            {
                this.count = count;
            }

            public int Count
            {
                get { return this.count; }
            }
        }
    }
}
