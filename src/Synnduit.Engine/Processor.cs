using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Deduplication;
using Synnduit.Events;
using Synnduit.Mappings;
using Synnduit.Merge;
using Synnduit.Preprocessing;
using Synnduit.Properties;
using Synnduit.Serialization;

namespace Synnduit
{
    /// <summary>
    /// Processes individual source system entities.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IProcessor<>))]
    internal class Processor<TEntity> : IProcessor<TEntity>
        where TEntity : class
    {
        private const string SourceSystemEntityDataKey = "SourceSystemEntity";

        private const string SourceSystemEntityIdDataKey = "SourceSystemEntityId";

        private const string DestinationSystemEntityDataKey = "DestinationSystemEntity";

        private const string MappingDataKey = "Mapping";

        private const string
            SerializedSourceSystemEntityDataKey = "SerializedSourceSystemEntity";

        private const string ValueChangesDataKey = "ValueChanges";

        private readonly IContext context;

        private readonly IOperationExecutive operationExecutive;

        private readonly ISafeMetadataProvider<TEntity> safeMetadataProvider;

        private readonly IServiceProvider<TEntity> serviceProvider;

        private readonly IMappingRepository<TEntity> mappingRepository;

        private readonly IPreprocessor<TEntity> preprocessor;

        private readonly IDeduplicator<TEntity> deduplicator;

        private readonly IHashingSerializer<TEntity> hashingSerializer;

        private readonly IGateway<TEntity> gateway;

        private readonly IEventHub<TEntity> eventHub;

        [ImportingConstructor]
        public Processor(
            IContext context,
            IOperationExecutive operationExecutive,
            ISafeMetadataProvider<TEntity> safeMetadataProvider,
            IServiceProvider<TEntity> serviceProvider,
            IMappingRepository<TEntity> mappingRepository,
            IPreprocessor<TEntity> preprocessor,
            IDeduplicator<TEntity> deduplicator,
            IHashingSerializer<TEntity> hashingSerializer,
            IGateway<TEntity> gateway,
            IEventHub<TEntity> eventHub)
        {
            this.context = context;
            this.operationExecutive = operationExecutive;
            this.safeMetadataProvider = safeMetadataProvider;
            this.serviceProvider = serviceProvider;
            this.mappingRepository = mappingRepository;
            this.preprocessor = preprocessor;
            this.deduplicator = deduplicator;
            this.hashingSerializer = hashingSerializer;
            this.gateway = gateway;
            this.eventHub = eventHub;
        }

        /// <summary>
        /// Processes the specified source system entity.
        /// </summary>
        /// <param name="entity">The entity to process.</param>
        /// <returns>The outcome of the processing of the specified entity.</returns>
        public IProcessedArgs<TEntity> Process(TEntity entity)
        {
            EntityTransactionOutcome outcome;
            this.SourceSystemEntity = entity;
            Exception exceptionThrown = null;
            try
            {
                outcome = this.Process();
            }
            catch(DestinationSystemException exception)
            {
                outcome = EntityTransactionOutcome.ExceptionThrown;
                exceptionThrown = exception.InnerException;
            }
            return new ProcessedArgs(
                this.operationExecutive.CurrentOperation.TimeStamp,
                this.operationExecutive.CurrentOperation.LogMessages,
                exceptionThrown,
                this.SourceSystemEntity,
                this.SourceSystemEntityId,
                this.DestinationSystemEntity,
                this.Mapping?.DestinationSystemEntityId,
                outcome,
                SerializedEntity.Create(this.SerializedSourceSystemEntity),
                this.ValueChanges);
        }

        private EntityTransactionOutcome Process()
        {
            EntityTransactionOutcome outcome;
            this.SourceSystemEntityId = this.GetSourceSystemEntityId();
            this.Mapping = this.mappingRepository.GetMapping(this.SourceSystemEntityId);
            if(this.ShouldProcess())
            {
                PreprocessedEntity<TEntity> preprocessedEntity
                    = this.preprocessor.Preprocess(
                        this.SourceSystemEntity,
                        EntityOrigin.SourceSystem,
                        this.MappingExists);
                this.SourceSystemEntity = preprocessedEntity.Entity;
                if(!preprocessedEntity.IsRejected)
                {
                    if(this.MappingExists)
                    {
                        outcome = this.ProcessEntityWithMapping();
                    }
                    else
                    {
                        outcome = this.ProcessEntityWithoutMapping();
                    }
                }
                else
                {
                    outcome = EntityTransactionOutcome.Rejected;
                }
            }
            else
            {
                outcome = EntityTransactionOutcome.Skipped;
            }
            return outcome;
        }

        private EntityIdentifier GetSourceSystemEntityId()
        {
            EntityIdentifier sourceSystemEntityId =
                this
                .safeMetadataProvider
                .GetSourceSystemEntityId(this.SourceSystemEntity);
            if(sourceSystemEntityId == null)
            {
                throw new InvalidOperationException(
                    Resources.EntityHasNoSourceSystemEntityId);
            }
            return sourceSystemEntityId;
        }

        private bool MappingExists
        {
            get { return this.Mapping != null; }
        }

        private bool ShouldProcess()
        {
            // an entity should be processed if the entity type is mutable (always), or
            // if no mapping for the entity exists (even if it's NOT mutable, as it's a
            // new instance)
            return this.context.EntityType.IsMutable || !this.MappingExists;
        }

        private EntityTransactionOutcome ProcessEntityWithMapping()
        {
            EntityTransactionOutcome outcome;
            this.SerializedSourceSystemEntity =
                this.hashingSerializer.Serialize(this.SourceSystemEntity);
            if(this.Mapping.SerializedEntityHash !=
                this.SerializedSourceSystemEntity.DataHash ||
                this.Mapping.State != MappingState.Active)
            {
                outcome = this.ProcessModifiedEntity();
            }
            else
            {
                outcome = EntityTransactionOutcome.NoChangesDetected;
            }
            return outcome;
        }

        private EntityTransactionOutcome ProcessEntityWithoutMapping()
        {
            EntityTransactionOutcome outcome;
            if(this.context.EntityType.IsDuplicable)
            {
                DeduplicationResult deduplicationResult =
                    this.deduplicator.Deduplicate(this.SourceSystemEntity);
                if(deduplicationResult.Status == DeduplicationStatus.DuplicateFound)
                {
                    outcome = this.ProcessEntityWithDuplicate(
                        deduplicationResult.DuplicateId);
                }
                else if(deduplicationResult.Status == DeduplicationStatus.NewEntity)
                {
                    outcome = this.ProcessNewEntity();
                }
                else    // == DeduplicationStatus.CandidateDuplicatesFound
                {
                    // TODO: Replace with an actual implementation
                    outcome = EntityTransactionOutcome.ReferredForManualDeduplication;
                }
            }
            else
            {
                outcome = this.ProcessNewEntity();
            }
            return outcome;
        }

        private EntityTransactionOutcome ProcessModifiedEntity()
        {
            EntityTransactionOutcome outcome;
            this.DestinationSystemEntity =
                this.gateway.Get(this.Mapping.DestinationSystemEntityId);
            if(this.DestinationSystemEntity != null)
            {
                TEntity previousVersion = this.Mapping.LoadEntity();
                this.ValueChanges = this.Merge(previousVersion);
                if(this.ValueChanges.Length > 0 ||
                    this.Mapping.State != MappingState.Active)
                {
                    this.UpdateDestinationSystemEntity();
                    if(this.Mapping.State != MappingState.Active)
                    {
                        this.Mapping.SetState(MappingState.Active);
                    }
                    outcome = EntityTransactionOutcome.ChangesDetectedAndMerged;
                }
                else
                {
                    outcome = EntityTransactionOutcome.NoChangesMerged;
                }
            }
            else
            {
                outcome = EntityTransactionOutcome.NotFoundInDestinationSystem;
            }
            this.Mapping.UpdateEntity(this.SourceSystemEntity);
            return outcome;
        }

        private EntityTransactionOutcome
            ProcessEntityWithDuplicate(EntityIdentifier duplicateId)
        {
            EntityTransactionOutcome outcome;
            this.DestinationSystemEntity =
                this.gateway.Get(duplicateId);
            if(this.DestinationSystemEntity != null)
            {
                this.ValueChanges = this.Merge();
                if(this.ValueChanges.Length > 0)
                {
                    this.UpdateDestinationSystemEntity();
                    outcome = EntityTransactionOutcome.DuplicateDetectedChangesMerged;
                }
                else
                {
                    outcome = EntityTransactionOutcome.DuplicateDetectedNoChangesMerged;
                }
                this.Mapping = this.mappingRepository.CreateMapping(
                    this.SourceSystemEntityId,
                    duplicateId,
                    MappingOrigin.Deduplication,
                    this.SourceSystemEntity);
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    Resources.DuplicateOfNonexistentEntity,
                    duplicateId));
            }
            return outcome;
        }

        private EntityTransactionOutcome ProcessNewEntity()
        {
            this.DestinationSystemEntity = this.gateway.New();
            this.ValueChanges = this.Merge();
            EntityIdentifier
                destinationSystemEntityId = this.CreateDestinationSystemEntity();
            this.Mapping = this.mappingRepository.CreateMapping(
                this.SourceSystemEntityId,
                destinationSystemEntityId,
                MappingOrigin.NewEntity,
                this.SourceSystemEntity);
            return EntityTransactionOutcome.NewEntityCreated;
        }

        private ValueChange[] Merge(TEntity previous = null)
        {
            var mergerEntity = new MergerEntity(
                this.DestinationSystemEntity, previous, this.SourceSystemEntity);
            IEnumerable<ValueChange> valueChanges =
                this.serviceProvider.Merger.Merge(mergerEntity);
            if(valueChanges == null)
            {
                throw new InvalidOperationException(Resources.MergerReturnedNull);
            }
            if(mergerEntity.Trunk == null)
            {
                throw new InvalidOperationException(Resources.MergerSetTrunkToNull);
            }
            this.DestinationSystemEntity = mergerEntity.Trunk;
            return valueChanges.ToArray();
        }

        private EntityIdentifier CreateDestinationSystemEntity()
        {
            this.DestinationSystemEntity =
                this.gateway.Create(this.DestinationSystemEntity);
            this.eventHub.EntityCreatedOrUpdated(this.DestinationSystemEntity);
            return
                this
                .safeMetadataProvider
                .GetDestinationSystemEntityId(this.DestinationSystemEntity);
        }

        private void UpdateDestinationSystemEntity()
        {
            this.gateway.Update(this.DestinationSystemEntity);
            this.eventHub.EntityCreatedOrUpdated(this.DestinationSystemEntity);
        }

        private TEntity SourceSystemEntity
        {
            get
            {
                return this.GetDataItem(SourceSystemEntityDataKey) as TEntity;
            }
            set
            {
                this.operationExecutive
                    .CurrentOperation
                    .Data[SourceSystemEntityDataKey] = value;
            }
        }

        private EntityIdentifier SourceSystemEntityId
        {
            get
            {
                return this.GetDataItem(SourceSystemEntityIdDataKey) as EntityIdentifier;
            }
            set
            {
                this.operationExecutive
                    .CurrentOperation
                    .Data[SourceSystemEntityIdDataKey] = value;
            }
        }

        private TEntity DestinationSystemEntity
        {
            get
            {
                return this.GetDataItem(DestinationSystemEntityDataKey) as TEntity;
            }
            set
            {
                this.operationExecutive
                    .CurrentOperation
                    .Data[DestinationSystemEntityDataKey] = value;
            }
        }

        private IMapping<TEntity> Mapping
        {
            get
            {
                return this.GetDataItem(MappingDataKey) as IMapping<TEntity>;
            }
            set
            {
                this.operationExecutive
                    .CurrentOperation
                    .Data[MappingDataKey] = value;
            }
        }

        private Persistence.ISerializedEntity SerializedSourceSystemEntity
        {
            get
            {
                return
                    this.GetDataItem(SerializedSourceSystemEntityDataKey)
                    as Persistence.ISerializedEntity;
            }
            set
            {
                this.operationExecutive
                    .CurrentOperation
                    .Data[SerializedSourceSystemEntityDataKey] = value;
            }
        }

        public ValueChange[] ValueChanges
        {
            get
            {
                return this.GetDataItem(ValueChangesDataKey) as ValueChange[];
            }
            set
            {
                this.operationExecutive
                    .CurrentOperation
                    .Data[ValueChangesDataKey] = value;
            }
        }

        private object GetDataItem(string key)
        {
            this.operationExecutive
                .CurrentOperation
                .Data
                .TryGetValue(key, out object dataItem);
            return dataItem;
        }

        private class ProcessedArgs : OperationCompletedArgs, IProcessedArgs<TEntity>
        {
            public ProcessedArgs(
                DateTimeOffset timeStamp,
                IEnumerable<ILogMessage> logMessages,
                Exception exception,
                TEntity sourceSystemEntity,
                EntityIdentifier sourceSystemEntityId,
                TEntity destinationSystemEntity,
                EntityIdentifier destinationSystemEntityId,
                EntityTransactionOutcome outcome,
                ISerializedEntity serializedSourceSystemEntity,
                IEnumerable<IValueChange> valueChanges)
                : base(timeStamp, logMessages, exception)
            {
                this.SourceSystemEntity = sourceSystemEntity;
                this.SourceSystemEntityId = sourceSystemEntityId;
                this.DestinationSystemEntity = destinationSystemEntity;
                this.DestinationSystemEntityId = destinationSystemEntityId;
                this.Outcome = outcome;
                this.SerializedSourceSystemEntity = serializedSourceSystemEntity;
                if(valueChanges == null || exception != null)
                {
                    this.ValueChanges = Enumerable.Empty<IValueChange>();
                }
                else
                {
                    this.ValueChanges = valueChanges;
                }
            }

            public TEntity SourceSystemEntity { get; }

            public EntityIdentifier SourceSystemEntityId { get; }

            public TEntity DestinationSystemEntity { get; }

            public EntityIdentifier DestinationSystemEntityId { get; }

            public EntityTransactionOutcome Outcome { get; }

            public ISerializedEntity SerializedSourceSystemEntity { get; }

            public IEnumerable<IValueChange> ValueChanges { get; }
        }

        private class MergerEntity : IMergerEntity<TEntity>
        {
            private readonly TEntity previous;

            private readonly TEntity current;

            public MergerEntity(TEntity trunk, TEntity previous, TEntity current)
            {
                this.Trunk = trunk;
                this.previous = previous;
                this.current = current;
            }

            public TEntity Trunk { get; set; }

            public TEntity Previous
            {
                get { return this.previous; }
            }

            public TEntity Current
            {
                get { return this.current; }
            }
        }

        private class SerializedEntity : ISerializedEntity
        {
            public static SerializedEntity Create(
                Persistence.ISerializedEntity serializedEntity)
            {
                return serializedEntity != null
                    ? new SerializedEntity(serializedEntity)
                    : null;
            }

            private readonly Persistence.ISerializedEntity serializedEntity;

            private SerializedEntity(Persistence.ISerializedEntity serializedEntity)
            {
                this.serializedEntity = serializedEntity;
            }

            public string DataHash
            {
                get { return this.serializedEntity.DataHash; }
            }

            public byte[] Data
            {
                get { return this.serializedEntity.Data; }
            }

            public string Label
            {
                get { return this.serializedEntity.Label; }
            }
        }
    }
}
