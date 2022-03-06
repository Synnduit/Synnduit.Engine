using Synnduit.Configuration;
using Synnduit.Persistence;
using Synnduit.Properties;
using System.Reflection;
using System.Transactions;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Executes individual deployment steps to ensure that the database is ready for
    /// migration runs.
    /// </summary>
    internal class DeploymentExecutor : IDeploymentExecutor
    {
        private const int MaxParameterNameLength = 128;

        private const int MaxParameterValueLength = 1024;

        private readonly IConfigurationProvider configurationProvider;

        private readonly IBootstrapperFactory bootstrapperFactory;

        private readonly IEnumerable<Assembly> explicitAssemblies;

        public DeploymentExecutor(
            IConfigurationProvider configurationProvider,
            IBootstrapperFactory bootstrapperFactory,
            IEnumerable<Assembly> explicitAssemblies)
        {
            this.configurationProvider = configurationProvider;
            this.bootstrapperFactory = bootstrapperFactory;
            this.explicitAssemblies = explicitAssemblies;
        }

        /// <summary>
        /// Executes individual deployment steps to ensure that the database is ready for
        /// migration runs.
        /// </summary>
        public void Deploy()
        {
            using IBootstrapper bootstrapper = this.bootstrapperFactory.CreateBootstrapper(
                this.configurationProvider, this.explicitAssemblies);
            var context = new DeploymentContext();
            this.ExecuteDeploymentSteps(bootstrapper, context);
            context.Validate();
            this.CreateOrUpdateAssets(bootstrapper, context);
        }

        private void ExecuteDeploymentSteps(
            IBootstrapper bootstrapper, DeploymentContext context)
        {
            foreach (IDeploymentStep deploymentStep
                in bootstrapper.GetMany<IDeploymentStep>())
            {
                try
                {
                    deploymentStep.Execute(context);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        Resources.DeploymentStepFailed, exception);
                }
            }
        }

        private void CreateOrUpdateAssets(
            IBootstrapper bootstrapper, DeploymentContext context)
        {
            using ISafeRepository safeRepository = bootstrapper.Get<ISafeRepository>();
            using var scope = new TransactionScope();
            this.CreateOrUpdateExternalSystems(
                context.ExternalSystems, safeRepository);
            this.CreateOrUpdateEntityTypes(
                context.EntityTypes, safeRepository);
            this.CreateSharedSourceSystemIdentifierGroups(
                context.SharedSourceSystemIdentifierGroups, safeRepository);
            this.CreateOrUpdateFeeds(
                context.Feeds, safeRepository);
            this.CreateOrUpdateParameters(context, safeRepository);
            scope.Complete();
        }

        private void CreateOrUpdateExternalSystems(
            IEnumerable<ExternalSystem> externalSystems,
            ISafeRepository safeRepository)
        {
            foreach (ExternalSystem externalSystem in externalSystems)
            {
                safeRepository.CreateOrUpdateExternalSystem(externalSystem);
            }
        }

        private void CreateOrUpdateEntityTypes(
            IEnumerable<EntityType> entityTypes,
            ISafeRepository safeRepository)
        {
            foreach (EntityType entityType in entityTypes)
            {
                safeRepository.CreateOrUpdateEntityType(entityType);
            }
        }

        private void CreateSharedSourceSystemIdentifierGroups(
            IEnumerable<SharedSourceSystemIdentifierGroup> groups,
            ISafeRepository safeRepository)
        {
            foreach (IGrouping<Guid, SharedSourceSystemIdentifierGroup>
                entityTypeGroups in groups.GroupBy(group => group.EntityTypeId))
            {
                safeRepository.ClearSharedSourceSystemIdentifiers(entityTypeGroups.Key);
                int groupNumber = 1;
                foreach (SharedSourceSystemIdentifierGroup group in entityTypeGroups)
                {
                    this.CreateSharedSourceSystemIdentifierGroup(
                        group, groupNumber++, safeRepository);
                }
            }
        }

        private void CreateSharedSourceSystemIdentifierGroup(
            SharedSourceSystemIdentifierGroup group,
            int groupNumber,
            ISafeRepository safeRepository)
        {
            foreach (Guid sourceSystemId in group.SourceSystemIds)
            {
                safeRepository.CreateSharedSourceSystemIdentifier(
                    new SharedSourceSystemIdentifier(
                        group.EntityTypeId, sourceSystemId, groupNumber));
            }
        }

        private void CreateOrUpdateFeeds(
            IEnumerable<Feed> feeds,
            ISafeRepository safeRepository)
        {
            foreach (Feed feed in feeds)
            {
                safeRepository.CreateOrUpdateFeed(feed);
            }
        }

        private void CreateOrUpdateParameters(
            DeploymentContext context, ISafeRepository safeRepository)
        {
            IEnumerable<ParameterScope>
                parameterScopes = this.GetParameterScopes(context);
            IDictionary<ParameterScope, Dictionary<string, IParameter>>
                existingParametersByScope =
                    this.GetExistingParametersByScope(safeRepository);
            foreach (ParameterScope scope in parameterScopes)
            {
                IDictionary<string, IParameter> existingParameters =
                    this.GetExistingParameters(existingParametersByScope, scope);
                IDictionary<string, string>
                    parameters = this.GetParameters(context, scope);
                this.ProcessParameters(
                    scope,
                    parameters,
                    existingParameters,
                    safeRepository);
                this.DeleteOrphanParameters(existingParameters.Values, safeRepository);
            }
        }

        private IEnumerable<ParameterScope> GetParameterScopes(DeploymentContext context)
        {
            var parameterScopes = new List<ParameterScope>();
            parameterScopes.AddRange(
                context
                .ExternalSystems
                .Select(destinationSystem =>
                    new ParameterScope(destinationSystem.Id, null, null)));
            parameterScopes.AddRange(
                context
                .EntityTypes
                .Select(entityType => new ParameterScope(null, entityType.Id, null)));
            parameterScopes.AddRange(
                context
                .ExternalSystems
                .Select(sourceSystem => new ParameterScope(null, null, sourceSystem.Id)));
            parameterScopes.AddRange(
                from entityType in context.EntityTypes
                from sourceSystem in context.ExternalSystems
                select new ParameterScope(null, entityType.Id, sourceSystem.Id));
            return parameterScopes;
        }

        private IDictionary<ParameterScope, Dictionary<string, IParameter>>
            GetExistingParametersByScope(ISafeRepository safeRepository)
        {
            return
                safeRepository
                .GetParameters()
                .GroupBy(parameter => new ParameterScope(
                    parameter.DestinationSystemId,
                    parameter.EntityTypeId,
                    parameter.SourceSystemId))
                .ToDictionary(
                    parameters => parameters.Key,
                    parameters => parameters.ToDictionary(
                        parameter => parameter.Name));
        }

        private IDictionary<string, IParameter> GetExistingParameters(
            IDictionary<
                ParameterScope,
                Dictionary<string, IParameter>> existingParametersByScope,
            ParameterScope scope)
        {
            if (existingParametersByScope.TryGetValue(
                scope, out Dictionary<string, IParameter> existingParameters) == false)
            {
                existingParameters = new Dictionary<string, IParameter>();
            }
            return existingParameters;
        }

        private IDictionary<string, string> GetParameters(
            DeploymentContext context, ParameterScope scope)
        {
            if (context.Parameters.TryGetValue(
                scope, out IDictionary<string, string> parameters) == false)
            {
                parameters = new Dictionary<string, string>();
            }
            return parameters;
        }

        private void ProcessParameters(
            ParameterScope scope,
            IDictionary<string, string> parameters,
            IDictionary<string, IParameter> existingParameters,
            ISafeRepository safeRepository)
        {
            foreach (string name in parameters.Keys)
            {
                if (existingParameters.ContainsKey(name))
                {
                    if (existingParameters[name].Value != parameters[name])
                    {
                        safeRepository.SetParameterValue(
                            existingParameters[name].Id, parameters[name]);
                    }
                    existingParameters.Remove(name);
                }
                else
                {
                    safeRepository.CreateParameter(
                        new Parameter(
                            Guid.NewGuid(),
                            scope.DestinationSystemId,
                            scope.EntityTypeId,
                            scope.SourceSystemId,
                            name,
                            parameters[name]));
                }
            }
        }

        private void DeleteOrphanParameters(
            IEnumerable<IParameter> parameters, ISafeRepository safeRepository)
        {
            foreach (IParameter parameter in parameters)
            {
                safeRepository.DeleteParameter(parameter.Id);
            }
        }

        private class DeploymentContext : IDeploymentContext
        {
            private readonly Dictionary<Guid, ExternalSystem> externalSystems;

            private readonly Dictionary<Guid, EntityType> entityTypes;

            private readonly List<
                SharedSourceSystemIdentifierGroup> sharedSourceSystemIdentifierGroups;

            private readonly Dictionary<Feed, Feed> feeds;

            private readonly Dictionary<
                ParameterScope, IDictionary<string, string>> parameters;

            public DeploymentContext()
            {
                this.externalSystems = new Dictionary<Guid, ExternalSystem>();
                this.entityTypes = new Dictionary<Guid, EntityType>();
                this.sharedSourceSystemIdentifierGroups =
                    new List<SharedSourceSystemIdentifierGroup>();
                this.feeds = new Dictionary<Feed, Feed>();
                this.parameters = new Dictionary<
                    ParameterScope, IDictionary<string, string>>();
            }

            public IEnumerable<ExternalSystem> ExternalSystems
            {
                get { return this.externalSystems.Values; }
            }

            public IEnumerable<EntityType> EntityTypes
            {
                get { return this.entityTypes.Values; }
            }

            public IEnumerable<
                SharedSourceSystemIdentifierGroup> SharedSourceSystemIdentifierGroups
            {
                get { return this.sharedSourceSystemIdentifierGroups; }
            }

            public IEnumerable<Feed> Feeds
            {
                get { return this.feeds.Values; }
            }

            public IDictionary<ParameterScope, IDictionary<string, string>> Parameters
            {
                get { return this.parameters; }
            }

            public void Validate()
            {
                this.EnsureExternalSystemsExist(
                    this
                    .EntityTypes
                    .Select(entityType => entityType.DestinationSystemId));
                this.EnsureEntityTypesExist(
                    this
                    .Feeds
                    .Select(feed => feed.EntityTypeId));
                this.EnsureExternalSystemsExist(
                    this
                    .Feeds
                    .Select(feed => feed.SourceSystemId));
                this.EnsureNamesUnique(
                    this
                    .ExternalSystems
                    .Select(externalSystem => externalSystem.Name),
                    Resources.ExternalSystemNamesMustBeUnique);
                this.EnsureNamesUnique(
                    this
                    .EntityTypes
                    .Select(entityType => entityType.Name),
                    Resources.EntityTypeNamesMustBeUnique);
                this.EnsureEntityTypesExist(
                    this
                    .SharedSourceSystemIdentifierGroups
                    .Select(group => group.EntityTypeId));
                this.EnsureExternalSystemsExist(
                    this
                    .SharedSourceSystemIdentifierGroups
                    .SelectMany(group => group.SourceSystemIds));
                this.EnsureSourceSystemIdsUnique(
                    this
                    .SharedSourceSystemIdentifierGroups
                    .Select(group => group.SourceSystemIds));
                this.EnsureEntityTypeSourceSystemCombinationsUnique(
                    this
                    .SharedSourceSystemIdentifierGroups
                    .SelectMany(group =>
                        group
                        .SourceSystemIds
                        .Select(sourceSystemId => new EntityTypeSourceSystemCombination(
                            group.EntityTypeId, sourceSystemId))));
                this.EnsureParameterScopeReferencesExist(
                    parameterScope => parameterScope.DestinationSystemId,
                    this.EnsureExternalSystemsExist);
                this.EnsureParameterScopeReferencesExist(
                    parameterScope => parameterScope.EntityTypeId,
                    this.EnsureEntityTypesExist);
                this.EnsureParameterScopeReferencesExist(
                    parameterScope => parameterScope.SourceSystemId,
                    this.EnsureExternalSystemsExist);
            }

            public void ExternalSystem(Guid id, string name)
            {
                this.externalSystems[id] = new ExternalSystem(id, name);
            }

            public void EntityType(
                Guid id,
                Guid destinationSystemId,
                string name,
                Type type,
                Type sinkType,
                Type cacheFeedType,
                bool isMutable,
                bool isDuplicable)
            {
                this.entityTypes[id] = new EntityType(
                    id,
                    destinationSystemId,
                    name,
                    type,
                    sinkType,
                    cacheFeedType,
                    isMutable,
                    isDuplicable);
            }

            public void SharedSourceSystemIdentifiers(
                Guid entityTypeId, params Guid[] sourceSystemIds)
            {
                this.sharedSourceSystemIdentifierGroups.Add(
                    new SharedSourceSystemIdentifierGroup(entityTypeId, sourceSystemIds));
            }

            public void Feed(
                Guid entityTypeId,
                Guid sourceSystemId,
                Type feedType)
            {
                var feed = new Feed(entityTypeId, sourceSystemId, feedType);
                this.feeds[feed] = feed;
            }

            public void DestinationSystemParameter(
                Guid destinationSystemId,
                string name,
                string value)
            {
                this.SetParameterValue(
                    destinationSystemId,
                    null,
                    null,
                    name,
                    value);
            }

            public void EntityTypeParameter(
                Guid entityTypeId,
                string name,
                string value)
            {
                this.SetParameterValue(
                    null,
                    entityTypeId,
                    null,
                    name,
                    value);
            }

            public void SourceSystemParameter(
                Guid sourceSystemId,
                string name,
                string value)
            {
                this.SetParameterValue(
                    null,
                    null,
                    sourceSystemId,
                    name,
                    value);
            }

            public void EntityTypeSourceSystemParameter(
                Guid entityTypeId,
                Guid sourceSystemId,
                string name,
                string value)
            {
                this.SetParameterValue(
                    null,
                    entityTypeId,
                    sourceSystemId,
                    name,
                    value);
            }

            private void EnsureExternalSystemsExist(IEnumerable<Guid> ids)
            {
                this.EnsureIdsExist(
                    ids,
                    this.externalSystems,
                    Resources.ExternalSystemIdNotFound);
            }

            private void EnsureEntityTypesExist(IEnumerable<Guid> ids)
            {
                this.EnsureIdsExist(
                    ids,
                    this.entityTypes,
                    Resources.EntityTypeIdNotFound);
            }

            private void EnsureIdsExist<T>(
                IEnumerable<Guid> ids,
                Dictionary<Guid, T> dictionary,
                string exceptionMessageFormat)
            {
                foreach (Guid id in ids)
                {
                    if (dictionary.ContainsKey(id) == false)
                    {
                        throw new InvalidOperationException(
                            string.Format(exceptionMessageFormat, id));
                    }
                }
            }

            private void EnsureSourceSystemIdsUnique(
                IEnumerable<IEnumerable<Guid>> groupSourceSystemIds)
            {
                foreach (IEnumerable<Guid> sourceSystemIds in groupSourceSystemIds)
                {
                    this.EnsureValuesUnique(
                        sourceSystemIds,
                        Resources.SharedIdentifierSourceSystemIdsMustBeUnique);
                }
            }

            private void EnsureEntityTypeSourceSystemCombinationsUnique(
                IEnumerable<EntityTypeSourceSystemCombination> combinations)
            {
                var usedCombinations = new HashSet<EntityTypeSourceSystemCombination>();
                foreach (EntityTypeSourceSystemCombination combination in combinations)
                {
                    if (usedCombinations.Add(combination) == false)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                Resources.SourceSystemCanOnlyShareIdentifiersWithOneGroup,
                                combination.EntityTypeId,
                                combination.SourceSystemId));
                    }
                }
            }

            private void EnsureNamesUnique(
                IEnumerable<string> names, string exceptionMessage)
            {
                this.EnsureValuesUnique(
                    names.Select(name => name.ToLower()),
                    exceptionMessage);
            }

            private void EnsureValuesUnique<TValue>(
                IEnumerable<TValue> values, string exceptionMessage)
            {
                if (values.Distinct().Count() < values.Count())
                {
                    throw new InvalidOperationException(exceptionMessage);
                }
            }

            private void EnsureParameterScopeReferencesExist(
                Func<ParameterScope, Guid?> getReferenceId,
                Action<IEnumerable<Guid>> ensureReferencesExist)
            {
                ensureReferencesExist(
                    this
                    .parameters
                    .Keys
                    .Select(getReferenceId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value));
            }

            private void SetParameterValue(
                Guid? destinationSystemId,
                Guid? entityTypeId,
                Guid? sourceSystemId,
                string name,
                string value)
            {
                this.ValidateParameterArguments(
                    destinationSystemId,
                    entityTypeId,
                    sourceSystemId,
                    name,
                    value);
                var scope = new ParameterScope(
                    destinationSystemId, entityTypeId, sourceSystemId);
                if (this.parameters.ContainsKey(scope) == false)
                {
                    this.parameters[scope] = new Dictionary<string, string>();
                }
                parameters[scope][name] = value;
            }

            private void ValidateParameterArguments(
                Guid? destinationSystemId,
                Guid? entityTypeId,
                Guid? sourceSystemId,
                string name,
                string value)
            {
                this.EnsureNotEmptyIfNotNull(
                    destinationSystemId, nameof(destinationSystemId));
                this.EnsureNotEmptyIfNotNull(entityTypeId, nameof(entityTypeId));
                this.EnsureNotEmptyIfNotNull(sourceSystemId, nameof(sourceSystemId));
                ArgumentValidator.EnsureArgumentNotNullOrWhiteSpace(name, nameof(name));
                ArgumentValidator.EnsureArgumentNotNullOrWhiteSpace(value, nameof(value));
                ArgumentValidator.EnsureArgumentDoesNotExceedMaxLength(
                    name, MaxParameterNameLength, nameof(name));
                ArgumentValidator.EnsureArgumentDoesNotExceedMaxLength(
                    value, MaxParameterValueLength, nameof(value));
            }

            private void EnsureNotEmptyIfNotNull(Guid? argument, string paramName)
            {
                if (argument.HasValue)
                {
                    ArgumentValidator.EnsureArgumentNotEmpty(argument.Value, paramName);
                }
            }

            private class EntityTypeSourceSystemCombination
            {
                public EntityTypeSourceSystemCombination(
                    Guid entityTypeId, Guid sourceSystemId)
                {
                    this.EntityTypeId = entityTypeId;
                    this.SourceSystemId = sourceSystemId;
                }

                public Guid EntityTypeId { get; }

                public Guid SourceSystemId { get; }

                public override int GetHashCode()
                {
                    return string.Format(
                        "{0}_{1}", this.EntityTypeId, this.SourceSystemId)
                        .GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    var other = (EntityTypeSourceSystemCombination)obj;
                    return this.EntityTypeId == other.EntityTypeId
                        && this.SourceSystemId == other.SourceSystemId;
                }
            }
        }

        private class ExternalSystem : Persistence.IExternalSystem
        {
            private const int MaxNameLength = 128;

            public ExternalSystem(Guid id, string name)
            {
                // validate parameters
                ArgumentValidator.EnsureArgumentNotEmpty(id, nameof(id));
                ArgumentValidator.EnsureArgumentNotNullOrWhiteSpace(name, nameof(name));
                string trimmedName = name.Trim();
                ArgumentValidator.EnsureArgumentDoesNotExceedMaxLength(
                    trimmedName, MaxNameLength, nameof(name));

                // set property values
                this.Id = id;
                this.Name = trimmedName;
            }

            public Guid Id { get; }

            public string Name { get; }
        }

        private class EntityType : Persistence.IEntityType
        {
            private const int MaxNameLength = 128;

            public EntityType(
                Guid id,
                Guid destinationSystemId,
                string name,
                Type type,
                Type sinkType,
                Type cacheFeedType,
                bool isMutable,
                bool isDuplicable)
            {
                // validate parameters
                ArgumentValidator.EnsureArgumentNotEmpty(id, nameof(id));
                ArgumentValidator.EnsureArgumentNotEmpty(
                    destinationSystemId, nameof(destinationSystemId));
                ArgumentValidator.EnsureArgumentNotNullOrWhiteSpace(name, nameof(name));
                string trimmedName = name.Trim();
                ArgumentValidator.EnsureArgumentDoesNotExceedMaxLength(
                    trimmedName, MaxNameLength, nameof(name));
                ArgumentValidator.EnsureArgumentNotNull(type, nameof(type));
                ArgumentValidator.EnsureArgumentNotNull(sinkType, nameof(sinkType));

                // set property values
                this.Id = id;
                this.DestinationSystemId = destinationSystemId;
                this.Name = trimmedName;
                this.TypeName = type.AssemblyQualifiedName;
                this.SinkTypeFullName = sinkType.GetStrippedFullName();
                this.CacheFeedTypeFullName = cacheFeedType?.GetStrippedFullName();
                this.IsMutable = isMutable;
                this.IsDuplicable = isDuplicable;
            }

            public Guid Id { get; }

            public Guid DestinationSystemId { get; }

            public string Name { get; }

            public string TypeName { get; }

            public string SinkTypeFullName { get; }

            public string CacheFeedTypeFullName { get; }

            public bool IsMutable { get; }

            public bool IsDuplicable { get; }
        }

        private class SharedSourceSystemIdentifier : ISharedSourceSystemIdentifier
        {
            public SharedSourceSystemIdentifier(
                Guid entityTypeId, Guid sourceSystemId, int groupNumber)
            {
                this.EntityTypeId = entityTypeId;
                this.SourceSystemId = sourceSystemId;
                this.GroupNumber = groupNumber;
            }

            public Guid EntityTypeId { get; }

            public Guid SourceSystemId { get; }

            public int GroupNumber { get; }
        }

        private class SharedSourceSystemIdentifierGroup
        {
            private const int MinSharedSourceSystemCount = 2;

            public SharedSourceSystemIdentifierGroup(
                Guid entityTypeId, IEnumerable<Guid> sourceSystemIds)
            {
                // validate parameters
                ArgumentValidator.EnsureArgumentNotEmpty(
                    entityTypeId, nameof(entityTypeId));
                ArgumentValidator.EnsureArgumentNotNull(
                    sourceSystemIds, nameof(sourceSystemIds));
                if (sourceSystemIds.Count() < MinSharedSourceSystemCount)
                {
                    throw new ArgumentException(
                        Resources.TooFewSourceSystemsInGroup, nameof(sourceSystemIds));
                }
                foreach (Guid sourceSystemId in sourceSystemIds)
                {
                    ArgumentValidator.EnsureArgumentNotEmpty(
                        sourceSystemId, nameof(sourceSystemIds));
                }

                // set property values
                this.EntityTypeId = entityTypeId;
                this.SourceSystemIds = sourceSystemIds;
            }

            public Guid EntityTypeId { get; }

            public IEnumerable<Guid> SourceSystemIds { get; }
        }

        private class Feed : IFeed
        {
            public Feed(
                Guid entityTypeId,
                Guid sourceSystemId,
                Type feedType)
            {
                // validate parameters
                ArgumentValidator.EnsureArgumentNotEmpty(
                    entityTypeId, nameof(entityTypeId));
                ArgumentValidator.EnsureArgumentNotEmpty(
                    sourceSystemId, nameof(sourceSystemId));
                ArgumentValidator.EnsureArgumentNotNull(feedType, nameof(feedType));

                // set property values
                this.EntityTypeId = entityTypeId;
                this.SourceSystemId = sourceSystemId;
                this.FeedTypeFullName = feedType.GetStrippedFullName();
            }

            public Guid EntityTypeId { get; }

            public Guid SourceSystemId { get; }

            public string FeedTypeFullName { get; }

            public override int GetHashCode()
            {
                return $"{this.EntityTypeId}_{this.SourceSystemId}".GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = (Feed)obj;
                return this.EntityTypeId == other.EntityTypeId
                    && this.SourceSystemId == other.SourceSystemId;
            }
        }

        private class ParameterScope
        {
            public ParameterScope(
                Guid? destinationSystemId,
                Guid? entityTypeId,
                Guid? sourceSystemId)
            {
                this.DestinationSystemId = destinationSystemId;
                this.EntityTypeId = entityTypeId;
                this.SourceSystemId = sourceSystemId;
            }

            public Guid? DestinationSystemId { get; }

            public Guid? EntityTypeId { get; }

            public Guid? SourceSystemId { get; }

            public override int GetHashCode()
            {
                return
                    string.Format(
                        "{0:d}_{1:d}_{2:d}",
                        this.DestinationSystemId.GetValueOrDefault(),
                        this.EntityTypeId.GetValueOrDefault(),
                        this.SourceSystemId.GetValueOrDefault())
                    .GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = (ParameterScope)obj;
                return
                    this.DestinationSystemId == other.DestinationSystemId &&
                    this.EntityTypeId == other.EntityTypeId &&
                    this.SourceSystemId == other.SourceSystemId;
            }
        }

        private class Parameter : IParameter
        {
            public Parameter(
                Guid id,
                Guid? destinationSystemId,
                Guid? entityTypeId,
                Guid? sourceSystemId,
                string name,
                string value)
            {
                this.Id = id;
                this.DestinationSystemId = destinationSystemId;
                this.EntityTypeId = entityTypeId;
                this.SourceSystemId = sourceSystemId;
                this.Name = name;
                this.Value = value;
            }

            public Guid Id { get; }

            public Guid? DestinationSystemId { get; }

            public Guid? EntityTypeId { get; }

            public Guid? SourceSystemId { get; }

            public string Name { get; }

            public string Value { get; }
        }
    }
}
