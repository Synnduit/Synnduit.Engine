namespace Synnduit.Merge
{
    /// <summary>
    /// Exposes information related to the merging of a source system entity into its
    /// destination system counterpart.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    public interface IMergerEntity<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the trunk (i.e., destination system) version of the entity.
        /// </summary>
        /// <remarks>
        /// The merger may modify the existing instance, or replace it with another
        /// instance, as long as the IDs remain unchanged.
        /// </remarks>
        TEntity Trunk { get; set; }

        /// <summary>
        /// Gets the previous version of the entity from the source system; this value will
        /// be null if no previous version of the entity from the source system exists.
        /// </summary>
        TEntity Previous { get; }

        /// <summary>
        /// Gets the current version of the entity from the source system.
        /// </summary>
        TEntity Current { get; }

        /// <summary>
        /// Gets the ID of the entity's source system.
        /// </summary>
        Guid SourceSystemId { get; }
    }
}
