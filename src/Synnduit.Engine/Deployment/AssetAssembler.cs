using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Deduplication;
using Synnduit.Properties;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Assembles the suite of assets (i.e., external systems, entity types, feeds, etc.)
    /// found in all currently loaded assemblies.
    /// </summary>
    [Export(typeof(IAssetAssembler))]
    internal class AssetAssembler : IAssetAssembler
    {
        private readonly int MaxExternalSystemNameLength = 128;

        private readonly int MaxEntityTypeNameLength = 128;

        private readonly IAssetExtractor assetExtractor;

        [ImportingConstructor]
        public AssetAssembler(IAssetExtractor assetExtractor)
        {
            this.assetExtractor = assetExtractor;
        }

        /// <summary>
        /// Assembles the suite of assets (i.e., external systems, entity types, feeds,
        /// etc.) found in all currently loaded assemblies.
        /// </summary>
        /// <returns>The suite of assets assembled.</returns>
        public AssetSuite AssembleAssets()
        {
            AssetSuite assetSuite;
            IDictionary<Type, ExternalSystemAttribute>
                externalSystems = this.GetExternalSystems();
            IDictionary<Type, SourceSystemParametersAttribute>
                sourceSystemParameters = this.assetExtractor.GetSourceSystemParameters();
            EntityTypeSuite entityTypes = this.GetEntityTypes(externalSystems);
            ConnectorSuite sinks = this.GetSinks(externalSystems, entityTypes);
            ConnectorSuite cacheFeeds = this.GetCacheFeeds(externalSystems, entityTypes);
            IEnumerable<Feed> feeds = this.GetFeeds(externalSystems, entityTypes);
            assetSuite = this.AssembleAssets(
                externalSystems,
                sourceSystemParameters,
                entityTypes,
                sinks,
                cacheFeeds,
                feeds);
            this.ValidateAssetSuite(assetSuite);
            return assetSuite;
        }

        private IDictionary<Type, ExternalSystemAttribute> GetExternalSystems()
        {
            return
                this
                .assetExtractor
                .GetExternalSystems()
                .ToDictionary(es => es.AssetType, es => es.AssetAttribute);
        }

        private EntityTypeSuite GetEntityTypes(
            IDictionary<Type, ExternalSystemAttribute> externalSystems)
        {
            var destinationSystemEntityTypes =
                externalSystems
                .Keys
                .ToDictionary(
                    externalSystemType => externalSystemType,
                    externalSystemType => (IDictionary<Type, EntityTypeWrapper>)
                        new Dictionary<Type, EntityTypeWrapper>());
            var allEntityTypes = new HashSet<Type>();
            IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
                sharedSourceSystemIdentifiers =
                this.assetExtractor.GetSharedSourceSystemIdentifiers();
            this.ProcessEntityTypeAssets(
                externalSystems,
                destinationSystemEntityTypes,
                allEntityTypes,
                sharedSourceSystemIdentifiers);
            this.ProcessEntityTypeDefinitionAssets(
                externalSystems,
                destinationSystemEntityTypes,
                allEntityTypes,
                sharedSourceSystemIdentifiers);
            this.ValidateSharedSourceSystemIdentifiersAttributesUsage(
                sharedSourceSystemIdentifiers);
            return new EntityTypeSuite(
                destinationSystemEntityTypes, allEntityTypes);
        }

        private void ProcessEntityTypeAssets(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            IDictionary<Type, IDictionary<Type, EntityTypeWrapper>>
                destinationSystemEntityTypes,
            HashSet<Type> allEntityTypes,
            IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
                sharedSourceSystemIdentifiers)
        {
            foreach(SingularAsset<EntityTypeAttribute>
                entityTypeAsset in this.assetExtractor.GetEntityTypes())
            {
                this.AddEntityType(
                    destinationSystemEntityTypes,
                    allEntityTypes,
                    entityTypeAsset.AssetType,
                    new EntityTypeWrapper(
                        entityTypeAsset.AssetAttribute,
                        this.GetSharedSourceSystemIdentifiers(
                            sharedSourceSystemIdentifiers,
                            externalSystems,
                            entityTypeAsset.AssetType)));
            }
        }

        private void ProcessEntityTypeDefinitionAssets(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            IDictionary<Type, IDictionary<Type, EntityTypeWrapper>>
                destinationSystemEntityTypes,
            HashSet<Type> allEntityTypes,
            IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
                sharedSourceSystemIdentifiers)
        {
            foreach(SingularAsset<EntityTypeDefinitionAttribute> entityTypeDefinitionAsset
                in this.assetExtractor.GetEntityTypeDefinitions())
            {
                Type entityType = this.GetEntityTypeDefinitionEntityType(
                    entityTypeDefinitionAsset.AssetType);
                this.AddEntityType(
                    destinationSystemEntityTypes,
                    allEntityTypes,
                    entityType,
                    new EntityTypeWrapper(
                        entityTypeDefinitionAsset.AssetAttribute,
                        this.GetSharedSourceSystemIdentifiers(
                            sharedSourceSystemIdentifiers,
                            externalSystems,
                            entityType,
                            entityTypeDefinitionAsset.AssetType)));
            }
        }

        private void AddEntityType(
            IDictionary<Type, IDictionary<Type, EntityTypeWrapper>>
                destinationSystemEntityTypes,
            HashSet<Type> allEntityTypes,
            Type entityType,
            EntityTypeWrapper entityTypeWrapper)
        {
            if(allEntityTypes.Add(entityType))
            {
                if(destinationSystemEntityTypes.ContainsKey(
                    entityTypeWrapper.Attribute.DestinationSystem))
                {
                    destinationSystemEntityTypes[
                        entityTypeWrapper.Attribute.DestinationSystem]
                        .Add(entityType, entityTypeWrapper);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.EntityTypeDestinationSystemDoesNotExist,
                        entityTypeWrapper.Attribute.DestinationSystem.FullName,
                        entityType.FullName));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    Resources.EntityTypeDefinedMultipleTimes,
                    entityType.FullName));
            }
        }

        private Type GetEntityTypeDefinitionEntityType(Type entityTypeDefinitionType)
        {
            Type entityType;
            if(this.IsNonGeneric(entityTypeDefinitionType))
            {
                IEnumerable<Type> interfaceImplementations =
                    this.GetInterfaceImplementations(
                        entityTypeDefinitionType, typeof(IEntityTypeDefinition<>));
                if(interfaceImplementations.Count() == 1)
                {
                    entityType = this.ExtractEntityType(interfaceImplementations.Single());
                }
                else
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.EntityTypeDefinitionImplementsInterfaceMultipleTimes,
                        entityTypeDefinitionType.FullName,
                        typeof(IEntityTypeDefinition<>).FullName));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format(
                    Resources.EntityTypeDefinitionHasGenericArguments,
                    entityTypeDefinitionType.FullName));
            }
            return entityType;
        }

        private IEnumerable<SharedSourceSystemIdentifiersAttribute>
            GetSharedSourceSystemIdentifiers(
                IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
                    sharedSourceSystemIdentifiers,
                IDictionary<Type, ExternalSystemAttribute> externalSystems,
                params Type[] types)
        {
            var typeSharedSourceSystemIdentifiers =
                new List<SharedSourceSystemIdentifiersAttribute>();
            foreach(Type type in types)
            {
                if(sharedSourceSystemIdentifiers.ContainsKey(type))
                {
                    typeSharedSourceSystemIdentifiers
                        .AddRange(sharedSourceSystemIdentifiers[type]);
                    sharedSourceSystemIdentifiers.Remove(type);
                }
            }
            this.ValidateSharedSourceSystemIdentifiers(
                typeSharedSourceSystemIdentifiers, externalSystems);
            return typeSharedSourceSystemIdentifiers;
        }

        private void ValidateSharedSourceSystemIdentifiers(
            IEnumerable<SharedSourceSystemIdentifiersAttribute>
                sharedSourceSystemIdentifiers,
            IDictionary<Type, ExternalSystemAttribute> externalSystems)
        {
            var sourceSystems = new HashSet<Type>();
            foreach(Type sourceSystem in
                sharedSourceSystemIdentifiers.SelectMany(sssi => sssi.SourceSystemTypes))
            {
                this.ValidateExternalSystem(sourceSystem, externalSystems);
                if(!sourceSystems.Add(sourceSystem))
                {
                    throw new InvalidOperationException(
                        Resources.SourceSystemCannotShareIdentifiersWithMultipleGroups);
                }
            }
        }

        private void ValidateSharedSourceSystemIdentifiersAttributesUsage(
            IDictionary<Type, IEnumerable<SharedSourceSystemIdentifiersAttribute>>
                sharedSourceSystemIdentifiers)
        {
            if(sharedSourceSystemIdentifiers.Count > 0)
            {
                throw new InvalidOperationException(
                    Resources.SharedSourceSystemIdentifiersAttributeAppliedToInvalidType);
            }
        }

        private ConnectorSuite GetSinks(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            EntityTypeSuite entityTypes)
        {
            return this.GetDestinationSystemConnectors(
                this.assetExtractor.GetSinks(),
                sinkAttribute => sinkAttribute.DestinationSystem,
                typeof(ISink<>),
                externalSystems,
                entityTypes,
                Resources.MultipleSinksDefinedForDestinationSystem,
                Resources.MultipleSinksDefinedForEntityType);
        }

        private ConnectorSuite GetCacheFeeds(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            EntityTypeSuite entityTypes)
        {
            return this.GetDestinationSystemConnectors(
                this.assetExtractor.GetCacheFeeds(),
                cacheFeedAttribute => cacheFeedAttribute.DestinationSystem,
                typeof(ICacheFeed<>),
                externalSystems,
                entityTypes,
                Resources.MultipleCacheFeedsDefinedForDestinationSystem,
                Resources.MultipleCacheFeedsDefinedForEntityType);
        }

        private ConnectorSuite GetDestinationSystemConnectors<TAttribute>(
            IEnumerable<SingularAsset<TAttribute>> connectorAssets,
            Func<TAttribute, Type> getDestinationSystem,
            Type genericInterfaceType,
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            EntityTypeSuite entityTypes,
            string multipleAssetsDefinedForDestinationSystemExceptionMessageFormat,
            string multipleAssetsDefinedForEntityTypeExceptionMessageFormat)
        {
            var destinationSystemConnectors = new Dictionary<Type, Type>();
            var entityTypeConnectors = new Dictionary<Type, Type>();
            foreach(SingularAsset<TAttribute> connectorAsset in connectorAssets)
            {
                bool isNonGeneric = this.IsNonGeneric(connectorAsset.AssetType);
                bool hasOneGenericArgument =
                    this.HasOneGenericArgument(connectorAsset.AssetType);
                Type destinationSystem =
                    getDestinationSystem(connectorAsset.AssetAttribute);
                if(hasOneGenericArgument && destinationSystem != null)
                {
                    this.AddDestinationSystemConnector(
                        destinationSystemConnectors,
                        destinationSystem,
                        connectorAsset.AssetType,
                        genericInterfaceType,
                        externalSystems,
                        multipleAssetsDefinedForDestinationSystemExceptionMessageFormat);
                }
                else if(isNonGeneric && destinationSystem == null)
                {
                    this.AddEntityTypeConnectors(
                        entityTypeConnectors,
                        connectorAsset.AssetType,
                        genericInterfaceType,
                        entityTypes,
                        multipleAssetsDefinedForEntityTypeExceptionMessageFormat);
                }
                else
                {
                    this.ThrowInterfaceImplementedIllegaly(
                        connectorAsset.AssetType, genericInterfaceType);
                }
            }
            return new ConnectorSuite(
                destinationSystemConnectors, entityTypeConnectors);
        }

        private void AddDestinationSystemConnector(
            IDictionary<Type, Type> destinationSystemConnectors,
            Type destinationSystem,
            Type connectorAssetType,
            Type genericInterfaceType,
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            string multipleAssetsDefinedForDestinationSystemExceptionMessageFormat)
        {
            if(this.ImplementsGenericInterface(connectorAssetType, genericInterfaceType))
            {
                if(!destinationSystemConnectors.ContainsKey(destinationSystem))
                {
                    destinationSystemConnectors.Add(
                        destinationSystem, connectorAssetType);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(
                        multipleAssetsDefinedForDestinationSystemExceptionMessageFormat,
                        destinationSystem.FullName));
                }
            }
            else
            {
                this.ThrowTypeDoesNotImplementInterfaceException(
                    connectorAssetType, genericInterfaceType);
            }
        }

        private void AddEntityTypeConnectors(
            IDictionary<Type, Type> entityTypeConnectors,
            Type connectorAssetType,
            Type genericInterfaceType,
            EntityTypeSuite entityTypes,
            string multipleAssetsDefinedForEntityTypeExceptionMessageFormat)
        {
            foreach(Type interfaceImplementation in
                this.GetInterfaceImplementations(connectorAssetType, genericInterfaceType))
            {
                Type entityType = this.ExtractEntityType(interfaceImplementation);
                this.ValidateEntityType(entityType, entityTypes);
                if(!entityTypeConnectors.ContainsKey(entityType))
                {
                    entityTypeConnectors.Add(entityType, connectorAssetType);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(
                        multipleAssetsDefinedForEntityTypeExceptionMessageFormat,
                        entityType.FullName));
                }
            }
        }

        private IEnumerable<Feed> GetFeeds(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            EntityTypeSuite entityTypes)
        {
            var feeds = new HashSet<Feed>();
            foreach(CombinedAsset<FeedAttribute> feedAsset
                in this.assetExtractor.GetFeeds())
            {
                bool isNonGeneric = this.IsNonGeneric(feedAsset.AssetType);
                bool hasOneGenericArgument =
                    this.HasOneGenericArgument(feedAsset.AssetType);
                if(isNonGeneric)
                {
                    this.ProcessNonGenericFeedAsset(
                        feeds, feedAsset, externalSystems, entityTypes);
                }
                else if(hasOneGenericArgument)
                {
                    this.ProcessGenericFeed(
                        feeds, feedAsset, externalSystems, entityTypes);
                }
                else
                {
                    this.ThrowInterfaceImplementedIllegaly(
                        feedAsset.AssetType, typeof(IFeed<>));
                }
            }
            return feeds;
        }

        private void ProcessNonGenericFeedAsset(
            HashSet<Feed> feeds,
            CombinedAsset<FeedAttribute> feedAsset,
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            EntityTypeSuite entityTypes)
        {
            FeedAttribute defaultAttribute = this.GetDefaultFeedAttribute(feedAsset);
            IDictionary<Type, FeedAttribute> entityTypeFeedAttributes =
                this.GetEntityTypeFeedAttributes(feedAsset);
            foreach(Type feedImplementation in
                this.GetInterfaceImplementations(feedAsset.AssetType, typeof(IFeed<>)))
            {
                Type entityType = this.ExtractEntityType(feedImplementation);
                this.ValidateEntityType(entityType, entityTypes);
                IEnumerable<FeedAttribute> feedAttributes = this.GetFeedAttributes(
                    feedAsset.AssetType,
                    entityType,
                    defaultAttribute,
                    entityTypeFeedAttributes);
                Type sourceSystem = this.GetFeedSourceSystem(
                    feedAsset.AssetType,
                    entityType,
                    feedAttributes,
                    externalSystems);
                this.AddFeed(
                    feeds,
                    new Feed(
                        sourceSystem,
                        entityType,
                        feedAsset.AssetType,
                        feedAttributes));
            }
            this.VerifyEachEntityTypeHasMatchingInterfaceImplementation(
                feedAsset.AssetType, entityTypeFeedAttributes);
        }

        private FeedAttribute GetDefaultFeedAttribute(
            CombinedAsset<FeedAttribute> feedAsset)
        {
            FeedAttribute[] defaultAttributes =
                feedAsset
                .AssetAttributes
                .Where(fa => fa.EntityTypes.Count() == 0)
                .ToArray();
            if(defaultAttributes.Length > 1)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.FeedHasMoreThanOneDefaultAttribute,
                    feedAsset.AssetType.FullName));
            }
            return defaultAttributes.SingleOrDefault();
        }

        private IDictionary<Type, FeedAttribute> GetEntityTypeFeedAttributes(
            CombinedAsset<FeedAttribute> feedAsset)
        {
            var entityTypeFeedAttributes = new Dictionary<Type, FeedAttribute>();
            foreach(FeedAttribute feedAttribute in feedAsset.AssetAttributes)
            {
                foreach(Type entityType in feedAttribute.EntityTypes)
                {
                    if(!entityTypeFeedAttributes.ContainsKey(entityType))
                    {
                        entityTypeFeedAttributes.Add(entityType, feedAttribute);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(
                            Resources.DuplicateFeedEntityType,
                            feedAsset.AssetType.FullName,
                            entityType.FullName));
                    }
                }
            }
            return entityTypeFeedAttributes;
        }

        private IEnumerable<FeedAttribute> GetFeedAttributes(
            Type assetType,
            Type entityType,
            FeedAttribute defaultAttribute,
            IDictionary<Type, FeedAttribute> entityTypeFeedAttributes)
        {
            var feedAttributes = new List<FeedAttribute>();
            if(entityTypeFeedAttributes.TryGetValue(
                entityType, out FeedAttribute feedAttribute))
            {
                feedAttributes.Add(feedAttribute);
                entityTypeFeedAttributes.Remove(entityType);
            }
            if(defaultAttribute != null)
            {
                feedAttributes.Add(defaultAttribute);
            }
            if(feedAttributes.Count == 0)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.NoFeedAttributeForEntityType,
                    assetType.FullName,
                    entityType.FullName));
            }
            return feedAttributes;
        }

        private Type GetFeedSourceSystem(
            Type assetType,
            Type entityType,
            IEnumerable<FeedAttribute> feedAttributes,
            IDictionary<Type, ExternalSystemAttribute> externalSystems)
        {
            Type sourceSystem =
                feedAttributes
                .Where(fa => fa.SourceSystem != null)
                .Select(fa => fa.SourceSystem)
                .FirstOrDefault();
            if(sourceSystem == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.NoFeedSourceSystemForEntityType,
                    assetType.FullName,
                    entityType.FullName));
            }
            this.ValidateExternalSystem(sourceSystem, externalSystems);
            return sourceSystem;
        }

        private void VerifyEachEntityTypeHasMatchingInterfaceImplementation(
            Type feedType,
            IDictionary<Type, FeedAttribute> entityTypeFeedAttributes)
        {
            if(entityTypeFeedAttributes.Count != 0)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.FeedEntityTypeNotImplemented,
                    feedType.FullName));
            }
        }

        private void ProcessGenericFeed(
            HashSet<Feed> feeds,
            CombinedAsset<FeedAttribute> feedAsset,
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            EntityTypeSuite entityTypes)
        {
            FeedAttribute genericFeedAttribute =
                this.GetGenericFeedAttribute(feedAsset, externalSystems);
            foreach(Type entityType in genericFeedAttribute.EntityTypes)
            {
                this.ValidateEntityType(entityType, entityTypes);
                this.AddFeed(
                    feeds,
                    new Feed(
                        genericFeedAttribute.SourceSystem,
                        entityType,
                        feedAsset.AssetType,
                        new[] { genericFeedAttribute }));
            }
        }

        private FeedAttribute GetGenericFeedAttribute(
            CombinedAsset<FeedAttribute> feedAsset,
            IDictionary<Type, ExternalSystemAttribute> externalSystems)
        {
            FeedAttribute genericFeedAttribute = null;
            if(feedAsset.AssetAttributes.Count() == 1)
            {
                FeedAttribute singleAttribute =
                    feedAsset.AssetAttributes.Single();
                if(singleAttribute.SourceSystem != null &&
                    singleAttribute.EntityTypes.Count() > 0)
                {
                    this.ValidateExternalSystem(
                        singleAttribute.SourceSystem, externalSystems);
                    genericFeedAttribute = singleAttribute;
                }
            }
            if(genericFeedAttribute == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.InvalidGenericFeedAttribute,
                    feedAsset.AssetType.FullName));
            }
            return genericFeedAttribute;
        }

        private void AddFeed(HashSet<Feed> feeds, Feed feed)
        {
            if(!feeds.Add(feed))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.MultipleSourceSystemEntityTypeFeeds,
                    feed.SourceSystem.FullName,
                    feed.EntityType.FullName));
            }
        }

        private AssetSuite AssembleAssets(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            IDictionary<Type, SourceSystemParametersAttribute> sourceSystemParameters,
            EntityTypeSuite entityTypes,
            ConnectorSuite sinks,
            ConnectorSuite cacheFeeds,
            IEnumerable<Feed> feeds)
        {
            IDictionary<Type, ExternalSystemAsset> externalSystemAssetsByType =
                this.AssembleExternalSystems(
                    externalSystems,
                    sourceSystemParameters,
                    entityTypes,
                    sinks,
                    cacheFeeds);
            return new AssetSuite(
                externalSystemAssetsByType.Values,
                this.AssembleFeeds(feeds, externalSystemAssetsByType));
        }

        private IDictionary<Type, ExternalSystemAsset> AssembleExternalSystems(
            IDictionary<Type, ExternalSystemAttribute> externalSystems,
            IDictionary<Type, SourceSystemParametersAttribute> sourceSystemParameters,
            EntityTypeSuite entityTypes,
            ConnectorSuite sinks,
            ConnectorSuite cacheFeeds)
        {
            var externalSystemAssetsByType = new Dictionary<Type, ExternalSystemAsset>();
            foreach(Type externalSystem in externalSystems.Keys)
            {
                externalSystemAssetsByType.Add(
                    externalSystem,
                    new ExternalSystemAsset(
                        externalSystem,
                        externalSystems[externalSystem],
                        this.Get(externalSystem, sourceSystemParameters),
                        this.AssembleEntityTypes(
                            externalSystem,
                            entityTypes,
                            sinks,
                            cacheFeeds,
                            externalSystemAssetsByType)));
            }
            return externalSystemAssetsByType;
        }

        private IEnumerable<EntityTypeAsset> AssembleEntityTypes(
            Type externalSystem,
            EntityTypeSuite entityTypes,
            ConnectorSuite sinks,
            ConnectorSuite cacheFeeds,
            IDictionary<Type, ExternalSystemAsset> externalSystemAssetsByType)
        {
            return
                entityTypes
                .DestinationSystemEntityTypes[externalSystem]
                .Keys
                .Select(entityType => new EntityTypeAsset(
                    entityType,
                    entityTypes
                        .DestinationSystemEntityTypes[externalSystem][entityType]
                        .Attribute,
                    new SharedIdentifierSourceSystemsProvider(
                        externalSystemAssetsByType,
                        entityTypes
                            .DestinationSystemEntityTypes[externalSystem][entityType]
                            .SharedSourceSystemIdentifiersAttributes),
                    this.GetEntityTypeSink(entityType, externalSystem, sinks),
                    this.GetEntityTypeCacheFeed(entityType, externalSystem, cacheFeeds)))
                .ToArray();
        }

        private Type GetEntityTypeSink(
            Type entityType, Type externalSystem, ConnectorSuite sinks)
        {
            return this.GetConnector(
                entityType,
                externalSystem,
                sinks,
                Resources.NoSinkExistsForEntityType);
        }

        private Type GetEntityTypeCacheFeed(
            Type entityType, Type externalSystem, ConnectorSuite cacheFeeds)
        {
            return this.GetConnector(entityType, externalSystem, cacheFeeds);
        }

        private Type GetConnector(
            Type entityType,
            Type externalSystem,
            ConnectorSuite connectors,
            string notFoundExceptionMessageFormat = null)
        {
            Type connector;
            if(connectors.EntityTypeConnectors.ContainsKey(entityType))
            {
                connector = connectors.EntityTypeConnectors[entityType];
            }
            else if(connectors.DestinationSystemConnectors.ContainsKey(externalSystem))
            {
                connector = connectors.DestinationSystemConnectors[externalSystem];
            }
            else if(notFoundExceptionMessageFormat != null)
            {
                throw new InvalidOperationException(string.Format(
                    notFoundExceptionMessageFormat, entityType.FullName));
            }
            else
            {
                connector = null;
            }
            return connector;
        }

        private IEnumerable<FeedAsset> AssembleFeeds(
            IEnumerable<Feed> feeds,
            IDictionary<Type, ExternalSystemAsset> externalSystemAssetsByType)
        {
            IDictionary<Type, EntityTypeAsset> entityTypes =
                externalSystemAssetsByType
                .Values
                .SelectMany(esa => esa.EntityTypes)
                .ToDictionary(et => et.EntityType);
            return
                feeds
                .Select(f => new FeedAsset(
                    f.FeedType,
                    entityTypes[f.EntityType],
                    externalSystemAssetsByType[f.SourceSystem],
                    f.Attributes))
                .ToArray();
        }

        private void ValidateAssetSuite(AssetSuite assetSuite)
        {
            this.ValidateExternalSystems(assetSuite);
            this.ValidateEntityTypes(assetSuite);
        }

        private void ValidateExternalSystems(AssetSuite assetSuite)
        {
            this.ValidateAssets(
                assetSuite.ExternalSystems,
                es => es.Id,
                es => es.Name,
                es => es.ExternalSystemType,
                MaxExternalSystemNameLength,
                Resources.ExternalSystemIdNotUnique,
                Resources.ExternalSystemNameWhiteSpace,
                Resources.ExternalSystemNameTooLong,
                Resources.ExternalSystemNameNotUnique);
        }

        private void ValidateEntityTypes(AssetSuite assetSuite)
        {
            this.ValidateAssets(
                assetSuite.ExternalSystems.SelectMany(es => es.EntityTypes),
                et => et.Id,
                et => et.Name,
                et => et.EntityType,
                MaxEntityTypeNameLength,
                Resources.EntityTypeIdNotUnique,
                Resources.EntityTypeNameWhiteSpace,
                Resources.EntityTypeNameTooLong,
                Resources.EntityTypeNameNotUnique);
        }

        private void ValidateAssets<TAsset>(
            IEnumerable<TAsset> assets,
            Func<TAsset, Guid> getAssetId,
            Func<TAsset, string> getAssetName,
            Func<TAsset, Type> getAssetType,
            int maxLength,
            string idNotUniqueExceptionMessageFormat,
            string whiteSpaceNameExceptionMessageFormat,
            string nameTooLongExceptionMessageFormat,
            string nameNotUniqueExceptionMessageFormat)
        {
            var ids = new HashSet<Guid>();
            var names = new HashSet<string>();
            foreach(TAsset asset in assets)
            {
                Guid id = getAssetId(asset);
                string name = getAssetName(asset);
                Type assetType = getAssetType(asset);
                if(!ids.Add(id))
                {
                    throw new InvalidOperationException(string.Format(
                        idNotUniqueExceptionMessageFormat, id));
                }
                if(string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidOperationException(string.Format(
                        whiteSpaceNameExceptionMessageFormat,
                        assetType.FullName));
                }
                if(name.Length > maxLength)
                {
                    throw new InvalidOperationException(string.Format(
                        nameTooLongExceptionMessageFormat,
                        name,
                        assetType.FullName,
                        maxLength));
                }
                if(!names.Add(name))
                {
                    throw new InvalidOperationException(string.Format(
                        nameNotUniqueExceptionMessageFormat,
                        name));
                }
            }
        }

        private bool IsNonGeneric(Type type)
        {
            return this.HasGenericArguments(type, 0);
        }

        private bool HasOneGenericArgument(Type type)
        {
            return this.HasGenericArguments(type, 1);
        }

        private bool HasGenericArguments(Type type, int count)
        {
            return type.GetGenericArguments().Length == count;
        }

        private IEnumerable<Type> GetInterfaceImplementations(
            Type type, Type genericInterfaceType)
        {
            Type[] interfaceImplementations =
                type
                .GetInterfaces()
                .Where(it =>
                    it.IsGenericType &&
                    it.GetGenericTypeDefinition() == genericInterfaceType)
                .ToArray();
            if(interfaceImplementations.Length == 0)
            {
                this.ThrowTypeDoesNotImplementInterfaceException(
                    type, genericInterfaceType);
            }
            return interfaceImplementations;
        }

        private Type ExtractEntityType(Type interfaceImplementation)
        {
            return interfaceImplementation.GetGenericArguments().Single();
        }

        private bool ImplementsGenericInterface(Type type, Type genericInterfaceType)
        {
            return
                type
                .GetInterfaces()
                .Where(it =>
                    it == genericInterfaceType.MakeGenericType(
                        type.GetGenericArguments().Single()))
                .Count() == 1;
        }

        private void ValidateExternalSystem(
            Type externalSystem,
            IDictionary<Type, ExternalSystemAttribute> externalSystems)
        {
            if(!externalSystems.ContainsKey(externalSystem))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.TypeDoesNotRepresentExternalSystem,
                    externalSystem.FullName));
            }
        }

        private void ValidateEntityType(Type entityType, EntityTypeSuite entityTypes)
        {
            if(!entityTypes.AllEntityTypes.Contains(entityType))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.TypeDoesNotRepresentEntityType,
                    entityType.FullName));
            }
        }

        private void ThrowTypeDoesNotImplementInterfaceException(
            Type type, Type genericInterfaceType)
        {
            throw new InvalidOperationException(string.Format(
                Resources.TypeDoesNotImplementInterface,
                type.FullName,
                genericInterfaceType.FullName));
        }

        private void ThrowInterfaceImplementedIllegaly(
            Type type, Type genericInterfaceType)
        {
            throw new InvalidOperationException(string.Format(
                Resources.InterfaceImplementedIllegally,
                type.FullName,
                genericInterfaceType.FullName));
        }

        private TValue Get<TKey, TValue>(TKey key, IDictionary<TKey, TValue> dictionary)
        {
            dictionary.TryGetValue(key, out TValue value);
            return value;
        }

        private class EntityTypeWrapper
        {
            public EntityTypeWrapper(
                EntityTypeAttributeBase attribute,
                IEnumerable<SharedSourceSystemIdentifiersAttribute>
                    sharedSourceSystemIdentifiersAttributes)
            {
                this.Attribute = attribute;
                this.SharedSourceSystemIdentifiersAttributes
                    = sharedSourceSystemIdentifiersAttributes;
            }

            public EntityTypeAttributeBase Attribute { get; }

            public IEnumerable<SharedSourceSystemIdentifiersAttribute>
                SharedSourceSystemIdentifiersAttributes
            { get; }
        }

        private class EntityTypeSuite
        {
            public EntityTypeSuite(
                IDictionary<Type, IDictionary<Type, EntityTypeWrapper>>
                    destinationSystemEntityTypes,
                HashSet<Type> allEntityTypes)
            {
                this.DestinationSystemEntityTypes = destinationSystemEntityTypes;
                this.AllEntityTypes = allEntityTypes;
            }

            public IDictionary<Type, IDictionary<Type, EntityTypeWrapper>>
                DestinationSystemEntityTypes
            { get; }

            public HashSet<Type> AllEntityTypes { get; }
        }

        private class ConnectorSuite
        {
            public ConnectorSuite(
                IDictionary<Type, Type> destinationSystemConnectors,
                IDictionary<Type, Type> entityTypeConnectors)
            {
                this.DestinationSystemConnectors = destinationSystemConnectors;
                this.EntityTypeConnectors = entityTypeConnectors;
            }

            public IDictionary<Type, Type> DestinationSystemConnectors { get; }

            public IDictionary<Type, Type> EntityTypeConnectors { get; }
        }

        private class Feed
        {
            public Feed(
                Type sourceSystem,
                Type entityType,
                Type feedType,
                IEnumerable<FeedAttribute> attributes)
            {
                this.SourceSystem = sourceSystem;
                this.EntityType = entityType;
                this.FeedType = feedType;
                this.Attributes = attributes;
            }

            public Type SourceSystem { get; }

            public Type EntityType { get; }

            public Type FeedType { get; }

            public IEnumerable<FeedAttribute> Attributes { get; }

            public override int GetHashCode()
            {
                return
                    $"{this.SourceSystem.FullName}_{this.EntityType.FullName}"
                    .GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = (Feed) obj;
                return
                    this.SourceSystem == other.SourceSystem &&
                    this.EntityType == other.EntityType;
            }
        }

        private class SharedIdentifierSourceSystemsProvider :
            ISharedIdentifierSourceSystemsProvider
        {
            private readonly IDictionary<
                Type, ExternalSystemAsset> externalSystemAssetsByType;

            private readonly IEnumerable<SharedSourceSystemIdentifiersAttribute>
                sharedSourceSystemIdentifiersAttributes;

            public SharedIdentifierSourceSystemsProvider(
                IDictionary<Type, ExternalSystemAsset> externalSystemAssetsByType,
                IEnumerable<SharedSourceSystemIdentifiersAttribute>
                    sharedSourceSystemIdentifiersAttributes)
            {
                this.externalSystemAssetsByType = externalSystemAssetsByType;
                this.sharedSourceSystemIdentifiersAttributes
                    = sharedSourceSystemIdentifiersAttributes;
            }

            public IEnumerable<IEnumerable<ExternalSystemAsset>>
                GetSharedIdentifierSourceSystems()
            {
                return
                    this
                    .sharedSourceSystemIdentifiersAttributes
                    .Select(sssia =>
                        sssia
                        .SourceSystemTypes
                        .Select(sst => this.externalSystemAssetsByType[sst])
                        .ToArray())
                    .ToArray();
            }
        }
    }
}
