using Synnduit.Configuration;
using System.ComponentModel.Composition;

namespace Synnduit
{
    /// <summary>
    /// Provides information relevant to the run segment that's currently being executed.
    /// </summary>
    [Export(typeof(IContext))]
    internal class Context : IContext
    {
        private readonly IWritableContext writableContext;

        [ImportingConstructor]
        public Context(IWritableContext writableContext)
        {
            this.writableContext = writableContext;
        }

        /// <summary>
        /// Gets the type of the current run segment.
        /// </summary>
        public SegmentType SegmentType => this.writableContext.SegmentType;

        /// <summary>
        /// Gets the one-based index of the current run segment.
        /// </summary>
        public int SegmentIndex => this.writableContext.SegmentIndex;

        /// <summary>
        /// Gets the number of segments in the current run.
        /// </summary>
        public int SegmentCount => this.writableContext.SegmentCount;

        /// <summary>
        /// Gets source system information.
        /// </summary>
        public IExternalSystem SourceSystem => this.writableContext.SourceSystem;

        /// <summary>
        /// Gets destination system information.
        /// </summary>
        public IExternalSystem DestinationSystem => this.writableContext.DestinationSystem;

        /// <summary>
        /// Gets entity type infromation.
        /// </summary>
        public IEntityType EntityType => this.writableContext.EntityType;

        /// <summary>
        /// Gets the collection of all registered external systems.
        /// </summary>
        public IEnumerable<IExternalSystem> ExternalSystems =>
            this.writableContext.ExternalSystems;

        /// <summary>
        /// Gets the collection of all registered entity types.
        /// </summary>
        public IEnumerable<IEntityType> EntityTypes => this.writableContext.EntityTypes;

        /// <summary>
        /// Gets the consolidated dictionary of parameters applicable to the current run
        /// segment.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters => this.writableContext.Parameters;

        /// <summary>
        /// Gets the run data; this dictionary allows individual feeds, sinks, and services
        /// to preserve arbitrary pieces of data between segments.
        /// </summary>
        public IDictionary<string, object> RunData => this.writableContext.RunData;

        /// <summary>
        /// Gets the current run configuration.
        /// </summary>
        public IRunConfiguration RunConfiguration => this.writableContext.RunConfiguration;

        /// <summary>
        /// Gets the current run segment configuration.
        /// </summary>
        public ISegmentConfiguration SegmentConfiguration =>
            this.writableContext.SegmentConfiguration;
    }
}
