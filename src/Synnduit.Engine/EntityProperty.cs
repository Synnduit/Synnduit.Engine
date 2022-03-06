using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Encapsulates information of an entity property.
    /// </summary>
    internal class EntityProperty
    {
        private readonly PropertyInfo property;

        private readonly PropertyInfo nullableProxyProperty;

        private readonly string groupName;

        private readonly bool nullifyIfWhiteSpaceOnly;

        private readonly bool ignoreTrailingWhiteSpace;

        private readonly int maxLength;

        public EntityProperty(
            PropertyInfo property,
            PropertyInfo nullableProxyProperty,
            string groupName,
            bool nullifyIfWhiteSpaceOnly,
            bool ignoreTrailingWhiteSpace,
            int maxLength)
        {
            this.property = property;
            this.nullableProxyProperty = nullableProxyProperty;
            this.groupName = groupName;
            this.nullifyIfWhiteSpaceOnly = nullifyIfWhiteSpaceOnly;
            this.ignoreTrailingWhiteSpace = ignoreTrailingWhiteSpace;
            this.maxLength = maxLength;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo" /> instance representing the property.
        /// </summary>
        public PropertyInfo Property
        {
            get { return this.property; }
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo" /> instance representing the property's
        /// nullable proxy property; a null reference will be returned, if the property has
        /// no nullable proxy property.
        /// </summary>
        public PropertyInfo NullableProxyProperty
        {
            get { return this.nullableProxyProperty; }
        }

        /// <summary>
        /// Gets the name of the group that the value belongs to; groups of values are
        /// treated atomically: i.e., if any one value from a group is modified, all
        /// properties belonging to the group are considered modified and are marked for
        /// propagation.
        /// </summary>
        public string GroupName
        {
            get { return this.groupName; }
        }

        /// <summary>
        /// Gets a value indicating whether the value should be set to null if it consists
        /// exclusively of white space characters; only applicable to strings.
        /// </summary>
        public bool NullifyIfWhiteSpaceOnly
        {
            get { return this.nullifyIfWhiteSpaceOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether trailing white space characters should be
        /// ignored during a merge; only applicable to strings.
        /// </summary>
        public bool IgnoreTrailingWhiteSpace
        {
            get { return this.ignoreTrailingWhiteSpace; }
        }

        /// <summary>
        /// Gets the maximum length of a string value in characters; only applicable to
        /// strings; a negative value (-1) indicates no restriction.
        /// </summary>
        public int MaxLength
        {
            get { return this.maxLength; }
        }
    }
}
