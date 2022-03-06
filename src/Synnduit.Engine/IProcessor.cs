using Synnduit.Events;

namespace Synnduit
{
    /// <summary>
    /// Processes individual source system entities.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IProcessor<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Processes the specified source system entity.
        /// </summary>
        /// <param name="entity">The entity to process.</param>
        /// <returns>
        /// The event data representing the outcome of the entity's processing.
        /// </returns>
        IProcessedArgs<TEntity> Process(TEntity entity);
    }
}
