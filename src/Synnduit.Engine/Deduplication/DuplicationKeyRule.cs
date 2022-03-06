using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Synnduit.Properties;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// The duplication rule that deduplicates entities based on their duplication keys;
    /// i.e., properties decorated with <see cref="DuplicationKeyAttribute" />, as well as
    /// those designated as duplication keys via
    /// <see cref="IEntityTypeDefinitionContext{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IDuplicationRule<>))]
    internal class DuplicationKeyRule<TEntity> : IDuplicationRule<TEntity>
        where TEntity : class
    {
        private readonly ISafeMetadataProvider<TEntity> safeMetadataProvider;

        private readonly IMetadataParser<TEntity> metadataParser;

        private readonly ICache<TEntity> cache;

        private IEnumerable<IndexWrapper> indices;

        [ImportingConstructor]
        internal DuplicationKeyRule(
            IContext context,
            ISafeMetadataProvider<TEntity> safeMetadataProvider,
            IMetadataParser<TEntity> metadataParser,
            ICache<TEntity> cache,
            IInitializer initializer)
        {
            this.safeMetadataProvider = safeMetadataProvider;
            this.metadataParser = metadataParser;
            this.cache = cache;
            this.indices = new IndexWrapper[0];
            if(this.IsEntityTypeSupported(context))
            {
                initializer.Register(
                    new Initializer(this),
                    message: Resources.IndexingCache);
            }
        }

        /// <summary>
        /// Gets the collection of duplicates for the specified entity.
        /// </summary>
        /// <param name="entity">The entity that is being deduplicated.</param>
        /// <returns>The collection of duplicates.</returns>
        public IEnumerable<Duplicate> GetDuplicates(TEntity entity)
        {
            var duplicateIdentifiers = new HashSet<EntityIdentifier>();
            foreach(IndexWrapper index in this.indices)
            {
                foreach(EntityIdentifier identifier in
                    index
                    .GetEntities(entity)
                    .Select(duplicate =>
                        this.safeMetadataProvider.GetDestinationSystemEntityId(duplicate)))
                {
                    duplicateIdentifiers.Add(identifier);
                }
            }
            return
                duplicateIdentifiers
                .Select(identifier => new Duplicate(identifier, MatchWeight.Positive));
        }

        private bool IsEntityTypeSupported(IContext context) =>
            context
            .EntityTypes
            .Where(et => et.Type == typeof(TEntity))
            .Count() > 0;

        private class Initializer : IInitializable
        {
            private readonly DuplicationKeyRule<TEntity> parent;

            public Initializer(DuplicationKeyRule<TEntity> parent)
            {
                this.parent = parent;
            }

            public void Initialize(IInitializationContext context)
            {
                PropertyInfo[] duplicationKeyProperties
                    = this.GetDuplicationKeyProperties();
                var indices = new IndexWrapper[duplicationKeyProperties.Length];
                for(int i = 0; i < duplicationKeyProperties.Length; i++)
                {
                    indices[i] = new IndexWrapper(
                        this.parent.cache, duplicationKeyProperties[i]);
                }
                this.parent.indices = indices;
                context.Message = this.GetResultMessage(indices.Length);
            }

            private PropertyInfo[] GetDuplicationKeyProperties()
            {
                return
                    this
                    .parent
                    .metadataParser
                    .Metadata
                    .DuplicationKeyProperties
                    .ToArray();
            }

            private string GetResultMessage(int indexCount)
            {
                string resultMessage;
                if(indexCount != 1)
                {
                    resultMessage = string.Format(
                        Resources.IndicesCreatedFormat, indexCount);
                }
                else
                {
                    resultMessage = Resources.IndexCreated;
                }
                return resultMessage;
            }
        }

        private class IndexWrapper
        {
            private readonly PropertyInfo property;

            private readonly ICacheIndex<TEntity, object> index;

            public IndexWrapper(ICache<TEntity> cache, PropertyInfo property)
            {
                this.property = property;
                this.index = cache.CreateIndex(this.GetValue);
            }

            public IEnumerable<TEntity> GetEntities(TEntity entity)
            {
                return this.index.GetEntities(this.GetValue(entity));
            }

            private object GetValue(TEntity entity)
            {
                return this.property.GetValue(entity);
            }
        }
    }
}
