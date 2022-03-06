using System.ComponentModel.Composition;

namespace Synnduit.Merge
{
    /// <summary>
    /// The entity value merger that compares the previous and current version of an entity
    /// from a source system to be merged and propagates the values that differ between
    /// them and, at the same time, do not exist in the trunk version of the entity (they
    /// are null references).
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    //[Export(typeof(IMerger<>))]
    internal class NewValuesOnlyMerger<TEntity> : Merger<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="metadataParser">
        /// The metadata parser to be used by this instance.
        /// </param>
        [ImportingConstructor]
        internal NewValuesOnlyMerger(IMetadataParser<TEntity> metadataParser)
            : base(metadataParser, MergeStrategy.NewValuesOnly)
        { }
    }
}
