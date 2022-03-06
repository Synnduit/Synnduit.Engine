using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Deduplication;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Extracts information related to types representing assets (i.e., external systems,
    /// entity types, feeds, sinks, etc.) from all currently loaded assemblies.
    /// </summary>
    [Export(typeof(IAssetExtractor))]
    internal class AssetExtractor : IAssetExtractor
    {
        /// <summary>
        /// Gets all types representing external systems from all currently loaded
        /// assemblies.
        /// </summary>
        /// <returns>The collection of types representing external systems.</returns>
        public IEnumerable<SingularAsset<ExternalSystemAttribute>> GetExternalSystems()
        {
            return this.GetSingularAssets<ExternalSystemAttribute>();
        }

        /// <summary>
        /// Gets source system parameters for all types from all currently loaded
        /// assemblies.
        /// </summary>
        /// <returns>
        /// The dictionary of <see cref="SourceSystemParametersAttribute" /> instances
        /// appplied to individual types.
        /// </returns>
        public IDictionary<
            Type, SourceSystemParametersAttribute> GetSourceSystemParameters()
        {
            return
                this
                .GetExportedTypes<SourceSystemParametersAttribute>()
                .ToDictionary(
                    et => et,
                    et => this.GetAttribute<SourceSystemParametersAttribute>(et));
        }

        /// <summary>
        /// Gets all types representing entity types from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing entity types.</returns>
        public IEnumerable<SingularAsset<EntityTypeAttribute>> GetEntityTypes()
        {
            return this.GetSingularAssets<EntityTypeAttribute>();
        }

        /// <summary>
        /// Gets all types representing entity type definitions from all currently loaded
        /// assemblies.
        /// </summary>
        /// <returns>
        /// The collection of types representing entity type definitions.
        /// </returns>
        public IEnumerable<SingularAsset<
            EntityTypeDefinitionAttribute>> GetEntityTypeDefinitions()
        {
            return this.GetSingularAssets<EntityTypeDefinitionAttribute>();
        }

        /// <summary>
        /// Gets the shared source system identifiers for all types from all currently
        /// loaded assemblies.
        /// </summary>
        /// <returns>
        /// The dictionary of <see cref="SharedSourceSystemIdentifiersAttribute" />
        /// instances applied to individual types.
        /// </returns>
        public IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
            GetSharedSourceSystemIdentifiers()
        {
            return
                this
                .GetExportedTypes<SharedSourceSystemIdentifiersAttribute>()
                .ToDictionary(
                    et => et,
                    et => this.GetAttributes<SharedSourceSystemIdentifiersAttribute>(et));
        }

        /// <summary>
        /// Gets all types representing feeds from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing feeds.</returns>
        public IEnumerable<CombinedAsset<FeedAttribute>> GetFeeds()
        {
            return this.GetCombinedAssets<FeedAttribute>();
        }

        /// <summary>
        /// Gets all types representing sinks from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing sinks.</returns>
        public IEnumerable<SingularAsset<SinkAttribute>> GetSinks()
        {
            return this.GetSingularAssets<SinkAttribute>();
        }

        /// <summary>
        /// Gets all types representing cache feeds from all currently loaded assemblies.
        /// </summary>
        /// <returns>The collection of types representing cache feeds.</returns>
        public IEnumerable<SingularAsset<CacheFeedAttribute>> GetCacheFeeds()
        {
            return this.GetSingularAssets<CacheFeedAttribute>();
        }

        private IEnumerable<SingularAsset<TAttribute>> GetSingularAssets<TAttribute>()
        {
            return
                this
                .GetExportedTypes<TAttribute>()
                .Select(et =>
                    new SingularAsset<TAttribute>(
                        et, this.GetAttribute<TAttribute>(et)))
                .ToArray();
        }

        private IEnumerable<CombinedAsset<TAttribute>> GetCombinedAssets<TAttribute>()
        {
            return
                this
                .GetExportedTypes<TAttribute>()
                .Select(et =>
                    new CombinedAsset<TAttribute>(
                        et, this.GetAttributes<TAttribute>(et)))
                .ToArray();
        }

        private IEnumerable<Type> GetExportedTypes<TAttribute>()
        {
            return
                AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => a.IsDynamic == false)
                .SelectMany(a => a.ExportedTypes)
                .Where(et => this.HasAttribute<TAttribute>(et))
                .ToArray();
        }

        private IEnumerable<TAttribute> GetAttributes<TAttribute>(Type type)
        {
            return
                type
                .GetCustomAttributes(typeof(TAttribute), false)
                .Cast<TAttribute>()
                .ToArray();
        }

        private TAttribute GetAttribute<TAttribute>(Type type)
        {
            return
                this
                .GetAttributes<TAttribute>(type)
                .SingleOrDefault();
        }

        private bool HasAttribute<TAttribute>(Type type)
        {
            return this.GetAttributes<TAttribute>(type).Any();
        }
    }
}
