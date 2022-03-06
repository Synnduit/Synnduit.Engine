namespace Synnduit
{
    /// <summary>
    /// Runs individual run segments of the <see cref="SegmentType.GarbageCollection" />
    /// type.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IGarbageCollectionSegmentRunner<TEntity> : ISegmentRunner
        where TEntity : class
    {
    }
}
