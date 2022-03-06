using System.ComponentModel.Composition;
using System.Linq;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Sets the value of each string property of an entity to null, if the value is
    /// currently white space only.
    /// </summary>
    [Export(typeof(IPreprocessorOperation<>))]
    internal sealed class WhiteSpaceNullifier<TEntity> : IPreprocessorOperation<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        [ImportingConstructor]
        internal WhiteSpaceNullifier(IMetadataParser<TEntity> metadataParser)
        {
            this.metadataParser = metadataParser;
        }

        /// <summary>
        /// Sets the value of each string property of the specified to null, if the value
        /// is currently white space only.
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
                    property.Property.PropertyType == typeof(string)
                    && property.NullifyIfWhiteSpaceOnly))
            {
                string propertyValue = (string)
                    property.Property.GetValue(entity.Entity);
                if(propertyValue != null &&
                    string.IsNullOrWhiteSpace(propertyValue))
                {
                    property.Property.SetValue(entity.Entity, null);
                }
            }
        }
    }
}
