using System;
using System.Collections.Generic;
using Synnduit.Deduplication;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Extracts information related to types representing assets (i.e., external systems,
    /// entity types, feeds, sinks, etc.) from all currently loaded assemblies.
    /// </summary>
    internal interface IAssetExtractor
    {
        /// <summary>
        /// Gets all types representing external systems from all currently loaded
        /// assemblies.
        /// </summary>
        /// <returns>The collection of types representing external systems.</returns>
        IEnumerable<SingularAsset<ExternalSystemAttribute>> GetExternalSystems();

        /// <summary>
        /// Gets source system parameters for all types from all currently loaded
        /// assemblies.
        /// </summary>
        /// <returns>
        /// The dictionary of <see cref="SourceSystemParametersAttribute" /> instances
        /// appplied to individual types.
        /// </returns>
        IDictionary<Type, SourceSystemParametersAttribute> GetSourceSystemParameters();

        /// <summary>
        /// Gets all types representing entity types from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing entity types.</returns>
        IEnumerable<SingularAsset<EntityTypeAttribute>> GetEntityTypes();

        /// <summary>
        /// Gets all types representing entity type definitions from all currently loaded
        /// assemblies.
        /// </summary>
        /// <returns>
        /// The collection of types representing entity type definitions.
        /// </returns>
        IEnumerable<SingularAsset<
            EntityTypeDefinitionAttribute>> GetEntityTypeDefinitions();

        /// <summary>
        /// Gets the shared source system identifiers for all types from all currently
        /// loaded assemblies.
        /// </summary>
        /// <returns>
        /// The dictionary of <see cref="SharedSourceSystemIdentifiersAttribute" />
        /// instances applied to individual types.
        /// </returns>
        IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
            GetSharedSourceSystemIdentifiers();

        /// <summary>
        /// Gets all types representing feeds from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing feeds.</returns>
        IEnumerable<CombinedAsset<FeedAttribute>> GetFeeds();

        /// <summary>
        /// Gets all types representing sinks from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing sinks.</returns>
        IEnumerable<SingularAsset<SinkAttribute>> GetSinks();

        /// <summary>
        /// Gets all types representing cache feeds from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing cache feeds.</returns>
        IEnumerable<SingularAsset<CacheFeedAttribute>> GetCacheFeeds();
    }
}
