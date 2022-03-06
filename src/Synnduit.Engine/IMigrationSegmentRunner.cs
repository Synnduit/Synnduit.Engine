namespace Synnduit
{
    /// <summary>
    /// Runs individual run segments of the <see cref="SegmentType.Migration" /> type.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IMigrationSegmentRunner<TEntity> : ISegmentRunner
        where TEntity : class
    {
    }
}
