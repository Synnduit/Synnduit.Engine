namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Preprocesses source system/destination system entities.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IPreprocessor<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Preprocesses the specified entity.
        /// </summary>
        /// <param name="entity">
        /// The (source/destination system) entity that is being preprocessed.
        /// </param>
        /// <param name="origin">
        /// The origin of the entity that is being preprocessed (source/destination
        /// system).
        /// </param>
        /// <param name="mappingExists">
        /// A value indicating whether a mapping to an existing destination system entity
        /// exists for the entity, which is a source system entity; ignored if origin is
        /// DestinationSystem.
        /// </param>
        /// <returns>The entity after being preprocessed.</returns>
        PreprocessedEntity<TEntity> Preprocess(
            TEntity entity,
            EntityOrigin origin,
            bool? mappingExists = null);
    }
}
