using System;
using System.ComponentModel.Composition;
using Synnduit.Deployment;

namespace Synnduit
{
    /// <summary>
    /// Exposes run segment parameters in a strongly-typed fashion.
    /// </summary>
    [Export(typeof(IParameterProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class ParameterProvider : IParameterProvider
    {
        private readonly IContext context;

        private readonly Lazy<OrphanMappingBehavior> orphanMappingBehavior;

        private readonly Lazy<GarbageCollectionBehavior> garbageCollectionBehavior;

        [ImportingConstructor]
        public ParameterProvider(IContext context)
        {
            this.context = context;
            this.orphanMappingBehavior =
                new Lazy<OrphanMappingBehavior>(this.GetOrphanMappingBehavior);
            this.garbageCollectionBehavior =
                new Lazy<GarbageCollectionBehavior>(this.GetGarbageCollectionBehavior);
        }

        /// <summary>
        /// Gets the currently applicable orphan mapping behavior.
        /// </summary>
        public OrphanMappingBehavior OrphanMappingBehavior
        {
            get { return this.orphanMappingBehavior.Value; }
        }

        /// <summary>
        /// Gets the currently applicable garbage collection behavior.
        /// </summary>
        public GarbageCollectionBehavior GarbageCollectionBehavior
        {
            get { return this.garbageCollectionBehavior.Value; }
        }

        private OrphanMappingBehavior GetOrphanMappingBehavior()
        {
            return this.context.GetParameter(
                DeploymentContextExtensions.OrphanMappingBehaviorParameterName,
                OrphanMappingBehavior.None);
        }

        private GarbageCollectionBehavior GetGarbageCollectionBehavior()
        {
            return this.context.GetParameter(
                DeploymentContextExtensions.GarbageCollectionBehaviorParameterName,
                GarbageCollectionBehavior.DeleteCreated);
        }
    }
}
