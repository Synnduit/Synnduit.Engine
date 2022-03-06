using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Synnduit.Serialization
{
    /// <summary>
    /// Serializes and deserializes entities for storage in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(ISerializer<>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class Serializer<TEntity> : ISerializer<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        private readonly Lazy<ContractResolver> contractResolver;

        [ImportingConstructor]
        internal Serializer(IMetadataParser<TEntity> metadataParser)
        {
            this.metadataParser = metadataParser;
            this.contractResolver =
                new Lazy<ContractResolver>(() => new ContractResolver(this));
        }

        /// <summary>
        /// Serializes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns>The serialized entity.</returns>
        public byte[] Serialize(TEntity entity)
        {
            var stream = new MemoryStream();
            var serializer = new JsonSerializer()
            {
                ContractResolver = this.contractResolver.Value,
                NullValueHandling = NullValueHandling.Ignore
            };
            using(var writer = new StreamWriter(stream))
            {
                serializer.Serialize(writer, entity);
            }
            return stream.ToArray();
        }

        /// <summary>
        /// Deserializes the entity represented by the specified collection of bytes.
        /// </summary>
        /// <param name="data">The data representing the entity.</param>
        /// <returns>The deserialized entity.</returns>
        public TEntity Deserialize(byte[] data)
        {
            TEntity entity;
            var serializer = new JsonSerializer()
            {
                ContractResolver = this.contractResolver.Value
            };
            var stream = new MemoryStream(data);
            using(var reader = new StreamReader(stream))
            {
                entity = (TEntity)
                    serializer.Deserialize(reader, typeof(TEntity));
            }
            return entity;
        }

        private class ContractResolver : DefaultContractResolver
        {
            private readonly Serializer<TEntity> parent;

            private IEnumerable<JsonProperty> properties;

            public ContractResolver(Serializer<TEntity> parent)
            {
                this.parent = parent;
                this.properties = null;
            }

            protected override IList<JsonProperty> CreateProperties(
                Type type, MemberSerialization memberSerialization)
            {
                if(this.properties == null)
                {
                    this.properties = this.CreateProperties(memberSerialization);
                }
                return this.properties.ToList();
            }

            private IEnumerable<JsonProperty>
                CreateProperties(MemberSerialization memberSerialization)
            {
                var properties = new List<JsonProperty>();
                IDictionary<string, EntityProperty>
                    propertiesByName = this.GetPropertiesByName();
                foreach(JsonProperty property in
                    base
                    .CreateProperties(typeof(TEntity), memberSerialization)
                    .Where(property =>
                        propertiesByName.ContainsKey(property.PropertyName)))
                {
                    property.PropertyName =
                        propertiesByName[property.PropertyName].Property.Name;
                    properties.Add(property);
                }
                return properties;
            }

            private IDictionary<string, EntityProperty> GetPropertiesByName()
            {
                return
                    this
                    .parent
                    .metadataParser
                    .Metadata
                    .EntityProperties
                    .ToDictionary(
                        property =>
                            property.NullableProxyProperty != null
                            ? property.NullableProxyProperty.Name
                            : property.Property.Name);
            }
        }
    }
}
