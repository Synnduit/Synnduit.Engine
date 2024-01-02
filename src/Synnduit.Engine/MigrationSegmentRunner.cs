using Synnduit.Events;
using Synnduit.Mappings;
using Synnduit.Properties;
using System.ComponentModel.Composition;

namespace Synnduit
{
    /// <summary>
    /// Loads entities from the source system feed and processes them.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IMigrationSegmentRunner<>))]
    internal class MigrationSegmentRunner<TEntity> : IMigrationSegmentRunner<TEntity>
        where TEntity : class
    {
        private readonly IOperationExecutive operationExecutive;

        private readonly IServiceProvider<TEntity> serviceProvider;

        private readonly ISafeMetadataProvider<TEntity> safeMetadataProvider;

        private readonly IParameterProvider parameterProvider;

        private readonly IMappingRepository<TEntity> mappingRepository;

        private readonly IProcessor<TEntity> processor;

        private readonly IEventDispatcher<TEntity> eventDispatcher;

        private readonly IExceptionHandler exceptionHandler;

        [ImportingConstructor]
        public MigrationSegmentRunner(
            IOperationExecutive operationExecutive,
            IServiceProvider<TEntity> serviceProvider,
            ISafeMetadataProvider<TEntity> safeMetadataProvider,
            IParameterProvider parameterProvider,
            IMappingRepository<TEntity> mappingRepository,
            IProcessor<TEntity> processor,
            IEventDispatcher<TEntity> eventDispatcher,
            IExceptionHandler exceptionHandler)
        {
            this.operationExecutive = operationExecutive;
            this.serviceProvider = serviceProvider;
            this.safeMetadataProvider = safeMetadataProvider;
            this.parameterProvider = parameterProvider;
            this.mappingRepository = mappingRepository;
            this.processor = processor;
            this.eventDispatcher = eventDispatcher;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Performs the current run segment run.
        /// </summary>
        public void Run()
        {
            IEnumerable<TEntity> entities = this.LoadEntities();
            IDictionary<EntityIdentifier, IMapping<TEntity>> mappings = this.GetMappings();
            int segmentExceptionCount = 0;
            foreach(TEntity entity in entities)
            {
                EntityTransactionOutcome outcome;
                using(IOperationScope scope = this.operationExecutive.CreateOperation())
                {
                    if(entity == null)
                    {
                        throw new InvalidOperationException(
                            Resources.FeedReturnedCollectionWithNullElement);
                    }
                    outcome = this.Process(entity);
                    this.RemoveMapping(mappings, entity);
                    scope.Complete();
                }
                this.exceptionHandler.ProcessEntityTransactionOutcome(
                    outcome, ref segmentExceptionCount);
            }
            this.ProcessOrphanMappings(mappings.Values);
        }

        private IEnumerable<TEntity> LoadEntities()
        {
            IEntityCollection<TEntity> entityCollection;
            this.eventDispatcher.Loading(new LoadingArgs());
            entityCollection = this.serviceProvider.Feed.LoadEntities();
            if(entityCollection == null)
            {
                throw new InvalidOperationException(Resources.FeedReturnedNull);
            }
            this.eventDispatcher.Loaded(
                new LoadedArgs(entityCollection.Count));
            return entityCollection.Entities;
        }

        private IDictionary<EntityIdentifier, IMapping<TEntity>> GetMappings()
        {
            IEnumerable<IMapping<TEntity>> mappings = new IMapping<TEntity>[0];
            switch(this.parameterProvider.OrphanMappingBehavior)
            {
                case OrphanMappingBehavior.Deactivate:
                    mappings = this.mappingRepository.GetMappings(MappingSet.ActiveOnly);
                    break;

                case OrphanMappingBehavior.Remove:
                    mappings = this.mappingRepository.GetMappings(MappingSet.All);
                    break;

                default:
                    break;
            }
            return mappings.ToDictionary(mapping => mapping.SourceSystemEntityId);
        }

        private EntityTransactionOutcome Process(TEntity entity)
        {
            this.eventDispatcher.Processing(new ProcessingArgs(
                this.operationExecutive.CurrentOperation.TimeStamp, entity));
            IProcessedArgs<TEntity> processedArgs = this.processor.Process(entity);
            this.eventDispatcher.Processed(processedArgs);
            return processedArgs.Outcome;
        }

        private void RemoveMapping(
            IDictionary<EntityIdentifier, IMapping<TEntity>> mappings,
            TEntity entity)
        {
            EntityIdentifier sourceSystemEntityId =
                this.safeMetadataProvider.GetSourceSystemEntityId(entity);
            mappings.Remove(sourceSystemEntityId);
        }

        private void ProcessOrphanMappings(IEnumerable<IMapping<TEntity>> orphanMappings)
        {
            IMapping<TEntity>[] mappings = orphanMappings.ToArray();
            if(mappings.Length > 0)
            {
                this.eventDispatcher.OrphanMappingsProcessing(
                    new OrphanMappingsProcessingArgs(
                        mappings.Length, this.parameterProvider.OrphanMappingBehavior));
                MappingState targetState =
                    this
                    .parameterProvider
                    .OrphanMappingBehavior == OrphanMappingBehavior.Deactivate
                        ? MappingState.Deactivated
                        : MappingState.Removed;
                foreach(IMapping<TEntity> mapping in mappings)
                {
                    this.SetMappingState(mapping, targetState);
                }
            }
        }

        private void SetMappingState(IMapping<TEntity> mapping, MappingState state)
        {
            using(IOperationScope scope = this.operationExecutive.CreateOperation())
            {
                mapping.SetState(state);
                this.eventDispatcher.OrphanMappingProcessed(
                    new OrphanMappingProcessedArgs(
                        this.operationExecutive.CurrentOperation.TimeStamp));
                scope.Complete();
            }
        }

        private class LoadingArgs : ILoadingArgs
        {
        }

        private class LoadedArgs : ILoadedArgs
        {
            private readonly int count;

            public LoadedArgs(int count)
            {
                this.count = count;
            }

            public int Count
            {
                get { return this.count; }
            }
        }

        private class ProcessingArgs : OperationArgs, IProcessingArgs<TEntity>
        {
            private readonly TEntity entity;

            public ProcessingArgs(DateTimeOffset timeStamp, TEntity entity)
                : base(timeStamp)
            {
                this.entity = entity;
            }

            public TEntity Entity
            {
                get { return this.entity; }
            }
        }

        private class OrphanMappingsProcessingArgs : IOrphanMappingsProcessingArgs
        {
            private readonly int count;

            private readonly OrphanMappingBehavior behavior;

            public OrphanMappingsProcessingArgs(int count, OrphanMappingBehavior behavior)
            {
                this.count = count;
                this.behavior = behavior;
            }

            public int Count
            {
                get { return this.count; }
            }

            public OrphanMappingBehavior Behavior
            {
                get { return this.behavior; }
            }
        }

        private class OrphanMappingProcessedArgs :
            OperationArgs, IOrphanMappingProcessedArgs
        {
            public OrphanMappingProcessedArgs(DateTimeOffset timeStamp)
                : base(timeStamp)
            { }
        }
    }
}
