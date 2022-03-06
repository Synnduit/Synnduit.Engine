namespace Synnduit.Mappings
{
    /// <summary>
    /// Represents possible origins of an identifier mapping.
    /// </summary>
    public enum MappingOrigin
    {
        /// <summary>
        /// The mapping was created for a new entity in the destination system.
        /// </summary>
        NewEntity = 1,

        /// <summary>
        /// The mapping was created as a result of deduplication.
        /// </summary>
        Deduplication = 2
    }
}
