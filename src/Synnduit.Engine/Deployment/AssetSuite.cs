using System.Collections.Generic;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Encapsulates deployment assets (i.e., external systems, entity types, feeds, etc.).
    /// </summary>
    internal class AssetSuite
    {
        public AssetSuite(
            IEnumerable<ExternalSystemAsset> externalSystems,
            IEnumerable<FeedAsset> feeds)
        {
            this.ExternalSystems = externalSystems;
            this.Feeds = feeds;
        }

        /// <summary>
        /// Gets the collection of external (source/destination) systems.
        /// </summary>
        public IEnumerable<ExternalSystemAsset> ExternalSystems { get; }

        /// <summary>
        /// Gets the collection of feeds.
        /// </summary>
        public IEnumerable<FeedAsset> Feeds { get; }
    }
}
