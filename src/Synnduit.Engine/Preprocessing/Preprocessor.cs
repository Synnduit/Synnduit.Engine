using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Synnduit.Properties;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Preprocesses source system/destination system entities.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IPreprocessor<>))]
    internal class Preprocessor<TEntity> : IPreprocessor<TEntity>
        where TEntity : class
    {
        private readonly IServiceProvider<TEntity> serviceProvider;

        [ImportingConstructor]
        public Preprocessor(IServiceProvider<TEntity> serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Preprocesses the specified entity.
        /// </summary>
        /// <param name="entity">
        /// The (source/destination system) entity that is being preprocessed.
        /// </param>
        /// <param name="origin">
        /// The origin of the entity that is being preprocessed (source/destination
        /// system).
        /// </param>
        /// <param name="mappingExists">
        /// A value indicating whether a mapping to an existing destination system entity
        /// exists for the entity, which is a source system entity; ignored if origin is
        /// DestinationSystem.
        /// </param>
        /// <returns>The entity after being preprocessed.</returns>
        public PreprocessedEntity<TEntity> Preprocess(
            TEntity entity,
            EntityOrigin origin,
            bool? mappingExists = null)
        {
            var preprocessorEntity =
                new PreprocessorEntity(entity, origin, mappingExists);
            IEnumerable<IPreprocessorOperation<TEntity>> operations =
                this.GetPreprocessorOperations(origin);
            foreach(IPreprocessorOperation<TEntity> operation in operations)
            {
                operation.Preprocess(preprocessorEntity);
            }
            return new PreprocessedEntity<TEntity>(
                preprocessorEntity.Entity, preprocessorEntity.IsRejected);
        }

        private IEnumerable<IPreprocessorOperation<TEntity>>
            GetPreprocessorOperations(EntityOrigin origin)
        {
            return origin == EntityOrigin.SourceSystem
                ? this.serviceProvider.SourceSystemPreprocessorOperations
                : this.serviceProvider.DestinationSystemPreprocessorOperations;
        }

        private class PreprocessorEntity : IPreprocessorEntity<TEntity>
        {
            private TEntity entity;

            private readonly EntityOrigin origin;

            private readonly bool? mappingExists;

            public PreprocessorEntity(
                TEntity entity, EntityOrigin origin, bool? mappingExists)
            {
                this.Entity = entity;
                this.origin = origin;
                this.mappingExists = mappingExists;
                this.IsRejected = false;
            }

            public TEntity Entity
            {
                get { return this.entity; }
                set
                {
                    this.entity =
                        value
                        ?? throw new ArgumentNullException(nameof(value));
                }
            }

            public EntityOrigin Origin
            {
                get { return this.origin; }
            }

            public bool? MappingExists
            {
                get { return this.mappingExists; }
            }

            public void Reject()
            {
                if(this.Origin != EntityOrigin.SourceSystem)
                {
                    throw new InvalidOperationException(
                        Resources.CannotRejectDestinationSystemEntity);
                }
                this.IsRejected = true;
            }

            public bool IsRejected { get; private set; }
        }
    }
}
