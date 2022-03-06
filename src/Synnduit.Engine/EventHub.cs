using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Synnduit
{
    /// <summary>
    /// Propagates entity-related events to subscribers.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IEventHub<>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class EventHub<TEntity> : IEventHub<TEntity>
        where TEntity : class
    {
        private readonly List<IEventReceiver<TEntity>> eventReceivers;

        [ImportingConstructor]
        public EventHub()
        {
            this.eventReceivers = new List<IEventReceiver<TEntity>>();
        }

        /// <summary>
        /// Subscribes the specified <see cref="IEventReceiver{TEntity}" /> instance to
        /// entity-related events.
        /// </summary>
        /// <param name="receiver">
        /// The <see cref="IEventReceiver{TEntity}" /> instance to subscribe.
        /// </param>
        public void Subscribe(IEventReceiver<TEntity> receiver)
        {
            this.eventReceivers.Add(receiver);
        }

        /// <summary>
        /// Called when an entity has been created or updated.
        /// </summary>
        /// <param name="entity">The entity that was created or updated.</param>
        public void EntityCreatedOrUpdated(TEntity entity)
        {
            foreach(IEventReceiver<TEntity> eventReceiver in this.eventReceivers)
            {
                eventReceiver.EntityCreatedOrUpdated(entity);
            }
        }
    }
}
