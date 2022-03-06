namespace Synnduit
{
    /// <summary>
    /// Receives events propagated by the <see cref="IEventHub{TEntity}" /> implementation.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IEventReceiver<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Called when an entity has been created or updated.
        /// </summary>
        /// <param name="entity">The entity that was created or updated.</param>
        void EntityCreatedOrUpdated(TEntity entity);
    }
}
