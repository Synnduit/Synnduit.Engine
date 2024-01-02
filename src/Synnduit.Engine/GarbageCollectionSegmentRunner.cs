using Synnduit.Events;
using Synnduit.Mappings;
using Synnduit.Persistence;
using System.ComponentModel.Composition;

namespace Synnduit
{
    /// <summary>
    /// Runs individual run segments of the <see cref="SegmentType.GarbageCollection" />
    /// type.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IGarbageCollectionSegmentRunner<>))]
    internal class GarbageCollectionSegmentRunner<TEntity> :
        IGarbageCollectionSegmentRunner<TEntity>
        where TEntity : class
    {
        private readonly IContext context;

        private readonly IParameterProvider parameterProvider;

        private readonly IOperationExecutive operationExecutive;

        private readonly IGateway<TEntity> gateway;

        private readonly ISafeRepository safeRepository;

        private readonly IEventDispatcher<TEntity> eventDispatcher;

        private readonly IExceptionHandler exceptionHandler;

        private EntityIdentifier[] idsOfEntitiesToDelete;

        [ImportingConstructor]
        public GarbageCollectionSegmentRunner(
            IContext context,
            IParameterProvider parameterProvider,
            IOperationExecutive operationExecutive,
            IGateway<TEntity> gateway,
            ISafeRepository safeRepository,
            IEventDispatcher<TEntity> eventDispatcher,
            IExceptionHandler exceptionHandler,
            IInitializer initializer)
        {
            this.context = context;
            this.parameterProvider = parameterProvider;
            this.operationExecutive = operationExecutive;
            this.gateway = gateway;
            this.safeRepository = safeRepository;
            this.eventDispatcher = eventDispatcher;
            this.exceptionHandler = exceptionHandler;
            this.idsOfEntitiesToDelete = null;
            initializer.Register(
                new Initializer(this),
                suppressEvents: true);
        }

        /// <summary>
        /// Performs the current run segment run.
        /// </summary>
        public void Run()
        {
            int segmentExceptionCount = 0;
            foreach(EntityIdentifier id in this.idsOfEntitiesToDelete)
            {
                EntityDeletionOutcome outcome;
                using(IOperationScope scope = this.operationExecutive.CreateOperation())
                {
                    outcome = this.DeleteEntity(id);
                    scope.Complete();
                }
                this.exceptionHandler.ProcessEntityDeletionOutcome(
                    outcome, ref segmentExceptionCount);
            }
        }

        private EntityDeletionOutcome DeleteEntity(EntityIdentifier id)
        {
            EntityDeletionOutcome outcome;
            Exception exceptionThrown = null;
            try
            {
                this.RaiseDeletionProcessing(id);
                TEntity entity = this.gateway.Get(id);
                if(entity != null)
                {
                    this.RaiseDeletionEntityLoaded(id, entity);
                    this.gateway.Delete(entity);
                    outcome = EntityDeletionOutcome.Deleted;
                }
                else
                {
                    outcome = EntityDeletionOutcome.NotFound;
                }
            }
            catch(DestinationSystemException exception)
            {
                outcome = EntityDeletionOutcome.ExceptionThrown;
                exceptionThrown = exception.InnerException;
            }
            this.RaiseDeletionProcessed(id, outcome, exceptionThrown);
            return outcome;
        }

        private void RaiseDeletionProcessing(EntityIdentifier entityId)
        {
            this.eventDispatcher.DeletionProcessing(
                new DeletionProcessingArgs(
                    this.operationExecutive.CurrentOperation.TimeStamp,
                    entityId));
        }

        private void RaiseDeletionEntityLoaded(EntityIdentifier entityId, TEntity entity)
        {
            this.eventDispatcher.DeletionEntityLoaded(
                new DeletionEntityLoadedArgs(
                    this.operationExecutive.CurrentOperation.TimeStamp,
                    entityId,
                    entity));
        }

        private void RaiseDeletionProcessed(
            EntityIdentifier entityId,
            EntityDeletionOutcome outcome,
            Exception exception)
        {
            this.eventDispatcher.DeletionProcessed(
                new DeletionProcessedArgs(
                    this.operationExecutive.CurrentOperation.TimeStamp,
                    this.operationExecutive.CurrentOperation.LogMessages,
                    exception,
                    entityId,
                    outcome));
        }

        private class Initializer : IInitializable
        {
            private readonly GarbageCollectionSegmentRunner<TEntity> parent;

            public Initializer(GarbageCollectionSegmentRunner<TEntity> parent)
            {
                this.parent = parent;
            }

            public void Initialize(IInitializationContext context)
            {
                this.parent
                    .eventDispatcher
                    .GarbageCollectionInitializing(
                        new GarbageCollectionInitializingArgs());
                this.parent.idsOfEntitiesToDelete = this.GetIdsOfEntitiesToDelete();
                this.parent
                    .eventDispatcher
                    .GarbageCollectionInitialized(
                        new GarbageCollectionInitializedArgs(
                            this.parent.idsOfEntitiesToDelete.Length));
            }

            private EntityIdentifier[] GetIdsOfEntitiesToDelete()
            {
                IEnumerable<EntityIdentifier> idsOfEntitiesToDelete;
                IEnumerable<EntityIdentifier>
                    destinationSystemIds = this.parent.gateway.GetEntityIdentifiers();
                MappingEntityIdentifiers mappingEntityIdentifiers
                    = this.GetMappingEntityIdentifiers();
                switch(this.parent.parameterProvider.GarbageCollectionBehavior)
                {
                    case GarbageCollectionBehavior.DeleteCreated:
                        idsOfEntitiesToDelete =
                            destinationSystemIds
                            .Intersect(mappingEntityIdentifiers.InactiveCreated);
                        break;

                    case GarbageCollectionBehavior.DeleteMapped:
                        idsOfEntitiesToDelete =
                            destinationSystemIds
                            .Intersect(mappingEntityIdentifiers.Inactive);
                        break;

                    case GarbageCollectionBehavior.DeleteAll:
                        idsOfEntitiesToDelete =
                            destinationSystemIds
                            .Except(mappingEntityIdentifiers.Active);
                        break;

                    default:
                        idsOfEntitiesToDelete = new EntityIdentifier[0];
                        break;
                }
                return idsOfEntitiesToDelete.ToArray();
            }

            private MappingEntityIdentifiers GetMappingEntityIdentifiers()
            {
                var active = new HashSet<EntityIdentifier>();
                var inactiveCreated = new HashSet<EntityIdentifier>();
                var inactiveNotCreated = new HashSet<EntityIdentifier>();
                foreach(IMappedEntityIdentifier mappedEntityIdentifier in
                    this
                    .parent
                    .safeRepository
                    .GetMappedEntityIdentifiers(this.parent.context.EntityType.Id))
                {
                    var state = (MappingState) mappedEntityIdentifier.State;
                    var origin = (MappingOrigin) mappedEntityIdentifier.Origin;
                    if(state == MappingState.Active)
                    {
                        active.Add(mappedEntityIdentifier.DestinationSystemEntityId);
                    }
                    else if(
                        state == MappingState.Deactivated ||
                        state == MappingState.Removed)
                    {
                        if(origin == MappingOrigin.NewEntity)
                        {
                            inactiveCreated.Add(
                                mappedEntityIdentifier.DestinationSystemEntityId);
                        }
                        else if(origin == MappingOrigin.Deduplication)
                        {
                            inactiveNotCreated.Add(
                                mappedEntityIdentifier.DestinationSystemEntityId);
                        }
                    }
                }
                return new MappingEntityIdentifiers(
                    active,
                    inactiveCreated.Union(inactiveNotCreated).Except(active),
                    inactiveCreated.Except(active));
            }

            private class MappingEntityIdentifiers
            {
                public MappingEntityIdentifiers(
                    IEnumerable<EntityIdentifier> active,
                    IEnumerable<EntityIdentifier> inactive,
                    IEnumerable<EntityIdentifier> inactiveCreated)
                {
                    this.Active = active;
                    this.Inactive = inactive;
                    this.InactiveCreated = inactiveCreated;
                }

                public IEnumerable<EntityIdentifier> Active { get; }

                public IEnumerable<EntityIdentifier> Inactive { get; }

                public IEnumerable<EntityIdentifier> InactiveCreated { get; }
            }
        }

        private class GarbageCollectionInitializingArgs :
            IGarbageCollectionInitializingArgs
        {
        }

        private class GarbageCollectionInitializedArgs : IGarbageCollectionInitializedArgs
        {
            private readonly int count;

            public GarbageCollectionInitializedArgs(int count)
            {
                this.count = count;
            }

            public int Count
            {
                get { return this.count; }
            }
        }

        private abstract class DeletionArgs : OperationArgs, IDeletionArgs
        {
            private readonly EntityIdentifier entityId;

            public DeletionArgs(DateTimeOffset timeStamp, EntityIdentifier entityId)
                : base(timeStamp)
            {
                this.entityId = entityId;
            }

            public EntityIdentifier EntityId
            {
                get { return this.entityId; }
            }
        }

        private class DeletionProcessingArgs : DeletionArgs, IDeletionProcessingArgs
        {
            public DeletionProcessingArgs(
                DateTimeOffset timeStamp, EntityIdentifier entityId)
                : base(timeStamp, entityId)
            { }
        }

        private class DeletionEntityLoadedArgs :
            DeletionArgs, IDeletionEntityLoadedArgs<TEntity>
        {
            private readonly TEntity entity;

            public DeletionEntityLoadedArgs(
                DateTimeOffset timeStamp,
                EntityIdentifier entityId,
                TEntity entity)
                : base(timeStamp, entityId)
            {
                this.entity = entity;
            }

            public TEntity Entity
            {
                get { return this.entity; }
            }
        }

        private class DeletionProcessedArgs :
            OperationCompletedArgs, IDeletionProcessedArgs
        {
            private readonly EntityIdentifier entityId;

            private readonly EntityDeletionOutcome outcome;

            public DeletionProcessedArgs(
                DateTimeOffset timeStamp,
                IEnumerable<ILogMessage> logMessages,
                Exception exception,
                EntityIdentifier entityId,
                EntityDeletionOutcome outcome)
                : base(timeStamp, logMessages, exception)
            {
                this.entityId = entityId;
                this.outcome = outcome;
            }

            public EntityIdentifier EntityId
            {
                get { return this.entityId; }
            }

            public EntityDeletionOutcome Outcome
            {
                get { return this.outcome; }
            }
        }
    }
}
