using System;

namespace Synnduit
{
    /// <summary>
    /// Creates instances that implement the <see cref="IBridge" /> interface.
    /// </summary>
    internal class BridgeFactory : IBridgeFactory
    {
        private static Lazy<BridgeFactory> instance =
            new Lazy<BridgeFactory>(() => new BridgeFactory());

        /// <summary>
        /// Gets the singleton instance of the class.
        /// </summary>
        public static BridgeFactory Instance
        {
            get { return instance.Value; }
        }

        private BridgeFactory()
        { }

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
        public IBridge CreateBridge(
            SegmentType segmentType,
            Type entityType,
            IBootstrapper bootstrapper)
        {
            return new Bridge(segmentType, entityType, bootstrapper);
        }
    }
}
