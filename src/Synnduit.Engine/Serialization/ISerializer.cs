namespace Synnduit.Serialization
{
    /// <summary>
    /// Serializes and deserializes entities for storage in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface ISerializer<TEntity>
    {
        /// <summary>
        /// Serializes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns>The serialized entity.</returns>
        byte[] Serialize(TEntity entity);

        /// <summary>
        /// Deserializes the entity represented by the specified collection of bytes.
        /// </summary>
        /// <param name="data">The data representing the entity.</param>
        /// <returns>The deserialized entity.</returns>
        TEntity Deserialize(byte[] data);
    }
}
