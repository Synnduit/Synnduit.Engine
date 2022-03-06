using System;

namespace Synnduit
{
    /// <summary>
    /// Creates instances that implement the <see cref="IBridge" /> interface.
    /// </summary>
    internal interface IBridgeFactory
    {
        /// <summary>
        /// Creates a new instance that implements the <see cref="IBridge" /> interface.
        /// </summary>
        /// <param name="segmentType">The current run segment type.</param>
        /// <param name="entityType">The current entity-representing type.</param>
        /// <param name="bootstrapper">
        /// The <see cref="IBootstrapper" /> instance to use.
        /// </param>
        /// <returns>
        /// A new instance that implements the <see cref="IBridge" /> interface.
        /// </returns>
        IBridge CreateBridge(
            SegmentType segmentType,
            Type entityType,
            IBootstrapper bootstrapper);
    }
}
