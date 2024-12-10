using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Synnduit.Merge
{
    /// <summary>
    /// The base class for individual entity mergers.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal abstract class Merger<TEntity> : IMerger<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        private readonly MergeStrategy strategy;

        private readonly Lazy<IEnumerable<PropertyGroup>> propertyGroups;

        internal Merger(
            IMetadataParser<TEntity> metadataParser,
            MergeStrategy strategy)
        {
            this.metadataParser = metadataParser;
            this.strategy = strategy;
            this.propertyGroups =
                new Lazy<IEnumerable<PropertyGroup>>(this.CreatePropertyGroups);
        }

        /// <summary>
        /// Merges the source system version of the specified entity into its destination
        /// system counterpart.
        /// </summary>
        /// <param name="entity">The entity to merge.</param>
        /// <returns>The collection of value changes applied.</returns>
        public IEnumerable<ValueChange> Merge(IMergerEntity<TEntity> entity)
        {
            var changes = new List<ValueChange>();
            foreach (PropertyGroup group in this.propertyGroups.Value)
            {
                if (group.ValuesDiffer(entity))
                {
                    if (this.ShouldPropagateChanges(entity.Trunk, group))
                    {
                        group.CopyValues(entity.Trunk, entity.Current, changes);
                    }
                }
            }
            return changes;
        }

        private IEnumerable<PropertyGroup> CreatePropertyGroups()
        {
            var propertyGroups = new List<PropertyGroup>();
            foreach (EntityProperty property in
                this.metadataParser.Metadata.EntityProperties)
            {
                PropertyGroup propertyGroup =
                    this.GetPropertyGroup(propertyGroups, property.GroupName);
                propertyGroup.Properties.Add(property);
            }
            return propertyGroups;
        }

        private PropertyGroup GetPropertyGroup(
            List<PropertyGroup> propertyGroups, string groupName)
        {
            PropertyGroup propertyGroup;
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                propertyGroup = propertyGroups
                    .FirstOrDefault(pg => pg.GroupName == groupName);
                if (propertyGroup == null)
                {
                    propertyGroup = new PropertyGroup(groupName);
                    propertyGroups.Add(propertyGroup);
                }
            }
            else
            {
                propertyGroup = new PropertyGroup(null);
                propertyGroups.Add(propertyGroup);
            }
            return propertyGroup;
        }

        private bool ShouldPropagateChanges(
            TEntity trunkVersion, PropertyGroup propertyGroup)
        {
            bool propagate = false;
            if (this.strategy == MergeStrategy.AllChanges)
            {
                propagate = true;
            }
            else if (this.strategy == MergeStrategy.NewValuesOnly)
            {
                propagate = !propertyGroup.HasValue(trunkVersion);
            }
            return propagate;
        }

        private class PropertyGroup
        {
            public PropertyGroup(string groupName)
            {
                this.GroupName = groupName;
                this.Properties = new List<EntityProperty>();
            }

            public string GroupName { get; }

            public List<EntityProperty> Properties { get; }

            public bool HasValue(TEntity entity)
            {
                bool hasValue = false;
                foreach (EntityProperty property in this.Properties)
                {
                    if (this.GetValue(entity, property) != null)
                    {
                        hasValue = true;
                        break;
                    }
                }
                return hasValue;
            }

            public bool ValuesDiffer(IMergerEntity<TEntity> entity)
            {
                bool valuesDiffer = false;
                foreach (EntityProperty property in this.Properties)
                {
                    object previousValue = this.GetValue(entity.Previous, property);
                    object currentValue = this.GetValue(entity.Current, property);
                    if (!object.Equals(previousValue, currentValue) ||
                        ShouldForceNullPropagation(property, currentValue))
                    {
                        valuesDiffer = true;
                        break;
                    }
                }
                return valuesDiffer;

                bool ShouldForceNullPropagation(EntityProperty property, object currentValue) =>
                    entity.Previous == null &&
                    currentValue == null &&
                    property.ForceNullPropagationSourceSystemIds.Contains(entity.SourceSystemId) &&
                    this.GetValue(entity.Trunk, property) != null;
            }

            public void CopyValues(
                TEntity destinationEntity,
                TEntity sourceEntity,
                IList<ValueChange> changes)
            {
                foreach (EntityProperty property in this.Properties)
                {
                    object previousValue = this.GetValue(destinationEntity, property);
                    object newValue = this.GetValue(sourceEntity, property);
                    if (!this.Equal(previousValue, newValue, property))
                    {
                        if (this.SetValue(destinationEntity, property, newValue))
                        {
                            var change = new ValueChange(
                                property.Property.Name,
                                this.ValueToString(previousValue),
                                this.ValueToString(newValue));
                            changes.Add(change);
                        }
                    }
                }
            }

            private object GetValue(TEntity entity, EntityProperty property)
            {
                object value = null;
                if (entity != null)
                {
                    if (property.NullableProxyProperty != null)
                    {
                        value = property.NullableProxyProperty.GetValue(entity);
                    }
                    else
                    {
                        value = property.Property.GetValue(entity);
                    }
                }
                return value;
            }

            private bool Equal(
                object previousValue,
                object newValue,
                EntityProperty property)
            {
                bool equal;
                if (previousValue is string &&
                    newValue is string &&
                    property.IgnoreTrailingWhiteSpace)
                {
                    equal = this.TrimEnd(previousValue).Equals(this.TrimEnd(newValue));
                }
                else
                {
                    equal = object.Equals(previousValue, newValue);
                }
                return equal;
            }

            private string TrimEnd(object value)
            {
                return ((string)value).TrimEnd();
            }

            private bool SetValue(
                TEntity entity, EntityProperty property, object value)
            {
                bool valueSet = false;
                if (value != null ||
                    property.Property.PropertyType.IsValueType == false ||
                    (property.Property.PropertyType.IsConstructedGenericType &&
                     property.Property.PropertyType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                {
                    property.Property.SetValue(entity, value);
                    if (property.NullableProxyProperty != null)
                    {
                        property.NullableProxyProperty.SetValue(entity, value);
                    }
                    valueSet = true;
                }
                return valueSet;
            }

            private string ValueToString(object value)
            {
                string result = null;
                if (value is IFormattable)
                {
                    result =
                        ((IFormattable)value)
                        .ToString(null, CultureInfo.InvariantCulture);
                }
                else if (value != null)
                {
                    result = value.ToString();
                }
                return result;
            }
        }
    }
}
