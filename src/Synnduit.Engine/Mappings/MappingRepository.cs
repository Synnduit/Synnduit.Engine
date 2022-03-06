using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Events;
using Synnduit.Persistence;
using Synnduit.Serialization;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Manages mappings between source system entities and their destination system
    /// counterparts.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IMappingRepository<>))]
    internal class MappingRepository<TEntity> : IMappingRepository<TEntity>
        where TEntity : class
    {
        private readonly IContext context;

        private readonly IOperationExecutive operationExecutive;

        private readonly IMappingDataRepository mappingDataRepository;

        private readonly ISafeRepository safeRepository;

        private readonly IHashingSerializer<TEntity> hashingSerializer;

        [ImportingConstructor]
        public MappingRepository(
            IContext context,
            IOperationExecutive operationExecutive,
            IMappingDataRepository mappingDataRepository,
            ISafeRepository safeRepository,
            IHashingSerializer<TEntity> hashingSerializer,
            IEventDispatcher<TEntity> eventDispatcher,
            IInitializer initializer)
        {
            this.context = context;
            this.operationExecutive = operationExecutive;
            this.mappingDataRepository = mappingDataRepository;
            this.safeRepository = safeRepository;
            this.hashingSerializer = hashingSerializer;
            initializer.Register(
                this.mappingDataRepository.CreateInitializer(eventDispatcher),
                suppressEvents: true);
        }

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
        public IMapping<TEntity> GetMapping(EntityIdentifier sourceSystemEntityId)
        {
            IMapping<TEntity> mapping = null;
            MappingDataObject mappingData =
                this.mappingDataRepository.GetMappingData(
                    this.context.EntityType.Id,
                    this.context.SourceSystem.Id,
                    sourceSystemEntityId);
            if(mappingData != null)
            {
                mapping = new Mapping(this, mappingData);
            }
            return mapping;
        }

        /// <summary>
        /// Gets mappings for all entities of the current type originating in the current
        /// source system.
        /// </summary>
        /// <param name="mappingSet">The requested set.</param>
        /// <returns>
        /// The collection of mappings for all entities of the current type originating in
        /// the current source system.
        /// </returns>
        public IEnumerable<IMapping<TEntity>> GetMappings(MappingSet mappingSet)
        {
            return
                this
                .mappingDataRepository
                .GetMappingData(mappingSet)
                .Select(mappingData => new Mapping(this, mappingData))
                .ToArray();
        }

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
        public IMapping<TEntity> CreateMapping(
            EntityIdentifier sourceSystemEntityId,
            EntityIdentifier destinationSystemEntityId,
            MappingOrigin origin,
            TEntity entity)
        {
            Persistence.ISerializedEntity serializedEntity =
                this.hashingSerializer.Serialize(entity);
            MappingDataObject mappingDataObject =
                this.mappingDataRepository.CreateMapping(
                    sourceSystemEntityId,
                    destinationSystemEntityId,
                    origin,
                    serializedEntity);
            this.operationExecutive
                .CurrentOperation
                .UpdateIdentityCorrelationId(sourceSystemEntityId);
            return new Mapping(this, mappingDataObject);
        }

        private class Mapping : IMapping<TEntity>
        {
            private readonly MappingRepository<TEntity> parent;

            private readonly MappingDataObject mappingDataObject;

            public Mapping(
                MappingRepository<TEntity> parent,
                MappingDataObject mappingDataObject)
            {
                this.parent = parent;
                this.mappingDataObject = mappingDataObject;
            }

            public Guid Id
            {
                get { return this.mappingDataObject.MappingId; }
            }

            public EntityIdentifier SourceSystemEntityId
            {
                get { return this.mappingDataObject.SourceSystemEntityId; }
            }

            public EntityIdentifier DestinationSystemEntityId
            {
                get { return this.mappingDataObject.DestinationSystemEntityId; }
            }

            public string SerializedEntityHash
            {
                get { return this.mappingDataObject.SerializedEntityHash; }
            }

            public MappingOrigin Origin
            {
                get { return this.mappingDataObject.Origin; }
            }

            public MappingState State
            {
                get { return this.mappingDataObject.State; }
            }

            public TEntity LoadEntity()
            {
                byte[] entityData =
                    this.parent.safeRepository.GetMappingEntity(
                        this.mappingDataObject.MappingId);
                return this.parent.hashingSerializer.Deserialize(entityData);
            }

            public void UpdateEntity(TEntity entity)
            {
                Persistence.ISerializedEntity serializedEntity =
                    this.parent.hashingSerializer.Serialize(entity);
                this.parent.safeRepository.UpdateMappingEntity(
                    this.mappingDataObject.MappingId,
                    this.parent.operationExecutive.CurrentOperation,
                    serializedEntity);
                this.mappingDataObject.SerializedEntityHash = serializedEntity.DataHash;
                this.parent
                    .operationExecutive
                    .CurrentOperation
                    .UpdateIdentityCorrelationId(
                        this.mappingDataObject.SourceSystemEntityId);
            }

            public void SetState(MappingState state)
            {
                this.parent.safeRepository.SetMappingState(
                    this.mappingDataObject.MappingId,
                    this.parent.operationExecutive.CurrentOperation,
                    (int) state);
                this.mappingDataObject.State = state;
            }
        }
    }
}
