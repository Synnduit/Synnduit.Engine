using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Events;
using Synnduit.Properties;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// Represents an in-memory entity cache to be used for deduplication purposes.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(ICache<>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class Cache<TEntity> : ICache<TEntity>
        where TEntity : class
    {
        private readonly IGateway<TEntity> gateway;

        private readonly ICompositeHomogenizer<TEntity> compositeHomogenizer;

        private readonly IServiceProvider<TEntity> serviceProvider;

        private readonly IEventDispatcher<TEntity> eventDispatcher;

        private bool isCacheInitialized;

        /// <summary>
        /// The list of all cached entities.
        /// </summary>
        private readonly List<TEntity> entities;

        /// <summary>
        /// The list of indices created; used to index and reindex entities.
        /// </summary>
        private readonly List<IIndexer> indices;

        /// <summary>
        /// The dictionary of (index) lists that individual entities have been added to;
        /// (the Keys represent the entities' IDs in the destination system); once an
        /// entity is updated, it will be removed from all the lists and reindexed; this
        /// is done because the indexed values may potentially change during each update.
        /// </summary>
        private readonly Dictionary<EntityIdentifier, List<List<TEntity>>> entityLists;

        [ImportingConstructor]
        public Cache(
            IGateway<TEntity> gateway,
            ICompositeHomogenizer<TEntity> compositeHomogenizer,
            IServiceProvider<TEntity> serviceProvider,
            IEventDispatcher<TEntity> eventDispatcher,
            IEventHub<TEntity> eventHub,
            IInitializer initializer)
        {
            this.gateway = gateway;
            this.compositeHomogenizer = compositeHomogenizer;
            this.serviceProvider = serviceProvider;
            this.eventDispatcher = eventDispatcher;
            this.isCacheInitialized = false;
            this.entities = new List<TEntity>();
            this.indices = new List<IIndexer>();
            this.entityLists = new Dictionary<EntityIdentifier, List<List<TEntity>>>();
            if(gateway.IsCacheFeedAvailable)
            {
                eventHub.Subscribe(new EventReceiver(this));
                initializer.Register(
                    new Initializer(this),
                    suppressEvents: true);
            }
        }

        /// <summary>
        /// Gets the collection of all entities currently in the cache.
        /// </summary>
        /// <returns>The collection of all entities currently in the cache.</returns>
        public IEnumerable<TEntity> GetAll()
        {
            this.ValidateCacheInitialized();
            return this.entities;
        }

        /// <summary>
        /// Creates an index for the specified entity value.
        /// </summary>
        /// <typeparam name="TValue">The indexed value type.</typeparam>
        /// <param name="valueGetter">
        /// Used to get the indexed value from an entity instance.
        /// </param>
        /// <returns>The cache index created.</returns>
        public ICacheIndex<TEntity, TValue>
            CreateIndex<TValue>(Func<TEntity, TValue> valueGetter)
        {
            ArgumentValidator.EnsureArgumentNotNull(valueGetter, nameof(valueGetter));

            // create and return the index
            var index = new CacheIndex<TValue>(this, valueGetter);
            this.indices.Add(index);
            return index;
        }

        /// <summary>
        /// Validates that the cache has been initialized (i.e., populated), throwing an
        /// <see cref="InvalidOperationException" /> if it hasn't.
        /// </summary>
        private void ValidateCacheInitialized()
        {
            if(!this.isCacheInitialized)
            {
                throw new InvalidOperationException(Resources.CacheNotInitialized);
            }
        }

        /// <summary>
        /// Populates the cache with entities.
        /// </summary>
        /// <param name="entities">
        /// The collection of entities to populate the cache with.
        /// </param>
        private void Populate(IEnumerable<TEntity> entities)
        {
            this.entities.AddRange(entities);
            foreach(TEntity entity in entities)
            {
                this.EntityCreatedOrUpdated(entity);
            }
        }

        /// <summary>
        /// Notifies the cache that an entity has been created or updated.
        /// </summary>
        /// <param name="entity">The entity that was created or updated.</param>
        private void EntityCreatedOrUpdated(TEntity entity)
        {
            List<List<TEntity>> entityLists = this.GetEntityLists(entity);
            this.RemoveFromLists(entity, entityLists);
            foreach(IIndexer index in this.indices)
            {
                List<TEntity> entityList = index.IndexEntity(entity);
                if(entityList != null)
                {
                    entityLists.Add(entityList);
                }
            }
        }

        /// <summary>
        /// Gets the list of lists that the specified entity has been added to; the list
        /// will be created, if it doesn't already exist.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The list of lists that the entity has been added to.</returns>
        private List<List<TEntity>> GetEntityLists(TEntity entity)
        {
            EntityIdentifier destinationSystemEntityId =
                this.GetDestinationSystemEntityId(entity);
            if(this.entityLists.TryGetValue(
                destinationSystemEntityId,
                out List<List<TEntity>> entityLists) == false)
            {
                entityLists = new List<List<TEntity>>();
                this.entityLists.Add(
                    destinationSystemEntityId,
                    entityLists);
            }
            return entityLists;
        }

        /// <summary>
        /// Removes the specified entity from each of the specified lists.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityLists">The list of lists to remove the entity from.</param>
        private void RemoveFromLists(TEntity entity, List<List<TEntity>> entityLists)
        {
            EntityIdentifier destinationSystemEntityId =
                this.GetDestinationSystemEntityId(entity);
            foreach(List<TEntity> entityList in entityLists)
            {
                entityList.RemoveAll(
                    e => this.GetDestinationSystemEntityId(e)
                        == destinationSystemEntityId);
            }
            entityLists.Clear();
        }

        /// <summary>
        /// Gets the ID that uniquely identifies the specified entity in the destination
        /// system, validating that the value returned by the metadata provider isn't a
        /// null reference.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The ID that uniquely identifies the entity in the destination system.
        /// </returns>
        private EntityIdentifier GetDestinationSystemEntityId(TEntity entity)
        {
            EntityIdentifier destinationSystemEntityId =
                this.serviceProvider.MetadataProvider.GetDestinationSystemEntityId(entity);
            if(destinationSystemEntityId == null)
            {
                throw new InvalidOperationException(
                    Resources.DestinationSystemEntityIdNotSet);
            }
            return destinationSystemEntityId;
        }

        /// <summary>
        /// The interface, implemented by the entity cache index class (see below), that is
        /// used to index and reindex entities.
        /// </summary>
        private interface IIndexer
        {
            /// <summary>
            /// Indexes the specified entity.
            /// </summary>
            /// <param name="entity">The entity to index.</param>
            /// <returns>
            /// The list that the entity has been added into (and that it should be removed
            /// from when it's modified before it gets reindexed); if the entity is not
            /// added to a list, null will be returned.
            /// </returns>
            List<TEntity> IndexEntity(TEntity entity);
        }

        private class Initializer : IInitializable
        {
            private readonly Cache<TEntity> parent;

            public Initializer(Cache<TEntity> parent)
            {
                this.parent = parent;
            }

            public void Initialize(IInitializationContext context)
            {
                this.parent.eventDispatcher.CachePopulating(new CachePopulatingArgs());
                TEntity[] entities =
                    this.parent.gateway.LoadEntities().ToArray();
                this.parent.Populate(entities);
                this.parent.isCacheInitialized = true;
                this.parent.eventDispatcher.CachePopulated(
                    new CachePopulatedArgs(entities.Length));
            }
        }

        private class EventReceiver : IEventReceiver<TEntity>
        {
            private readonly Cache<TEntity> parent;

            public EventReceiver(Cache<TEntity> parent)
            {
                this.parent = parent;
            }

            public void EntityCreatedOrUpdated(TEntity entity)
            {
                this.parent.EntityCreatedOrUpdated(entity);
            }

            public void EntitiesProcessed(IEnumerable<EntityIdentifier> ids)
            { }
        }

        private class CacheIndex<TValue> : ICacheIndex<TEntity, TValue>, IIndexer
        {
            private readonly Cache<TEntity> parent;

            private readonly Func<TEntity, TValue> valueGetter;

            /// <summary>
            /// The dictionary of entities; the Keys represent the indexed values, the
            /// Values represent the lists of entities with these values (Key); the Keys
            /// are objects, as opposed to instances of TValue, as homogenization may
            /// potentially change types.
            /// </summary>
            private readonly Dictionary<object, List<TEntity>> indexedEntities;

            public CacheIndex(
                Cache<TEntity> parent,
                Func<TEntity, TValue> valueGetter)
            {
                this.parent = parent;
                this.valueGetter = valueGetter;
                this.indexedEntities = new Dictionary<object, List<TEntity>>();
                foreach(TEntity entity in this.parent.entities)
                {
                    this.IndexEntity(entity);
                }
            }

            public IEnumerable<TEntity> GetEntities(TValue value)
            {
                this.parent.ValidateCacheInitialized();

                // get and returned indexed entities
                IEnumerable<TEntity> entities = new TEntity[0];
                if(value != null)
                {
                    object homogenizedValue =
                        this.parent.compositeHomogenizer.Homogenize(value);
                    entities = this.GetEntityList(homogenizedValue, false);
                }
                return entities;
            }

            public List<TEntity> IndexEntity(TEntity entity)
            {
                List<TEntity> entityList = null;
                TValue value = this.valueGetter(entity);
                if(value != null)
                {
                    object homogenizedValue =
                        this.parent.compositeHomogenizer.Homogenize(value);
                    entityList = this.GetEntityList(homogenizedValue, true);
                    entityList.Add(entity);
                }
                return entityList;
            }

            /// <summary>
            /// Gets the list of entities with the specified indexed value.
            /// </summary>
            /// <param name="value">The indexed value.</param>
            /// <param name="addIfNotPresent">
            /// A value indicating whether the list should be added to the index, if not
            /// already present; if this value is false, a new empty List instance will
            /// still be created and returned, but it will not be added to the index.
            /// </param>
            /// <returns>The list of entities with the specified indexed value.</returns>
            private List<TEntity> GetEntityList(object value, bool addIfNotPresent)
            {
                if(this.indexedEntities.TryGetValue(
                    value, out List<TEntity> entityList) == false)
                {
                    entityList = new List<TEntity>();
                    if(addIfNotPresent)
                    {
                        this.indexedEntities.Add(value, entityList);
                    }
                }
                return entityList;
            }
        }

        private class CachePopulatingArgs : ICachePopulatingArgs
        {
        }

        private class CachePopulatedArgs : ICachePopulatedArgs
        {
            private readonly int count;

            public CachePopulatedArgs(int count)
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
