using System;
using System.ComponentModel.Composition;
using Synnduit.Persistence;
using Synnduit.Properties;

namespace Synnduit.Serialization
{
    /// <summary>
    /// Serializes, hashes and deserializes entities.
    /// </summary>
    [Export(typeof(IHashingSerializer<>))]
    internal class HashingSerializer<TEntity> : IHashingSerializer<TEntity>
        where TEntity : class
    {
        private readonly IServiceProvider<TEntity> serviceProvider;

        private readonly ISafeMetadataProvider<TEntity> safeMetadataProvider;

        private readonly IHashFunction hashFunction;

        [ImportingConstructor]
        public HashingSerializer(
            IServiceProvider<TEntity> serviceProvider,
            ISafeMetadataProvider<TEntity> safeMetadataProvider,
            IHashFunction hashFunction)
        {
            this.serviceProvider = serviceProvider;
            this.safeMetadataProvider = safeMetadataProvider;
            this.hashFunction = hashFunction;
        }

        /// <summary>
        /// Serializes and hashes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns>The serialized and hashed entity.</returns>
        public ISerializedEntity Serialize(TEntity entity)
        {
            byte[] data = this.serviceProvider.Serializer.Serialize(entity);
            if(data == null)
            {
                throw new InvalidOperationException(Resources.SerializerReturnedNull);
            }
            return new SerializedEntity(
                this.hashFunction.ComputeHash(data),
                data,
                this.safeMetadataProvider.GetLabel(entity));
        }

        /// <summary>
        /// Deserializes the specified entity.
        /// </summary>
        /// <param name="data">The serialized entity as a byte array.</param>
        /// <returns>The deserialized entity.</returns>
        public TEntity Deserialize(byte[] data)
        {
            TEntity entity = this.serviceProvider.Serializer.Deserialize(data);
            if(entity == null)
            {
                throw new InvalidOperationException(Resources.SerializerReturnedNull);
            }
            return entity;
        }

        private class SerializedEntity : ISerializedEntity
        {
            private readonly string dataHash;

            private readonly byte[] data;

            private readonly string label;

            public SerializedEntity(string dataHash, byte[] data, string label)
            {
                this.dataHash = dataHash;
                this.data = data;
                this.label = label;
            }

            public string DataHash
            {
                get { return this.dataHash; }
            }

            public byte[] Data
            {
                get { return this.data; }
            }

            public string Label
            {
                get { return this.label; }
            }
        }
    }
}
