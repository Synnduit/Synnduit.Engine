using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Transactions;
using Synnduit.Preprocessing;

namespace Synnduit
{
    /// <summary>
    /// An internally used "safe" implementation of destination system interfaces (i.e.,
    /// <see cref="ISink{TEntity}" /> and <see cref="Deduplication.ICacheFeed{TEntity}" />);
    /// used as a wrapper around client implementations.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IGateway<>))]
    internal class SinkGateway<TEntity> : IGateway<TEntity>
        where TEntity : class
    {
        private readonly IServiceProvider<TEntity> serviceProvider;

        private readonly IPreprocessor<TEntity> preprocessor;

        [ImportingConstructor]
        public SinkGateway(
            IServiceProvider<TEntity> serviceProvider,
            IPreprocessor<TEntity> preprocessor)
        {
            this.serviceProvider = serviceProvider;
            this.preprocessor = preprocessor;
        }

        /// <summary>
        /// Gets a value indicating whether a
        /// <see cref="Deduplication.ICacheFeed{TEntity}" /> implementation is available.
        /// </summary>
        public bool IsCacheFeedAvailable
        {
            get
            {
                return this.serviceProvider.CacheFeed != null;
            }
        }

        /// <summary>
        /// Loads the collection of entities (of the type represented by the current
        /// instance) that currently exist in the destination system.
        /// </summary>
        /// <returns>
        /// The collection of entities that currently exist in the destination system.
        /// </returns>
        public IEnumerable<TEntity> LoadEntities()
        {
            using(TransactionScope scope = this.CreateTransactionScope())
            {
                return
                    this
                    .serviceProvider
                    .CacheFeed
                    .LoadEntities()
                    .Select(entity => this.Preprocess(entity))
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets the collection of IDs of entities that currently exist in the system.
        /// </summary>
        /// <returns>
        /// The collection of IDs of entities that currently exist in the system.
        /// </returns>
        /// <remarks>
        /// This method need not be implemented if you don't intend to use garbage
        /// collection.
        /// </remarks>
        public IEnumerable<EntityIdentifier> GetEntityIdentifiers()
        {
            using(TransactionScope scope = this.CreateTransactionScope())
            {
                return
                    this
                    .serviceProvider
                    .Sink
                    .GetEntityIdentifiers()
                    .ToArray();
            }
        }

        /// <summary>
        /// Creates a new empty instance of an entity represented by the current entity
        /// type.
        /// </summary>
        /// <returns>
        /// A new empty instance of an entity represented by the current entity type.
        /// </returns>
        public TEntity New()
        {
            return this.Execute(() => this.serviceProvider.Sink.New());
        }

        /// <summary>
        /// Creates a new entity instance in the destination system.
        /// </summary>
        /// <param name="entity">The entity instance to create.</param>
        /// <returns>
        /// The entity created; may be the same instance as the parameter passed to the
        /// method or another instance, as long as its destination system entity identifier
        /// value is populated.
        /// </returns>
        public TEntity Create(TEntity entity)
        {
            return this.Execute(() => this.serviceProvider.Sink.Create(entity));
        }

        /// <summary>
        /// Gets the specified entity instance from the destination system.
        /// </summary>
        /// <param name="id">The ID of the requested entity instance.</param>
        /// <returns>The requested entity instance.</returns>
        public TEntity Get(EntityIdentifier id)
        {
            return this.Execute(
                () =>
                {
                    TEntity entity = this.serviceProvider.Sink.Get(id);
                    if(entity != null)
                    {
                        entity = this.Preprocess(entity);
                    }
                    return entity;
                });
        }

        /// <summary>
        /// Updates the specified entity instance in the destination system.
        /// </summary>
        /// <param name="entity">The updated entity instance.</param>
        public void Update(TEntity entity)
        {
            this.Execute(() => this.serviceProvider.Sink.Update(entity));
        }

        /// <summary>
        /// Deletes the specified entity instance in the destination system.
        /// </summary>
        /// <param name="entity">The entity instance to delete.</param>
        /// <remarks>
        /// This method need not be implemented if you don't intend to use garbage
        /// collection.
        /// </remarks>
        public void Delete(TEntity entity)
        {
            this.Execute(() => this.serviceProvider.Sink.Delete(entity));
        }

        public T Execute<T>(Func<T> method)
        {
            using(TransactionScope scope = this.CreateTransactionScope())
            {
                try
                {
                    return method();
                }
                catch(Exception exception)
                {
                    throw new DestinationSystemException(exception);
                }
            }
        }

        public void Execute(Action method)
        {
            using(TransactionScope scope = this.CreateTransactionScope())
            {
                try
                {
                    method();
                }
                catch(Exception exception)
                {
                    throw new DestinationSystemException(exception);
                }
            }
        }

        private TransactionScope CreateTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Suppress);
        }

        private TEntity Preprocess(TEntity entity)
        {
            return
                this
                .preprocessor
                .Preprocess(entity, EntityOrigin.DestinationSystem)
                .Entity;
        }
    }
}
