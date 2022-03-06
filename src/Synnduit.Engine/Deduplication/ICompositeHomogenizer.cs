namespace Synnduit.Deduplication
{
    /// <summary>
    /// A homogenizer that applies a collection of homogenizer to the value being
    /// homogenized.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface ICompositeHomogenizer<TEntity> : IHomogenizer
        where TEntity : class
    {
    }
}
