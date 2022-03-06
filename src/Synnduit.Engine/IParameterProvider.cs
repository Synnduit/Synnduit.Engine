namespace Synnduit
{
    /// <summary>
    /// Exposes run segment parameters in a strongly-typed fashion.
    /// </summary>
    internal interface IParameterProvider
    {
        /// <summary>
        /// Gets the currently applicable orphan mapping behavior.
        /// </summary>
        OrphanMappingBehavior OrphanMappingBehavior { get; }

        /// <summary>
        /// Gets the currently applicable garbage collection behavior.
        /// </summary>
        GarbageCollectionBehavior GarbageCollectionBehavior { get; }
    }
}
