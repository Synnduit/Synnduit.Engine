namespace Synnduit.Merge
{
    /// <summary>
    /// Represents individual merge strategies.
    /// </summary>
    internal enum MergeStrategy
    {
        /// <summary>
        /// Compares the previous and current version of an entity from a source system to
        /// be merged and propagates all values that differ between the two versions.
        /// </summary>
        AllChanges = 1,

        /// <summary>
        /// Compares the previous and current version of an entity from a source system to
        /// be merged and propagates the values that differ between them and, at the same
        /// time, do not exist in the trunk version of the entity (they are null
        /// references).
        /// </summary>
        NewValuesOnly
    }
}
