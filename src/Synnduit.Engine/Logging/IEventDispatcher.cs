namespace Synnduit.Events
{
    /// <summary>
    /// Dispatches events to individual event receivers.
    /// </summary>
    /// <seealso cref="IEventReceiver{TEventArgs}" />
    internal interface IEventDispatcher
    {
        /// <summary>
        /// To be called when the (deduplication) in-memory cache of destination system
        /// entities has been populated.
        /// </summary>
        /// <param name="args">The event data.</param>
        void CachePopulated(ICachePopulatedArgs args);

        /// <summary>
        /// To be called when the (deduplication) in-memory cache of destination system
        /// entities is about to be populated.
        /// </summary>
        /// <param name="args">The event data.</param>
        void CachePopulating(ICachePopulatingArgs args);

        /// <summary>
        /// To be called when the deletion of a destination system entity (identified for
        /// deletion) has been processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void DeletionProcessed(IDeletionProcessedArgs args);

        /// <summary>
        /// To be called when the deletion of a destination system entity (identified for
        /// deletion) is about to be processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void DeletionProcessing(IDeletionProcessingArgs args);

        /// <summary>
        /// To be called when a garbage collection run segment has been initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        void GarbageCollectionInitialized(IGarbageCollectionInitializedArgs args);

        /// <summary>
        /// To be called when a garbage collection run segment is about to be initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        void GarbageCollectionInitializing(IGarbageCollectionInitializingArgs args);

        /// <summary>
        /// To be called when a subsystem has been initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        void Initialized(IInitializedArgs args);

        /// <summary>
        /// To be called when a subsystem is about to be initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        void Initializing(IInitializingArgs args);

        /// <summary>
        /// To be called when entities from a source system have been loaded.
        /// </summary>
        /// <param name="args">The event data.</param>
        void Loaded(ILoadedArgs args);

        /// <summary>
        /// To be called when entities from a source system feed are about to be loaded.
        /// </summary>
        /// <param name="args">The event data.</param>
        void Loading(ILoadingArgs args);

        /// <summary>
        /// To be called when source/destination system identifier mappings have been
        /// loaded from the database into the in-memory cache.
        /// </summary>
        /// <param name="args">The event data.</param>
        void MappingsCached(IMappingsCachedArgs args);

        /// <summary>
        /// To be called when source/destination system identifier mappings are about to be
        /// loaded from the database into the in-memory cache.
        /// </summary>
        /// <param name="args">The event data.</param>
        void MappingsCaching(IMappingsCachingArgs args);

        /// <summary>
        /// To be called when an orphan identifier mapping has been processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void OrphanMappingProcessed(IOrphanMappingProcessedArgs args);

        /// <summary>
        /// To be called when orphan identifier mappings are about to be processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void OrphanMappingsProcessing(IOrphanMappingsProcessingArgs args);

        /// <summary>
        /// To be called when a run segment has finished executing.
        /// </summary>
        /// <param name="args">The event data.</param>
        void SegmentExecuted(ISegmentExecutedArgs args);

        /// <summary>
        /// To be called when a run segment is about to be executed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void SegmentExecuting(ISegmentExecutingArgs args);
    }

    /// <summary>
    /// Dispatches events to individual event receivers.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    /// <seealso cref="IEventReceiver{TEventArgs}" />
    internal interface IEventDispatcher<TEntity> : IEventDispatcher
        where TEntity : class
    {
        /// <summary>
        /// To be called when a destination system entity identified for deletion has been
        /// loaded from the destination system.
        /// </summary>
        /// <param name="args">The event data.</param>
        void DeletionEntityLoaded(IDeletionEntityLoadedArgs<TEntity> args);

        /// <summary>
        /// To be called when a source system entity has been processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void Processed(IProcessedArgs<TEntity> args);

        /// <summary>
        /// To be called when a source system entity is about to be processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        void Processing(IProcessingArgs<TEntity> args);
    }
}
