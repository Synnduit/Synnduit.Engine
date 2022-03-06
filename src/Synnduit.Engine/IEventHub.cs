namespace Synnduit
{
    /// <summary>
    /// Propagates entity-related events to subscribers.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IEventHub<TEntity> : IEventReceiver<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Subscribes the specified <see cref="IEventReceiver{TEntity}" /> instance to
        /// entity-related events.
        /// </summary>
        /// <param name="receiver">
        /// The <see cref="IEventReceiver{TEntity}" /> instance to subscribe.
        /// </param>
        void Subscribe(IEventReceiver<TEntity> receiver);
    }
}
