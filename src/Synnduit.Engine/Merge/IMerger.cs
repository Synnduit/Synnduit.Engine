using System.Collections.Generic;

namespace Synnduit.Merge
{
    /// <summary>
    /// Merges source system entities into their destination system counterparts.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    public interface IMerger<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Merges the source system version of the specified entity into its destination
        /// system counterpart.
        /// </summary>
        /// <param name="entity">The entity to merge.</param>
        /// <returns>The collection of value changes applied.</returns>
        IEnumerable<ValueChange> Merge(IMergerEntity<TEntity> entity);
    }
}
