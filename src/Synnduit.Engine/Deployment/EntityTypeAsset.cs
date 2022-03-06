using System;
using System.Collections.Generic;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Encapsulates entity type information.
    /// </summary>
    internal class EntityTypeAsset
    {
        private readonly
            ISharedIdentifierSourceSystemsProvider sharedIdentifierSourceSystemsProvider;

        public EntityTypeAsset(
            Type entityType,
            EntityTypeAttributeBase attribute,
            ISharedIdentifierSourceSystemsProvider sharedIdentifierSourceSystemsProvider,
            Type sinkType,
            Type cacheFeedType)
        {
            this.EntityType = entityType;
            this.Attribute = attribute;
            this.sharedIdentifierSourceSystemsProvider
                = sharedIdentifierSourceSystemsProvider;
            this.SinkType = sinkType;
            this.CacheFeedType = cacheFeedType;
        }

        /// <summary>
        /// Gets the ID of the entity type.
        /// </summary>
        public Guid Id => this.Attribute.Id;

        /// <summary>
        /// Gets the name of the entity type.
        /// </summary>
        public string Name
        {
            get { return this.Attribute.Name ?? this.EntityType.Name; }
        }

        /// <summary>
        /// Gets the type representing the entity.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the attribute specifying the entity type information.
        /// </summary>
        public EntityTypeAttributeBase Attribute { get; }

        /// <summary>
        /// Gets the collection of sets (collections) of source (external) systems that
        /// share identifiers (for the entity type).
        /// </summary>
        public IEnumerable<IEnumerable<ExternalSystemAsset>> SharedIdentifierSourceSystems
        {
            get
            {
                return
                    this
                    .sharedIdentifierSourceSystemsProvider
                    .GetSharedIdentifierSourceSystems();
            }
        }

        /// <summary>
        /// Gets the entity type's sink type.
        /// </summary>
        public Type SinkType { get; }

        /// <summary>
        /// Gets the entity type's cache feed type.
        /// </summary>
        public Type CacheFeedType { get; }
    }
}
