using System.ComponentModel.Composition;

namespace Synnduit.Merge
{
    /// <summary>
    /// The entity value merger that compares the previous and current version of an entity
    /// from a source system to be merged and propagates all values that differ between the
    /// two versions.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IMerger<>))]
    internal sealed class AllChangesMerger<TEntity> : Merger<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="metadataParser">
        /// The metadata parser to be used by this instance.
        /// </param>
        [ImportingConstructor]
        internal AllChangesMerger(IMetadataParser<TEntity> metadataParser)
            : base(metadataParser, MergeStrategy.AllChanges)
        { }
    }
}
