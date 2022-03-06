using System.Collections.Generic;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Encapsulates entity type metadata information extracted from attributes applied to
    /// the type and its properties and defined via
    /// <see cref="IEntityTypeDefinitionContext{TEntity}" />.
    /// </summary>
    internal class EntityTypeMetadata
    {
        private readonly PropertyInfo sourceSystemIdentifierProperty;

        private readonly PropertyInfo destinationSystemIdentifierProperty;

        private readonly IEnumerable<EntityProperty> entityProperties;

        private readonly IEnumerable<
            ReferenceIdentifierProperty> referenceIdentifierProperties;

        private readonly IEnumerable<PropertyInfo> duplicationKeyProperties;

        public EntityTypeMetadata(
            PropertyInfo sourceSystemIdentifierProperty,
            PropertyInfo destinationSystemIdentifierProperty,
            IEnumerable<EntityProperty> entityProperties,
            IEnumerable<ReferenceIdentifierProperty> referenceIdentifierProperties,
            IEnumerable<PropertyInfo> duplicationKeyProperties)
        {
            this.sourceSystemIdentifierProperty = sourceSystemIdentifierProperty;
            this.destinationSystemIdentifierProperty = destinationSystemIdentifierProperty;
            this.entityProperties = entityProperties;
            this.referenceIdentifierProperties = referenceIdentifierProperties;
            this.duplicationKeyProperties = duplicationKeyProperties;
        }

        /// <summary>
        /// Gets the property that represents the ID that uniquely identifies an entity
        /// in the source system.
        /// </summary>
        public PropertyInfo SourceSystemIdentifierProperty
        {
            get { return this.sourceSystemIdentifierProperty; }
        }

        /// <summary>
        /// Gets the property that represents the ID that uniquely identifies an entity
        /// in the destination system.
        /// </summary>
        public PropertyInfo DestinationSystemIdentifierProperty
        {
            get { return this.destinationSystemIdentifierProperty; }
        }

        /// <summary>
        /// Gets the collection of entity properties.
        /// </summary>
        public IEnumerable<EntityProperty> EntityProperties
        {
            get { return this.entityProperties; }
        }

        /// <summary>
        /// Gets the collection of reference identifier properties.
        /// </summary>
        public IEnumerable<ReferenceIdentifierProperty> ReferenceIdentifierProperties
        {
            get { return this.referenceIdentifierProperties; }
        }

        /// <summary>
        /// Gets the collection of properties that represent duplication keys.
        /// </summary>
        public IEnumerable<PropertyInfo> DuplicationKeyProperties
        {
            get { return this.duplicationKeyProperties; }
        }
    }
}
