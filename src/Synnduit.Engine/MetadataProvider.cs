using System.ComponentModel.Composition;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Extracts entity-related information that the system needs for entity processing.
    /// </summary>
    [Export(typeof(IMetadataProvider<>))]
    internal sealed class MetadataProvider<TEntity> : IMetadataProvider<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        private readonly IEntityIdentifierConverter entityIdentifierConverter;

        [ImportingConstructor]
        internal MetadataProvider(
            IMetadataParser<TEntity> metadataParser,
            IEntityIdentifierConverter entityIdentifierConverter)
        {
            this.metadataParser = metadataParser;
            this.entityIdentifierConverter = entityIdentifierConverter;
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
            return this.GetIdentifier(
                entity,
                this.metadataParser.Metadata.SourceSystemIdentifierProperty);
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
            return this.GetIdentifier(
                entity,
                this.metadataParser.Metadata.DestinationSystemIdentifierProperty);
        }

        /// <summary>
        /// Gets the specified entity's label (i.e., name or short description).
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity label (i.e., name or short description).</returns>
        public string GetLabel(TEntity entity)
        {
            return entity.ToString();
        }

        private EntityIdentifier GetIdentifier(TEntity entity, PropertyInfo property)
        {
            return this.entityIdentifierConverter.FromValue(property.GetValue(entity));
        }
    }
}
