using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// A safe wrapper around the <see cref="IMetadataProvider{TEntity}" /> implementation;
    /// ensures that data returned by individual methods is (formally) valid (e.g., no null
    /// references returned); makes sure a given instance's identifiers don't change
    /// between calls.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(ISafeMetadataProvider<>))]
    internal class SafeMetadataProvider<TEntity> : ISafeMetadataProvider<TEntity>
        where TEntity : class
    {
        private const string SourceSystemEntityIdDictionaryDataKey
            = "SourceSystemEntityIdDictionary";

        private const string DestinationSystemEntityIdDictionaryDataKey
            = "DestinationSystemEntityIdDictionary";

        private readonly IServiceProvider<TEntity> serviceProvider;

        private readonly IOperationExecutive operationExecutive;

        [ImportingConstructor]
        public SafeMetadataProvider(
            IServiceProvider<TEntity> serviceProvider,
            IOperationExecutive operationExecutive)
        {
            this.serviceProvider = serviceProvider;
            this.operationExecutive = operationExecutive;
        }

        /// <summary>
        /// Gets the ID that uniquely identifies the specified entity in its source system.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The ID that uniquely identifies the entity in its source system.
        /// </returns>
        public EntityIdentifier GetSourceSystemEntityId(TEntity entity)
        {
            return this.ReturnValidatedEntityId(
                entity,
                this.serviceProvider.MetadataProvider.GetSourceSystemEntityId,
                SourceSystemEntityIdDictionaryDataKey,
                Resources.GetSourceSystemEntityIdReturnedNull,
                Resources.SourceSystemEntityIdChanged);
        }

        /// <summary>
        /// Gets the ID that uniquely identifies the specified entity in the destination
        /// system.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The ID that uniquely identifies the entity in the destination system.
        /// </returns>
        public EntityIdentifier GetDestinationSystemEntityId(TEntity entity)
        {
            return this.ReturnValidatedEntityId(
                entity,
                this.serviceProvider.MetadataProvider.GetDestinationSystemEntityId,
                DestinationSystemEntityIdDictionaryDataKey,
                Resources.GetDestinationSystemEntityIdReturnedNull,
                Resources.DestinationSystemEntityIdChanged);
        }

        /// <summary>
        /// Gets the specified entity's label (i.e., name or short description).
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity label (i.e., name or short description).</returns>
        public string GetLabel(TEntity entity)
        {
            return this.ReturnNonNullValue(
                this.serviceProvider
                    .MetadataProvider
                    .GetLabel(entity),
                Resources.GetLabelReturnedNull);
        }

        private EntityIdentifier ReturnValidatedEntityId(
            TEntity entity,
            Func<TEntity, EntityIdentifier> getEntityIdMethod,
            string entityIdDictionaryDataKey,
            string entityIdNullExceptionMessage,
            string entityIdChangedExceptionMessageFormat)
        {
            EntityIdentifier entityId = this.ReturnNonNullValue(
                getEntityIdMethod(entity), entityIdNullExceptionMessage);
            IDictionary<TEntity, EntityIdentifier> entityIdDictionary
                = this.GetEntityIdDictionary(entityIdDictionaryDataKey);
            if(entityIdDictionary.TryGetValue(
                entity, out EntityIdentifier previousEntityId))
            {
                if(entityId != previousEntityId)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            entityIdChangedExceptionMessageFormat,
                            previousEntityId,
                            entityId));
                }
            }
            else
            {
                entityIdDictionary.Add(entity, entityId);
            }
            return entityId;
        }

        private TValue ReturnNonNullValue<TValue>(TValue value, string exceptionMessage)
            where TValue : class
        {
            if(value == null)
            {
                throw new InvalidOperationException(exceptionMessage);
            }
            return value;
        }

        private IDictionary<TEntity, EntityIdentifier> GetEntityIdDictionary(string dataKey)
        {
            IDictionary<TEntity, EntityIdentifier> entityIdDictionary;
            this.operationExecutive
                .CurrentOperation
                .Data
                .TryGetValue(dataKey, out object entityIdDictionaryAsObject);
            if(entityIdDictionaryAsObject is IDictionary<TEntity, EntityIdentifier>)
            {
                entityIdDictionary =
                    (IDictionary<TEntity, EntityIdentifier>) entityIdDictionaryAsObject;
            }
            else
            {
                entityIdDictionary = new Dictionary<TEntity, EntityIdentifier>();
                this.operationExecutive
                    .CurrentOperation
                    .Data[dataKey] = entityIdDictionary;
            }
            return entityIdDictionary;
        }
    }
}
