using System.ComponentModel.Composition;
using System.Linq;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Sets the values of nullable proxy properties to those of their entity properties
    /// in destination system entities.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IPreprocessorOperation<>))]
    internal class NullableProxyPropertyValueSetter<TEntity> :
        IPreprocessorOperation<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        [ImportingConstructor]
        internal NullableProxyPropertyValueSetter(
            IMetadataParser<TEntity> metadataParser)
        {
            this.metadataParser = metadataParser;
        }

        /// <summary>
        /// Sets the values of the specified entity's nullable proxy properties to those of
        /// their entity properties (as long as it's a destination system entity).
        /// </summary>
        /// <param name="entity">The entity that's being preprocessed.</param>
        public void Preprocess(IPreprocessorEntity<TEntity> entity)
        {
            if(entity.Origin == EntityOrigin.DestinationSystem)
            {
                foreach(EntityProperty property in
                    this
                    .metadataParser
                    .Metadata
                    .EntityProperties
                    .Where(property => property.NullableProxyProperty != null))
                {
                    property.NullableProxyProperty.SetValue(
                        entity.Entity,
                        property.Property.GetValue(entity.Entity));
                }
            }
        }
    }
}
