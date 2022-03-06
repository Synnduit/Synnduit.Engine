using Synnduit.Events;

namespace Synnduit
{
    /// <summary>
    /// Represents a "bridge" between those parts of the system that require a generic
    /// entity type parameter (TEntity) and those that don't have it.
    /// </summary>
    internal interface IBridge
    {
        /// <summary>
        /// Gets the <see cref="IEventDispatcher" /> implementation to be used for the
        /// current run segment.
        /// </summary>
        IEventDispatcher EventDispatcher { get; }

        /// <summary>
        /// Gets the <see cref="IContextValidator" /> implementation to be used for the
        /// current run segment.
        /// </summary>
        IContextValidator ContextValidator { get; }

        /// <summary>
        /// Creates a new instance that implements the <see cref="ISegmentRunner" />
        /// interface.
        /// </summary>
        ISegmentRunner CreateSegmentRunner();
    }
}
