using Synnduit.Persistence;

namespace Synnduit.Serialization
{
    /// <summary>
    /// Serializes, hashes and deserializes entities.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IHashingSerializer<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Serializes and hashes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns>The serialized and hashed entity.</returns>
        ISerializedEntity Serialize(TEntity entity);

        /// <summary>
        /// Deserializes the specified entity.
        /// </summary>
        /// <param name="data">The serialized entity as a byte array.</param>
        /// <returns>The deserialized entity.</returns>
        TEntity Deserialize(byte[] data);
    }
}
