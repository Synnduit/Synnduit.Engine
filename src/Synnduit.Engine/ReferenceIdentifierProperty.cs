using System;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Encapsulates information of a property that represents reference identifiers.
    /// </summary>
    internal class ReferenceIdentifierProperty
    {
        private readonly PropertyInfo destinationSystemIdentifierProperty;

        private readonly PropertyInfo sourceSystemIdentifierProperty;

        private readonly Type referencedEntityType;

        private readonly bool isMutable;

        private readonly bool isRequiredOnCreation;

        private readonly bool isRequiredOnUpdate;

        public ReferenceIdentifierProperty(
            PropertyInfo destinationSystemIdentifierProperty,
            PropertyInfo sourceSystemIdentifierProperty,
            Type referencedEntityType,
            bool isMutable,
            bool isRequiredOnCreation,
            bool isRequiredOnUpdate)
        {
            this.destinationSystemIdentifierProperty = destinationSystemIdentifierProperty;
            this.sourceSystemIdentifierProperty = sourceSystemIdentifierProperty;
            this.referencedEntityType = referencedEntityType;
            this.isMutable = isMutable;
            this.isRequiredOnCreation = isRequiredOnCreation;
            this.isRequiredOnUpdate = isRequiredOnUpdate;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo" /> representing the property for the
        /// destination system identifier.
        /// </summary>
        public PropertyInfo DestinationSystemIdentifierProperty
        {
            get { return this.destinationSystemIdentifierProperty; }
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo" /> representing the property for the source
        /// system identifier.
        /// </summary>
        public PropertyInfo SourceSystemIdentifierProperty
        {
            get { return this.sourceSystemIdentifierProperty; }
        }

        /// <summary>
        /// Gets the referenced entity type.
        /// </summary>
        public Type ReferencedEntityType
        {
            get { return this.referencedEntityType; }
        }

        /// <summary>
        /// Gets a value indicating whether the value is mutable: i.e., whether or not it
        /// can change during an update operation.
        /// </summary>
        public bool IsMutable
        {
            get { return this.isMutable; }
        }

        /// <summary>
        /// Gets a value indicating whether the reference identifier is required when a new
        /// entity is being created.
        /// </summary>
        public bool IsRequiredOnCreation
        {
            get { return this.isRequiredOnCreation; }
        }

        /// <summary>
        /// Gets a value indicating whether the reference identifier is required when an
        /// existing entity is being updated.
        /// </summary>
        public bool IsRequiredOnUpdate
        {
            get { return this.isRequiredOnUpdate; }
        }
    }
}
