using System;
using System.Collections.Generic;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Encapsulates external (source/destination) system information.
    /// </summary>
    internal class ExternalSystemAsset
    {
        public ExternalSystemAsset(
            Type externalSystemType,
            ExternalSystemAttribute attribute,
            SourceSystemParametersAttribute sourceSystemParametersAttribute,
            IEnumerable<EntityTypeAsset> entityTypes)
        {
            this.ExternalSystemType = externalSystemType;
            this.Attribute = attribute;
            this.SourceSystemParametersAttribute = sourceSystemParametersAttribute;
            this.EntityTypes = entityTypes;
        }

        /// <summary>
        /// Gets the ID of the external system.
        /// </summary>
        public Guid Id => this.Attribute.Id;

        /// <summary>
        /// Gets the name of the external system.
        /// </summary>
        public string Name
        {
            get
            {
                return this.Attribute.Name ?? this.ExternalSystemType.Name;
            }
        }

        /// <summary>
        /// Gets the type representing the external system.
        /// </summary>
        public Type ExternalSystemType { get; }

        /// <summary>
        /// Gets the attribute specifying the external system information.
        /// </summary>
        public ExternalSystemAttribute Attribute { get; }

        /// <summary>
        /// Gets the attribute specifying the values of parameters associated with the
        /// external system when acting as a source system.
        /// </summary>
        public SourceSystemParametersAttribute SourceSystemParametersAttribute { get; }

        /// <summary>
        /// Gets the collection of the external (destination) system's entity types.
        /// </summary>
        public IEnumerable<EntityTypeAsset> EntityTypes { get; }
    }
}
