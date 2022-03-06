using System.ComponentModel.Composition;
using System.Linq;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Ensures that no string entity properties are longer than their maximum allowed
    /// length.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IPreprocessorOperation<>))]
    internal sealed class StringLengthEnforcer<TEntity> : IPreprocessorOperation<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        [ImportingConstructor]
        internal StringLengthEnforcer(IMetadataParser<TEntity> metadataParser)
        {
            this.metadataParser = metadataParser;
        }

        /// <summary>
        /// Ensures that none of the specified entity's string entity properties are longer
        /// than their maximum allowed length.
        /// </summary>
        /// <param name="entity">The entity that's being preprocessed.</param>
        public void Preprocess(IPreprocessorEntity<TEntity> entity)
        {
            foreach(EntityProperty property in
                this
                .metadataParser
                .Metadata
                .EntityProperties
                .Where(property =>
                    property.Property.PropertyType == typeof(string) &&
                    property.MaxLength >= 0))
            {
                this.ProcessProperty(entity.Entity, property);
            }
        }

        private void ProcessProperty(TEntity entity, EntityProperty property)
        {
            string propertyValue = (string) property.Property.GetValue(entity);
            if(propertyValue != null)
            {
                if(propertyValue.Length > property.MaxLength)
                {
                    propertyValue = propertyValue.Substring(0, property.MaxLength);
                    property.Property.SetValue(entity, propertyValue);
                }
            }
        }
    }
}
