using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Deploys all assets (i.e., external systems, entity types, and feeds) represented by
    /// types denoted with the appropriate attributes from all currently loaded assemblies.
    /// </summary>
    [Export(typeof(IDeploymentStep))]
    internal class AssetsDeploymentStep : IDeploymentStep
    {
        private readonly IAssetAssembler assetAssembler;

        [ImportingConstructor]
        public AssetsDeploymentStep(IAssetAssembler assetAssembler)
        {
            this.assetAssembler = assetAssembler;
        }

        /// <summary>
        /// Deploys all assets (i.e., external systems, entity types, and feeds)
        /// represented by types denoted with the appropriate attributes from all currently
        /// loaded assemblies.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IDeploymentContext" /> instance to use.
        /// </param>
        public void Execute(IDeploymentContext context)
        {
            AssetSuite assetSuite = this.assetAssembler.AssembleAssets();
            this.DeployExternalSystems(context, assetSuite.ExternalSystems);
            this.DeployFeeds(context, assetSuite.Feeds);
        }

        private void DeployExternalSystems(
            IDeploymentContext context, IEnumerable<ExternalSystemAsset> externalSystems)
        {
            foreach(ExternalSystemAsset externalSystem in externalSystems)
            {
                context.ExternalSystem(externalSystem.Id, externalSystem.Name);
                this.SetExternalSystemParameters(context, externalSystem);
                this.DeployExternalSystemEntityTypes(context, externalSystem);
            }
        }

        private void SetExternalSystemParameters(
            IDeploymentContext context, ExternalSystemAsset externalSystem)
        {
            if(externalSystem.Attribute.OrphanMappingBehavior != 0)
            {
                context.DestinationSystemOrphanMappingBehavior(
                    externalSystem.Id,
                    externalSystem.Attribute.OrphanMappingBehavior);
            }
            if(externalSystem.Attribute.GarbageCollectionBehavior != 0)
            {
                context.DestinationSystemGarbageCollectionBehavior(
                    externalSystem.Id,
                    externalSystem.Attribute.GarbageCollectionBehavior);
            }
            if((externalSystem.SourceSystemParametersAttribute?.OrphanMappingBehavior)
                .GetValueOrDefault(0) != 0)
            {
                context.SourceSystemOrphanMappingBehavior(
                    externalSystem.Id,
                    externalSystem.SourceSystemParametersAttribute.OrphanMappingBehavior);
            }
        }

        private void DeployExternalSystemEntityTypes(
            IDeploymentContext context, ExternalSystemAsset externalSystem)
        {
            foreach(EntityTypeAsset entityType in externalSystem.EntityTypes)
            {
                context.EntityType(
                    entityType.Id,
                    externalSystem.Id,
                    entityType.Name,
                    entityType.EntityType,
                    entityType.SinkType,
                    entityType.Attribute.SuppressCacheFeed
                        ? null
                        : entityType.CacheFeedType,
                    entityType.Attribute.IsMutable,
                    entityType.Attribute.IsDuplicable);
                this.SetEntityTypeParameters(context, entityType);
                this.SetEntityTypeSharedSourceSystemIdentifiers(context, entityType);
            }
        }

        private void SetEntityTypeParameters(
            IDeploymentContext context, EntityTypeAsset entityType)
        {
            if(entityType.Attribute.OrphanMappingBehavior != 0)
            {
                context.EntityTypeOrphanMappingBehavior(
                    entityType.Id, entityType.Attribute.OrphanMappingBehavior);
            }
            if(entityType.Attribute.GarbageCollectionBehavior != 0)
            {
                context.EntityTypeGarbageCollectionBehavior(
                    entityType.Id, entityType.Attribute.GarbageCollectionBehavior);
            }
        }

        private void SetEntityTypeSharedSourceSystemIdentifiers(
            IDeploymentContext context, EntityTypeAsset entityType)
        {
            foreach(IEnumerable<ExternalSystemAsset> sourceSystems
                in entityType.SharedIdentifierSourceSystems)
            {
                context.SharedSourceSystemIdentifiers(
                    entityType.Id,
                    sourceSystems.Select(ss => ss.Id).ToArray());
            }
        }

        private void DeployFeeds(IDeploymentContext context, IEnumerable<FeedAsset> feeds)
        {
            foreach(FeedAsset feed in feeds)
            {
                context.Feed(
                    feed.EntityType.Id,
                    feed.SourceSystem.Id,
                    feed.FeedType);
                this.SetEntityTypeSourceSystemParameters(context, feed);
            }
        }

        private void SetEntityTypeSourceSystemParameters(
            IDeploymentContext context, FeedAsset feed)
        {
            OrphanMappingBehavior? orphanMappingBehavior =
                this.GetEntityTypeSourceSystemParameterValue(
                    feed, fa => fa.OrphanMappingBehavior);
            if(orphanMappingBehavior.HasValue)
            {
                context.OrphanMappingBehavior(
                    feed.EntityType.Id,
                    feed.SourceSystem.Id,
                    orphanMappingBehavior.Value);
            }
        }

        private T? GetEntityTypeSourceSystemParameterValue<T>(
            FeedAsset feed,
            Func<FeedAttribute, T> getParameter)
            where T : struct
        {
            return
                feed
                .Attributes
                .Where(a => Convert.ToInt32(getParameter(a)) != 0)
                .Select(a => (T?) getParameter(a))
                .FirstOrDefault();
        }
    }
}
