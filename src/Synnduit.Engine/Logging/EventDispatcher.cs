using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Properties;

namespace Synnduit.Events
{
    /// <summary>
    /// Dispatches events to individual event receivers.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    /// <seealso cref="IEventReceiver{TEventArgs}" />
    [Export(typeof(IEventDispatcher<>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class EventDispatcher<TEntity> : IEventDispatcher<TEntity>
        where TEntity : class
    {
        private readonly IEnumerable<IEntityTypeEventReceiver<TEntity>> loggers;

        [ImportingConstructor]
        public EventDispatcher(
            [ImportMany] IEnumerable<IEntityTypeEventReceiver<TEntity>> loggers)
        {
            this.loggers = loggers;
        }

        /// <summary>
        /// To be called when the (deduplication) in-memory cache of destination system
        /// entities has been populated.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void CachePopulated(ICachePopulatedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when the (deduplication) in-memory cache of destination system
        /// entities is about to be populated.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void CachePopulating(ICachePopulatingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when the deletion of a destination system entity (identified for
        /// deletion) has been processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void DeletionProcessed(IDeletionProcessedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when the deletion of a destination system entity (identified for
        /// deletion) is about to be processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void DeletionProcessing(IDeletionProcessingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a garbage collection run segment has been initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void GarbageCollectionInitialized(IGarbageCollectionInitializedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a garbage collection run segment is about to be initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void GarbageCollectionInitializing(IGarbageCollectionInitializingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a subsystem has been initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void Initialized(IInitializedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a subsystem is about to be initialized.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void Initializing(IInitializingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when entities from a source system have been loaded.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void Loaded(ILoadedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when entities from a source system feed are about to be loaded.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void Loading(ILoadingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when source/destination system identifier mappings have been
        /// loaded from the database into the in-memory cache.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void MappingsCached(IMappingsCachedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when source/destination system identifier mappings are about to be
        /// loaded from the database into the in-memory cache.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void MappingsCaching(IMappingsCachingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when an orphan identifier mapping has been processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void OrphanMappingProcessed(IOrphanMappingProcessedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when orphan identifier mappings are about to be processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void OrphanMappingsProcessing(IOrphanMappingsProcessingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a run segment has finished executing.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void SegmentExecuted(ISegmentExecutedArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a run segment is about to be executed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void SegmentExecuting(ISegmentExecutingArgs args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a destination system entity identified for deletion has been
        /// loaded from the destination system.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void DeletionEntityLoaded(IDeletionEntityLoadedArgs<TEntity> args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a source system entity has been processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void Processed(IProcessedArgs<TEntity> args)
        {
            this.Dispatch(args);
        }

        /// <summary>
        /// To be called when a source system entity is about to be processed.
        /// </summary>
        /// <param name="args">The event data.</param>
        public void Processing(IProcessingArgs<TEntity> args)
        {
            this.Dispatch(args);
        }

        private void Dispatch<TEventArgs>(TEventArgs args)
        {
            foreach(IEventReceiver<TEventArgs>
                eventReceiver in this.GetEventReceivers<TEventArgs>())
            {
                try
                {
                    eventReceiver.Occurred(args);
                }
                catch(Exception exception)
                {
                    throw new InvalidOperationException(
                        Resources.EventReceiverThrewException, exception);
                }
            }
        }

        private IEnumerable<IEventReceiver<TEventArgs>> GetEventReceivers<TEventArgs>()
        {
            return
                this
                .loggers
                .Where(l => l is IEventReceiver<TEventArgs>)
                .Cast<IEventReceiver<TEventArgs>>();
        }
    }
}
