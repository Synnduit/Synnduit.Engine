using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Encapsulates information of an entity property.
    /// </summary>
    internal class EntityProperty
    {
        public EntityProperty(
            PropertyInfo property,
            PropertyInfo nullableProxyProperty,
            string groupName,
            bool nullifyIfWhiteSpaceOnly,
            bool ignoreTrailingWhiteSpace,
            int maxLength,
            IEnumerable<Guid> forceNullPropagationSourceSystemIds)
        {
            this.Property = property;
            this.NullableProxyProperty = nullableProxyProperty;
            this.GroupName = groupName;
            this.NullifyIfWhiteSpaceOnly = nullifyIfWhiteSpaceOnly;
            this.IgnoreTrailingWhiteSpace = ignoreTrailingWhiteSpace;
            this.MaxLength = maxLength;
            this.ForceNullPropagationSourceSystemIds = forceNullPropagationSourceSystemIds;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo" /> instance representing the property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the <see cref="PropertyInfo" /> instance representing the property's
        /// nullable proxy property; a null reference will be returned, if the property has
        /// no nullable proxy property.
        /// </summary>
        public PropertyInfo NullableProxyProperty { get; }

        /// <summary>
        /// Gets the name of the group that the value belongs to; groups of values are
        /// treated atomically: i.e., if any one value from a group is modified, all
        /// properties belonging to the group are considered modified and are marked for
        /// propagation.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets a value indicating whether the value should be set to null if it consists
        /// exclusively of white space characters; only applicable to strings.
        /// </summary>
        public bool NullifyIfWhiteSpaceOnly { get; }

        /// <summary>
        /// Gets a value indicating whether trailing white space characters should be
        /// ignored during a merge; only applicable to strings.
        /// </summary>
        public bool IgnoreTrailingWhiteSpace { get; }

        /// <summary>
        /// Gets the maximum length of a string value in characters; only applicable to
        /// strings; a negative value (-1) indicates no restriction.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Gets the collection of IDs of source systems for which the propagation of null values
        /// during deduplication shall be forced.
        /// </summary>
        public IEnumerable<Guid> ForceNullPropagationSourceSystemIds { get; }
    }
}
