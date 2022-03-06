using System;
using System.Collections.Generic;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Encapsulates feed information.
    /// </summary>
    internal class FeedAsset
    {
        public FeedAsset(
            Type feedType,
            EntityTypeAsset entityType,
            ExternalSystemAsset sourceSystem,
            IEnumerable<FeedAttribute> attributes)
        {
            this.FeedType = feedType;
            this.EntityType = entityType;
            this.SourceSystem = sourceSystem;
            this.Attributes = attributes;
        }

        /// <summary>
        /// Gets the feed type.
        /// </summary>
        public Type FeedType { get; }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityTypeAsset EntityType { get; }

        /// <summary>
        /// Gets the source system.
        /// </summary>
        public ExternalSystemAsset SourceSystem { get; }

        /// <summary>
        /// Gets the collection of attributes, starting with the most specific one,
        /// specifying the feed information.
        /// </summary>
        public IEnumerable<FeedAttribute> Attributes { get; }
    }
}
