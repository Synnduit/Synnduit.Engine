using Synnduit.Events;

namespace Synnduit
{
    /// <summary>
    /// Represents the initializer that exposes the Initialize method, in addition to
    /// allowing individual classes to register for initialization.
    /// </summary>
    internal interface IInvocableInitializer : IInitializer
    {
        /// <summary>
        /// Initializes individual registered classes.
        /// </summary>
        /// <param name="eventDispatcher">
        /// The <see cref="IEventDispatcher" /> instance to use.
        /// </param>
        void Initialize(IEventDispatcher eventDispatcher);
    }
}
