using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Synnduit.Deduplication;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Parses individual entity types' metadata information defined by attributes applied
    /// to the types' properties and via
    /// <see cref="IEntityTypeDefinitionContext{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IMetadataParser<>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class MetadataParser<TEntity> : IMetadataParser<TEntity>
        where TEntity : class
    {
        private readonly IEntityTypeDefinition<TEntity> metadataDefinition;

        private readonly Lazy<EntityTypeMetadata> metadata;

        [ImportingConstructor]
        public MetadataParser(
            [Import(AllowDefault = true)] IEntityTypeDefinition<TEntity> metadataDefinition)
        {
            this.metadataDefinition = metadataDefinition;
            this.metadata = new Lazy<EntityTypeMetadata>(this.ParseMetadata);
        }

        /// <summary>
        /// Gets entity type metadata for the class represented by the current instance.
        /// </summary>
        public EntityTypeMetadata Metadata
        {
            get { return this.metadata.Value; }
        }

        private EntityTypeMetadata ParseMetadata()
        {
            var context = new MetadataDefinitionContext();
            if(this.metadataDefinition != null)
            {
                this.metadataDefinition.Define(context);
            }
            return new EntityTypeMetadata(
                this.GetSourceSystemIdentifierProperty(context),
                this.GetDestinationSystemIdentifierProperty(context),
                this.GetEntityProperties(context),
                this.GetReferenceIdentifierProperties(context),
                this.GetDuplicationKeyProperties(context));
        }

        private PropertyInfo
            GetSourceSystemIdentifierProperty(MetadataDefinitionContext context)
        {
            return this.GetIdentifierProperty<SourceSystemIdentifierAttribute>(
                context.SourceSystemIdentifierProperty,
                Resources.SourceSystemIdentifierPropertyNotFound);
        }

        private PropertyInfo
            GetDestinationSystemIdentifierProperty(MetadataDefinitionContext context)
        {
            return this.GetIdentifierProperty<DestinationSystemIdentifierAttribute>(
                context.DestinationSystemIdentifierProperty,
                Resources.DestinationSystemIdentifierPropertyNotFound);
        }

        private PropertyInfo GetIdentifierProperty<TAttribute>(
            PropertyInfo contextIdentifierProperty,
            string exceptionMessageFormat)
            where TAttribute : Attribute
        {
            PropertyInfo identifierProperty = contextIdentifierProperty;
            if(identifierProperty == null)
            {
                identifierProperty = this.GetProperty<TAttribute>();
                if(identifierProperty == null)
                {
                    throw new InvalidOperationException(string.Format(
                        exceptionMessageFormat, typeof(TEntity).FullName));
                }
                this.ValidatePropertyReadable(identifierProperty);
            }
            return identifierProperty;
        }

        private PropertyInfo GetProperty<TAttribute>()
            where TAttribute : Attribute
        {
            try
            {
                return
                    typeof(TEntity)
                    .GetProperties()
                    .SingleOrDefault(
                        property => property.GetCustomAttribute<TAttribute>() != null);
            }
            catch(InvalidOperationException)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.AmbiguousIdentifierProperty,
                    typeof(TAttribute).Name,
                    typeof(TEntity).FullName));
            }
        }

        private IEnumerable<EntityProperty>
            GetEntityProperties(MetadataDefinitionContext context)
        {
            return this.GetConsolidatedProperties(
                property => property.Property.Name,
                this.ValidateEntityProperty,
                this.GetAttributeEntityProperties(),
                context.EntityProperties);
        }

        private IEnumerable<ReferenceIdentifierProperty>
            GetReferenceIdentifierProperties(MetadataDefinitionContext context)
        {
            return this.GetConsolidatedProperties(
                property => property.DestinationSystemIdentifierProperty.Name,
                this.ValidateReferenceIdentifierProperty,
                this.GetAttributeReferenceIdentifierProperties(),
                context.ReferenceIdentifierProperties);
        }

        private IEnumerable<PropertyInfo>
            GetDuplicationKeyProperties(MetadataDefinitionContext context)
        {
            return this.GetConsolidatedProperties(
                property => property.Name,
                this.ValidatePropertyReadable,
                this.GetAttributeDuplicationKeyProperties(),
                context.DuplicationKeyProperties);
        }

        private IEnumerable<TProperty> GetConsolidatedProperties<TProperty, TKey>(
            Func<TProperty, TKey> getKey,
            Action<TProperty> validateProperty,
            params IEnumerable<TProperty>[] propertyLists)
        {
            var properties = new Dictionary<TKey, TProperty>();
            foreach(TProperty property in
                propertyLists.SelectMany(propertyList => propertyList))
            {
                validateProperty(property);
                properties[getKey(property)] = property;
            }
            return properties.Values.ToArray();
        }

        private IEnumerable<EntityProperty> GetAttributeEntityProperties()
        {
            return
                this
                .GetProperties<EntityPropertyAttribute>()
                .Select(property => new EntityProperty(
                    property.Property,
                    this.GetProperty(property.Attribute.NullableProxyProperty),
                    property.Attribute.GroupName,
                    property.Attribute.NullifyIfWhiteSpaceOnly,
                    property.Attribute.IgnoreTrailingWhiteSpace,
                    this.GetMaxLength(property.Property)));
        }

        private IEnumerable<ReferenceIdentifierProperty>
            GetAttributeReferenceIdentifierProperties()
        {
            return
                this
                .GetProperties<ReferenceIdentifierAttribute>()
                .Select(property => new ReferenceIdentifierProperty(
                    property.Property,
                    this.GetReferenceSourceSystemIdentifierProperty(
                        property.Attribute.ReferenceIdentifierPropertyName),
                    property.Attribute.ReferencedEntityType,
                    property.Attribute.IsMutable,
                    property.Attribute.IsRequiredOnCreation,
                    property.Attribute.IsRequiredOnUpdate));
        }

        private IEnumerable<PropertyInfo> GetAttributeDuplicationKeyProperties()
        {
            return
                this
                .GetProperties<DuplicationKeyAttribute>()
                .Select(property => property.Property);
        }

        private IEnumerable<PropertyWrapper<TAttribute>> GetProperties<TAttribute>()
            where TAttribute : Attribute
        {
            var properties = new List<PropertyWrapper<TAttribute>>();
            foreach(PropertyInfo property in typeof(TEntity).GetProperties())
            {
                TAttribute attribute = property.GetCustomAttribute<TAttribute>();
                if(attribute != null)
                {
                    properties.Add(
                        new PropertyWrapper<TAttribute>(property, attribute));
                }
            }
            return properties;
        }

        private PropertyInfo GetProperty(string propertyName)
        {
            PropertyInfo property = null;
            if(propertyName != null)
            {
                property = typeof(TEntity).GetProperty(propertyName);
                if(property == null)
                {
                    this.ThrowPropertyException(Resources.PropertyNotFound, propertyName);
                }
            }
            return property;
        }

        private int GetMaxLength(PropertyInfo property)
        {
            int maxLength = -1;
            MaxLengthAttribute maxLengthAttribute =
                property.GetCustomAttribute<MaxLengthAttribute>();
            if(maxLengthAttribute != null)
            {
                maxLength = maxLengthAttribute.Length;
            }
            return maxLength;
        }

        private PropertyInfo
            GetReferenceSourceSystemIdentifierProperty(string propertyName)
        {
            PropertyInfo property = typeof(TEntity).GetProperty(propertyName);
            if(property == null)
            {
                this.ThrowPropertyException(
                    Resources.PropertyNotFound, propertyName);
            }
            return property;
        }

        private void ValidateEntityProperty(EntityProperty entityProperty)
        {
            this.ValidatePropertyReadableAndWritable(entityProperty.Property);
            if(entityProperty.NullableProxyProperty != null)
            {
                this.ValidateProxyProperty(
                    entityProperty.Property,
                    entityProperty.NullableProxyProperty);
            }
        }

        private void ValidateProxyProperty(
            PropertyInfo property, PropertyInfo proxyProperty)
        {
            if(property.PropertyType.IsValueType == false)
            {
                this.ThrowPropertyException(
                    Resources.PropertyNotValueType, property.Name);
            }
            this.ValidatePropertyReadableAndWritable(proxyProperty);
            this.ValidateProxyPropertyType(property, proxyProperty);
        }

        private void ValidateProxyPropertyType(
            PropertyInfo property, PropertyInfo proxyProperty)
        {
            bool isValid = true;
            if(proxyProperty.PropertyType.IsConstructedGenericType == false)
            {
                isValid = false;
            }
            else if(
                proxyProperty
                .PropertyType
                .GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                isValid = false;
            }
            else if(
                proxyProperty
                .PropertyType
                .GetGenericArguments()
                .First() != property.PropertyType)
            {
                isValid = false;
            }
            if(isValid == false)
            {
                this.ThrowPropertyException(
                    Resources.InvalidProxyPropertyType, proxyProperty.Name);
            }
        }

        private void ValidateReferenceIdentifierProperty(
            ReferenceIdentifierProperty referenceIdentifierProperty)
        {
            this.ValidatePropertyReadableAndWritable(
                referenceIdentifierProperty.DestinationSystemIdentifierProperty);
            this.ValidatePropertyReadable(
                referenceIdentifierProperty.SourceSystemIdentifierProperty);
        }

        private void ValidatePropertyReadableAndWritable(PropertyInfo property)
        {
            if(!(property.CanRead && property.CanWrite))
            {
                this.ThrowPropertyException(
                    Resources.PropertyMustBeReadableAndWritable, property.Name);
            }
        }

        private void ValidatePropertyReadable(PropertyInfo property)
        {
            if(property.CanRead == false)
            {
                this.ThrowPropertyException(
                    Resources.PropertyMustBeReadable, property.Name);
            }
        }

        private void ThrowPropertyException(
            string exceptionMessageFormat, string propertyName)
        {
            throw new InvalidOperationException(string.Format(
                exceptionMessageFormat, typeof(TEntity).FullName, propertyName));
        }

        private class MetadataDefinitionContext : IEntityTypeDefinitionContext<TEntity>
        {
            private readonly List<EntityProperty> entityProperties;

            private readonly List<
                ReferenceIdentifierProperty> referenceIdentifierProperties;

            private readonly List<PropertyInfo> duplicationKeyProperties;

            public MetadataDefinitionContext()
            {
                this.entityProperties = new List<EntityProperty>();
                this.referenceIdentifierProperties =
                    new List<ReferenceIdentifierProperty>();
                this.duplicationKeyProperties = new List<PropertyInfo>();
            }

            public void SourceSystemIdentifier<TValue>(
                Expression<Func<TEntity, TValue>> propertyExpression)
            {
                this.SourceSystemIdentifierProperty =
                    this.GetProperty(propertyExpression, nameof(propertyExpression));
            }

            public void DestinationSystemIdentifier<TValue>(
                Expression<Func<TEntity, TValue>> propertyExpression)
            {
                this.DestinationSystemIdentifierProperty =
                    this.GetProperty(propertyExpression, nameof(propertyExpression));
            }

            public void EntityProperty<TValue>(
                Expression<Func<TEntity, TValue>> propertyExpression,
                string groupName,
                bool nullifyIfWhiteSpaceOnly,
                bool ignoreTrailingWhiteSpace,
                int maxLength)
            {
                ArgumentValidator.EnsureArgumentNotNull(
                    propertyExpression, nameof(propertyExpression));
                this.entityProperties.Add(
                    new EntityProperty(
                        this.GetProperty(propertyExpression, nameof(propertyExpression)),
                        null,
                        groupName,
                        nullifyIfWhiteSpaceOnly,
                        ignoreTrailingWhiteSpace,
                        maxLength));
            }

            public void EntityProperty<TValue>(
                Expression<Func<TEntity, TValue>> propertyExpression,
                Expression<Func<TEntity, TValue?>> nullableProxyPropertyExpression,
                string groupName,
                int maxLength)
                where TValue : struct
            {
                ArgumentValidator.EnsureArgumentNotNull(
                    propertyExpression, nameof(propertyExpression));
                ArgumentValidator.EnsureArgumentNotNull(
                    nullableProxyPropertyExpression,
                    nameof(nullableProxyPropertyExpression));
                this.entityProperties.Add(
                    new EntityProperty(
                        this.GetProperty(propertyExpression, nameof(propertyExpression)),
                        this.GetProperty(
                            nullableProxyPropertyExpression,
                            nameof(nullableProxyPropertyExpression)),
                        groupName,
                        true,
                        false,
                        maxLength));
            }

            public void ReferenceIdentifier<TDestinationIdentifier, TSourceIdentifier>(
                Expression<Func<TEntity, TDestinationIdentifier>>
                    destinationIdentifierPropertyExpression,
                Expression<Func<TEntity, TSourceIdentifier>>
                    sourceIdentifierPropertyExpression,
                Type referencedEntityType,
                bool isMutable,
                bool isRequiredOnCreation,
                bool isRequiredOnUpdate)
            {
                ArgumentValidator.EnsureArgumentNotNull(
                    destinationIdentifierPropertyExpression,
                    nameof(destinationIdentifierPropertyExpression));
                ArgumentValidator.EnsureArgumentNotNull(
                    sourceIdentifierPropertyExpression,
                    nameof(sourceIdentifierPropertyExpression));
                ArgumentValidator.EnsureArgumentNotNull(
                    referencedEntityType, nameof(referencedEntityType));
                this.referenceIdentifierProperties.Add(
                    new ReferenceIdentifierProperty(
                        this.GetProperty(
                            destinationIdentifierPropertyExpression,
                            nameof(destinationIdentifierPropertyExpression)),
                        this.GetProperty(
                            sourceIdentifierPropertyExpression,
                            nameof(sourceIdentifierPropertyExpression)),
                        referencedEntityType,
                        isMutable,
                        isRequiredOnCreation,
                        isRequiredOnUpdate));
            }

            public void DuplicationKey<TValue>(
                Expression<Func<TEntity, TValue>> propertyExpression)
            {
                ArgumentValidator.EnsureArgumentNotNull(
                    propertyExpression, nameof(propertyExpression));
                this.duplicationKeyProperties.Add(
                    this.GetProperty(propertyExpression, nameof(propertyExpression)));
            }

            public PropertyInfo SourceSystemIdentifierProperty { get; private set; }

            public PropertyInfo DestinationSystemIdentifierProperty { get; private set; }

            public IEnumerable<EntityProperty> EntityProperties
            {
                get { return this.entityProperties; }
            }

            public IEnumerable<ReferenceIdentifierProperty> ReferenceIdentifierProperties
            {
                get { return this.referenceIdentifierProperties; }
            }

            public IEnumerable<PropertyInfo> DuplicationKeyProperties
            {
                get { return this.duplicationKeyProperties; }
            }

            private PropertyInfo GetProperty<TValue>(
                Expression<Func<TEntity, TValue>> expression,
                string paramName)
            {
                PropertyInfo property;
                string propertyName = this.ExtractPropertyName(expression, paramName);
                property = typeof(TEntity).GetProperty(propertyName);
                if(property == null)
                {
                    throw new ArgumentException(
                        Resources.ExpressionDoesNotRepresentProperty, paramName);
                }
                return property;
            }

            private string ExtractPropertyName<TValue>(
                Expression<Func<TEntity, TValue>> expression,
                string paramName)
            {
                string propertyName;
                string expressionBodyAsString = expression.Body.ToString();
                int dotIndex = expressionBodyAsString.LastIndexOf('.');
                if(dotIndex >= 0)
                {
                    propertyName = expressionBodyAsString.Substring(dotIndex + 1);
                }
                else
                {
                    throw new ArgumentException(
                        Resources.CannotExtractPropertyName, paramName);
                }
                return propertyName;
            }
        }

        private class PropertyWrapper<TAttribute>
            where TAttribute : Attribute
        {
            public PropertyWrapper(PropertyInfo property, TAttribute attribute)
            {
                this.Property = property;
                this.Attribute = attribute;
            }

            public PropertyInfo Property { get; }

            public TAttribute Attribute { get; }
        }
    }
}
