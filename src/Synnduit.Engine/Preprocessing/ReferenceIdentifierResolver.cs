using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Events;
using Synnduit.Properties;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Resolves the destination system entity IDs of referenced entities based on the
    /// (referenced) entities' IDs in their respective originating source systems.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IPreprocessorOperation<>))]
    public sealed class ReferenceIdentifierResolver<TEntity> :
        IPreprocessorOperation<TEntity>
        where TEntity : class
    {
        private readonly IMetadataParser<TEntity> metadataParser;

        private readonly IContext context;

        private readonly IEntityIdentifierConverter entityIdentifierConverter;

        private readonly IMapper mapper;

        private readonly IMessageLogger messageLogger;

        private readonly Dictionary<Type, IEnumerable<IEntityType>> compatibleEntityTypes;

        [ImportingConstructor]
        internal ReferenceIdentifierResolver(
            IMetadataParser<TEntity> metadataParser,
            IContext context,
            IEntityIdentifierConverter entityIdentifierConverter,
            IMapper mapper,
            IMessageLogger messageLogger)
        {
            this.metadataParser = metadataParser;
            this.context = context;
            this.entityIdentifierConverter = entityIdentifierConverter;
            this.mapper = mapper;
            this.messageLogger = messageLogger;
            this.compatibleEntityTypes =
                new Dictionary<Type, IEnumerable<IEntityType>>();
        }

        /// <summary>
        /// Resolves the destination system IDs of all of the specified entity's referenced
        /// entities.
        /// </summary>
        /// <param name="entity">The entity that's being preprocessed.</param>
        public void Preprocess(IPreprocessorEntity<TEntity> entity)
        {
            if(entity.Origin == EntityOrigin.SourceSystem)
            {
                foreach(ReferenceIdentifierProperty property in
                    this.metadataParser.Metadata.ReferenceIdentifierProperties)
                {
                    try
                    {
                        this.ProcessProperty(entity, property);
                    }
                    catch(ReferenceIdentifierResolverException exception)
                    {
                        this.messageLogger.Log(
                            exception.Reject ? MessageType.Error : MessageType.Warning,
                            exception.Message);
                        if(exception.Reject)
                        {
                            entity.Reject();
                        }
                    }
                }
            }
        }

        private void ProcessProperty(
            IPreprocessorEntity<TEntity> entity,
            ReferenceIdentifierProperty property)
        {
            EntityIdentifier sourceSystemEntityId =
                this.entityIdentifierConverter.FromValue(
                    property.SourceSystemIdentifierProperty.GetValue(entity.Entity));
            if(sourceSystemEntityId != null)
            {
                EntityIdentifier destinationSystemEntityId =
                    this.GetDestinationSystemEntityId(
                        property.ReferencedEntityType,
                        sourceSystemEntityId,
                        property.DestinationSystemIdentifierProperty.Name);
                if(destinationSystemEntityId != null)
                {
                    object typedDestinationSystemEntityId =
                        this.entityIdentifierConverter.ToValue(
                            destinationSystemEntityId,
                            property.DestinationSystemIdentifierProperty.PropertyType);
                    property.DestinationSystemIdentifierProperty.SetValue(
                        entity.Entity, typedDestinationSystemEntityId);
                }
                else
                {
                    throw new ReferenceIdentifierResolverException(
                        string.Format(
                            Resources.UnableToResolveReferenceIdentifier,
                            property.DestinationSystemIdentifierProperty.Name,
                            sourceSystemEntityId),
                        this.ShouldRejectEntity(entity, property));
                }
            }
        }

        private EntityIdentifier GetDestinationSystemEntityId(
            Type type, EntityIdentifier sourceSystemEntityId, string propertyName)
        {
            IEnumerable<IEntityType>
                compatibleEntityTypes = this.GetCompatibleEntityTypes(type);
            EntityIdentifier[] destinationSystemEntityIds =
                compatibleEntityTypes
                .Select(cet => this.GetDestinationSystemEntityId(
                    cet, sourceSystemEntityId, propertyName))
                .Where(id => id != null)
                .ToArray();
            if(destinationSystemEntityIds.Length > 1)
            {
                throw new ReferenceIdentifierResolverException(string.Format(
                    Resources.CompatibleEntityTypeReferenceIdentifierConflict,
                    propertyName,
                    sourceSystemEntityId));
            }
            return destinationSystemEntityIds.SingleOrDefault();
        }

        private IEnumerable<IEntityType> GetCompatibleEntityTypes(Type type)
        {
            if(!this.compatibleEntityTypes.TryGetValue(
                type, out IEnumerable<IEntityType> compatibleEntityTypes))
            {
                compatibleEntityTypes = this.FindCompatibleEntityTypes(type);
                this.compatibleEntityTypes.Add(type, compatibleEntityTypes);
            }
            return compatibleEntityTypes;
        }

        private IEnumerable<IEntityType> FindCompatibleEntityTypes(Type type)
        {
            IEntityType[] compatibleEntityTypes =
                this
                .context
                .EntityTypes
                .Where(et => type.IsAssignableFrom(et.Type))
                .ToArray();
            if(compatibleEntityTypes.Length == 0)
            {
                throw new ReferenceIdentifierResolverException(string.Format(
                    Resources.TypeHasNoCompatibleEntityTypes,
                    type.FullName));
            }
            return compatibleEntityTypes;
        }

        private EntityIdentifier GetDestinationSystemEntityId(
            IEntityType entityType,
            EntityIdentifier sourceSystemEntityId,
            string propertyName)
        {
            EntityIdentifier destinationSystemEntityId =
                this.mapper.GetDestinationSystemEntityId(
                    entityType.Id, sourceSystemEntityId);
            if(destinationSystemEntityId == null)
            {
                EntityIdentifier[] destinationSystemEntityIds =
                    entityType
                    .SharedIdentifierSourceSystems
                    .Select(system =>
                        this.mapper.GetDestinationSystemEntityId(
                            entityType.Id, system.Id, sourceSystemEntityId))
                    .Where(id => id != null)
                    .Distinct()
                    .ToArray();
                if(destinationSystemEntityIds.Length > 1)
                {
                    throw new ReferenceIdentifierResolverException(string.Format(
                        Resources.AmbiguousReferenceIdentifer,
                        entityType.Name,
                        propertyName,
                        sourceSystemEntityId));
                }
                destinationSystemEntityId = destinationSystemEntityIds.FirstOrDefault();
            }
            return destinationSystemEntityId;
        }

        private bool ShouldRejectEntity(
            IPreprocessorEntity<TEntity> entity,
            ReferenceIdentifierProperty property)
        {
            return
                (entity.MappingExists == false && property.IsRequiredOnCreation) ||
                (entity.MappingExists == true && property.IsRequiredOnUpdate);
        }

        private class ReferenceIdentifierResolverException : Exception
        {
            public ReferenceIdentifierResolverException(string message, bool reject = true)
                : base(message)
            {
                this.Reject = reject;
            }

            public bool Reject { get; }
        }
    }
}
