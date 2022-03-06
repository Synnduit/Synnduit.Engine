using System.Collections.Generic;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Provides collections of sets (collections) of source (external) systems that share
    /// identifiers for a given entity type.
    /// </summary>
    internal interface ISharedIdentifierSourceSystemsProvider
    {
        /// <summary>
        /// Gets the collection of sets (collections) of source (external) systems that
        /// share identifiers for the entity type represented by the current instance.
        /// </summary>
        /// <returns>
        /// The collection of sets (collections) of source (external) systems that share
        /// identifiers for the entity type represented by the current instance.
        /// </returns>
        IEnumerable<IEnumerable<ExternalSystemAsset>> GetSharedIdentifierSourceSystems();
    }
}
