namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Encapsulates an entity that has been preprocessed.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal class PreprocessedEntity<TEntity>
    {
        public PreprocessedEntity(TEntity entity, bool isRejected)
        {
            this.Entity = entity;
            this.IsRejected = isRejected;
        }

        /// <summary>
        /// Gets the entity that has been preprocessed.
        /// </summary>
        public TEntity Entity { get; }

        /// <summary>
        /// Gets a value indicating whether the entity was rejected by a preprocessor
        /// operation.
        /// </summary>
        public bool IsRejected { get; }
    }
}
