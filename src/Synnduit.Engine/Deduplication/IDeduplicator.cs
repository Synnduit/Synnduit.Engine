namespace Synnduit.Deduplication
{
    /// <summary>
    /// Finds duplicates of source system entities in the destination system.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IDeduplicator<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Deduplicates the specified source system entity.
        /// </summary>
        /// <param name="entity">The entity to deduplicate.</param>
        /// <returns>The result of the deduplication process.</returns>
        DeduplicationResult Deduplicate(TEntity entity);
    }
}
