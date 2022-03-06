namespace Synnduit.Mappings
{
    /// <summary>
    /// Represents possible states of an identifier mapping.
    /// </summary>
    public enum MappingState
    {
        /// <summary>
        /// The mapping is currently active.
        /// </summary>
        Active = 1,

        /// <summary>
        /// The mapping has been deactivated.
        /// </summary>
        Deactivated = 2,

        /// <summary>
        /// The mapping has been removed.
        /// </summary>
        Removed = 3
    }
}
