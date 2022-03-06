using Synnduit.Deduplication;

namespace Synnduit
{
    /// <summary>
    /// An internally used "safe" implementation of destination system interfaces (i.e.,
    /// <see cref="ISink{TEntity}" /> and <see cref="ICacheFeed{TEntity}" />); used as a
    /// wrapper around client implementations.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IGateway<TEntity> :
        ISink<TEntity>, ICacheFeed<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets a value indicating whether a <see cref="ICacheFeed{TEntity}" />
        /// implementation is available.
        /// </summary>
        bool IsCacheFeedAvailable { get; }
    }
}
