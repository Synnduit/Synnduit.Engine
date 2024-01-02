using Synnduit.Configuration;
using Synnduit.Events;
using Synnduit.Persistence;
using Synnduit.Properties;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Performs individual runs.
    /// </summary>
    internal class Runner : IRunner
    {
        private readonly IConfigurationProvider configurationProvider;

        private readonly IBootstrapperFactory bootstrapperFactory;

        private readonly IBridgeFactory bridgeFactory;

        private readonly string runName;

        private readonly IEnumerable<Assembly> explicitAssemblies;

        private readonly IDictionary<string, object> runData;

        private ContextFactory contextFactory;

        public Runner(
            IConfigurationProvider configurationProvider,
            IBootstrapperFactory bootstrapperFactory,
            IBridgeFactory bridgeFactory,
            string runName,
            IEnumerable<Assembly> explicitAssemblies)
        {
            this.configurationProvider = configurationProvider;
            this.bootstrapperFactory = bootstrapperFactory;
            this.bridgeFactory = bridgeFactory;
            this.runName = runName;
            this.explicitAssemblies = explicitAssemblies;
            this.runData = new Dictionary<string, object>();
            this.contextFactory = null;
        }

        /// <summary>
        /// Performs the run that the current instance is configured for.
        /// </summary>
        public void Run()
        {
            try
            {
                var runConfiguration = this.GetRunConfiguration();
                var segments = runConfiguration.Segments.ToArray();
                for (int i = 0; i < segments.Length; i++)
                {
                    this.RunSegment(i + 1, segments.Length, runConfiguration, segments[i]);
                }
            }
            catch (RunExceptionThresholdReachedException)
            { }
            catch (Exception exception)
            {
                throw new SynnduitException(
                    Resources.RunExceptionMessage, exception);
            }
        }

        private IRunConfiguration GetRunConfiguration() =>
            this.Single(
                this.configurationProvider.ApplicationConfiguration.Runs,
                this.runName,
                run => run.Name,
                Resources.RunNotFound);

        private void RunSegment(
            int segmentIndex,
            int segmentCount,
            IRunConfiguration runConfiguration,
            ISegmentConfiguration segmentConfiguration)
        {
            using IBootstrapper bootstrapper = this.bootstrapperFactory.CreateBootstrapper(
                this.configurationProvider, this.explicitAssemblies);
            using ISafeRepository safeRepository =
                bootstrapper.Get<ISafeRepository>();
            this.RunSegment(
                segmentIndex,
                segmentCount,
                runConfiguration,
                segmentConfiguration,
                bootstrapper,
                safeRepository);
        }

        private void RunSegment(
            int segmentIndex,
            int segmentCount,
            IRunConfiguration runConfiguration,
            ISegmentConfiguration segmentConfiguration,
            IBootstrapper bootstrapper,
            ISafeRepository safeRepository)
        {
            IContext context = this.SetupContext(
                runConfiguration,
                segmentConfiguration,
                segmentIndex,
                segmentCount,
                bootstrapper,
                safeRepository);
            IBridge bridge = this.bridgeFactory.CreateBridge(
                segmentConfiguration.Type,
                context.EntityType.Type,
                bootstrapper);
            IInvocableInitializer invocableInitializer =
                bootstrapper.Get<IInvocableInitializer>();
            bridge.ContextValidator.Validate(context);
            ISegmentRunner segmentRunner = bridge.CreateSegmentRunner();
            bridge.EventDispatcher.SegmentExecuting(new SegmentExecutingArgs());
            invocableInitializer.Initialize(bridge.EventDispatcher);
            try
            {
                segmentRunner.Run();
            }
            catch (SegmentExceptionThresholdReachedException segmentThresholdException)
            {
                bridge.EventDispatcher.SegmentAborted(
                    new SegmentAbortedArgs(segmentThresholdException.Threshold));
            }
            catch (RunExceptionThresholdReachedException runThresholdException)
            {
                bridge.EventDispatcher.RunAborted(
                    new RunAbortedArgs(runThresholdException.Threshold));
                throw;
            }
            bridge.EventDispatcher.SegmentExecuted(new SegmentExecutedArgs());
        }

        private IContext SetupContext(
            IRunConfiguration runConfiguration,
            ISegmentConfiguration segmentConfiguration,
            int segmentIndex,
            int segmentCount,
            IBootstrapper bootstrapper,
            ISafeRepository safeRepository)
        {
            IContext context =
                this
                .GetContextFactory(runConfiguration, safeRepository)
                .CreateContext(
                    bootstrapper,
                    segmentConfiguration,
                    segmentIndex,
                    segmentCount,
                    this.runData);
            IWritableContext writableContext = bootstrapper.Get<IWritableContext>();
            writableContext.SetContext(context);
            return context;
        }

        private ContextFactory GetContextFactory(
            IRunConfiguration runConfiguration, ISafeRepository safeRepository)
        {
            if (this.contextFactory == null)
            {
                this.contextFactory = new ContextFactory(
                    this,
                    runConfiguration,
                    safeRepository.GetExternalSystems(),
                    safeRepository.GetEntityTypes(),
                    safeRepository.GetSharedIdentifierSourceSystems());
            }
            return this.contextFactory;
        }

        private T Single<T>(
            IEnumerable<T> collection,
            string name,
            Func<T, string> getName,
            string exceptionMessageFormat)
        {
            try
            {
                return
                    collection
                    .Single(t => getName(t) == name);
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException(
                    string.Format(exceptionMessageFormat, name),
                    exception);
            }
        }

        private abstract class AbortedArgs : IAbortedArgs
        {
            protected AbortedArgs(int threshold)
            {
                this.Threshold = threshold;
            }

            public int Threshold { get; }
        }

        private class RunAbortedArgs : AbortedArgs, IRunAbortedArgs
        {
            public RunAbortedArgs(int threshold)
                : base(threshold)
            { }
        }

        private class SegmentAbortedArgs : AbortedArgs, ISegmentAbortedArgs
        {
            public SegmentAbortedArgs(int threshold)
                : base(threshold)
            { }
        }

        private class SegmentExecutingArgs : ISegmentExecutingArgs
        {
        }

        private class SegmentExecutedArgs : ISegmentExecutedArgs
        {
        }

        private class ConfigurationWrapper
        {
            public ConfigurationWrapper(
                ApplicationConfiguration applicationConfiguration,
                RunConfiguration runConfiguration)
            {
                this.ApplicationConfiguration = applicationConfiguration;
                this.RunConfiguration = runConfiguration;
            }

            public ApplicationConfiguration ApplicationConfiguration { get; }

            public RunConfiguration RunConfiguration { get; }
        }

        private class ContextFactory
        {
            private readonly Runner parent;

            private readonly IRunConfiguration runConfiguration;

            private readonly IEnumerable<Persistence.IExternalSystem> externalSystems;

            private readonly IEnumerable<Persistence.IEntityType> entityTypes;

            private readonly IEnumerable<
                ISharedIdentifierSourceSystem> sharedIdentifierSourceSystems;

            public ContextFactory(
                Runner parent,
                IRunConfiguration runConfiguration,
                IEnumerable<Persistence.IExternalSystem> externalSystems,
                IEnumerable<Persistence.IEntityType> entityTypes,
                IEnumerable<ISharedIdentifierSourceSystem> sharedIdentifierSourceSystems)
            {
                this.parent = parent;
                this.runConfiguration = runConfiguration;
                this.externalSystems = externalSystems;
                this.entityTypes = entityTypes;
                this.sharedIdentifierSourceSystems = sharedIdentifierSourceSystems;
            }

            public IContext CreateContext(
                IBootstrapper bootstrapper,
                ISegmentConfiguration segmentConfiguration,
                int segmentIndex,
                int segmentCount,
                IDictionary<string, object> runData)
            {
                IContext context;
                IEnumerable<IExternalSystem>
                    externalSystems = this.CreateExternalSystems();
                IExternalSystem sourceSystem =
                    this.GetSourceSystem(segmentConfiguration, externalSystems);
                IEnumerable<IEntityType> entityTypes =
                    this.CreateEntityTypes(sourceSystem, externalSystems);
                IEntityType entityType =
                    this.GetEntityType(segmentConfiguration.EntityType, entityTypes);
                IReadOnlyDictionary<string, string> parameters =
                    this.AssembleParameters(bootstrapper, entityType, sourceSystem);
                context = new Context(
                    segmentConfiguration.Type,
                    segmentIndex,
                    segmentCount,
                    sourceSystem,
                    entityType,
                    externalSystems,
                    entityTypes,
                    parameters,
                    runData,
                    this.runConfiguration,
                    segmentConfiguration);
                return context;
            }

            private IEnumerable<IExternalSystem> CreateExternalSystems() =>
                this
                .externalSystems
                .Select(externalSystem => new ExternalSystem(externalSystem))
                .ToArray();

            private IExternalSystem GetSourceSystem(
                ISegmentConfiguration segmentConfiguration,
                IEnumerable<IExternalSystem> externalSystems)
            {
                IExternalSystem sourceSystem = null;
                string sourceSystemName = this.GetSourceSystemName(segmentConfiguration);
                if(sourceSystemName != null)
                {
                    sourceSystem = this.parent.Single(
                        externalSystems,
                        this.GetSourceSystemName(segmentConfiguration),
                        externalSystem => externalSystem.Name,
                        Resources.SourceSystemNotFound);
                }
                return sourceSystem;
            }

            private string GetSourceSystemName(ISegmentConfiguration segmentConfiguration) =>
                this.GetInheritedOverridenValue(
                    segmentConfiguration.SourceSystem,
                    this.runConfiguration.SourceSystem,
                    this.parent.configurationProvider.ApplicationConfiguration.SourceSystem);

            private IEnumerable<IEntityType> CreateEntityTypes(
                IExternalSystem sourceSystem,
                IEnumerable<IExternalSystem> externalSystems)
            {
                IEnumerable<IEntityType> entityTypes;
                IDictionary<Guid, IExternalSystem> externalSystemsById =
                    externalSystems
                    .ToDictionary(externalSystem => externalSystem.Id);
                if(sourceSystem != null)
                {
                    IDictionary<Guid, IEnumerable<Guid>> sharedIdentifierSourceSystemIds =
                        this
                        .sharedIdentifierSourceSystems
                        .Where(siss => siss.SourceSystemId == sourceSystem.Id)
                        .GroupBy(siss => siss.EntityTypeId)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(
                                siss => siss.SharedIdentifierSourceSystemId));
                    entityTypes =
                        this
                        .entityTypes
                        .Select(entityType => new EntityType(
                            entityType,
                            this.GetExternalSystem(
                                entityType.DestinationSystemId, externalSystemsById),
                            this.GetSharedIdentifierSourceSystems(
                                entityType.Id,
                                sharedIdentifierSourceSystemIds,
                                externalSystemsById)))
                        .ToArray();
                }
                else
                {
                    entityTypes =
                        this
                        .entityTypes
                        .Select(entityType => new EntityType(
                            entityType,
                            this.GetExternalSystem(
                                entityType.DestinationSystemId, externalSystemsById),
                            null))
                        .ToArray();
                }
                return entityTypes;
            }

            private IEnumerable<IExternalSystem> GetSharedIdentifierSourceSystems(
                Guid entityTypeId,
                IDictionary<Guid, IEnumerable<Guid>> sharedIdentifierSourceSystemIds,
                IDictionary<Guid, IExternalSystem> externalSystemsById)
            {
                var sharedIdentifierSourceSystems = Array.Empty<IExternalSystem>();
                if(sharedIdentifierSourceSystemIds
                    .TryGetValue(entityTypeId, out IEnumerable<Guid> sourceSystemIds))
                {
                    sharedIdentifierSourceSystems =
                        sourceSystemIds
                        .Select(id => this.GetExternalSystem(id, externalSystemsById))
                        .ToArray();
                }
                return sharedIdentifierSourceSystems;
            }

            private IExternalSystem GetExternalSystem(
                Guid id, IDictionary<Guid, IExternalSystem> externalSystemsById)
            {
                if(externalSystemsById.TryGetValue(
                    id, out IExternalSystem externalSystem) == false)
                {
                    throw new InvalidOperationException(
                        string.Format(Resources.ExternalSystemIdNotFound, id));
                }
                return externalSystem;
            }

            private IEntityType GetEntityType(
                string entityTypeName, IEnumerable<IEntityType> entityTypes) =>
                this.parent.Single(
                    entityTypes,
                    entityTypeName,
                    entityType => entityType.Name,
                    Resources.EntityTypeNotFound);

            private IReadOnlyDictionary<string, string> AssembleParameters(
                IBootstrapper bootstrapper,
                IEntityType entityType,
                IExternalSystem sourceSystem)
            {
                IParametersAssembler parametersAssembler
                    = bootstrapper.Get<IParametersAssembler>();
                return parametersAssembler.AssembleParameters(
                    entityType.DestinationSystem.Id,
                    entityType.Id,
                    sourceSystem != null ? (Guid?) sourceSystem.Id : null);
            }

            private string GetInheritedOverridenValue(params string[] values) =>
                values.FirstOrDefault(value => value != null);
        }

        private class Context : IContext
        {
            public Context(
                SegmentType segmentType,
                int segmentIndex,
                int segmentCount,
                IExternalSystem sourceSystem,
                IEntityType entityType,
                IEnumerable<IExternalSystem> externalSystems,
                IEnumerable<IEntityType> entityTypes,
                IReadOnlyDictionary<string, string> parameters,
                IDictionary<string, object> runData,
                IRunConfiguration runConfiguration,
                ISegmentConfiguration segmentConfiguration)
            {
                this.SegmentType = segmentType;
                this.SegmentIndex = segmentIndex;
                this.SegmentCount = segmentCount;
                this.SourceSystem = sourceSystem;
                this.EntityType = entityType;
                this.ExternalSystems = externalSystems;
                this.EntityTypes = entityTypes;
                this.Parameters = parameters;
                this.RunData = runData;
                this.RunConfiguration = runConfiguration;
                this.SegmentConfiguration = segmentConfiguration;
            }

            public SegmentType SegmentType { get; }

            public int SegmentIndex { get; }

            public int SegmentCount { get; }

            public IExternalSystem SourceSystem { get; }

            public IExternalSystem DestinationSystem => this.EntityType.DestinationSystem;

            public IEntityType EntityType { get; }

            public IEnumerable<IExternalSystem> ExternalSystems { get; }

            public IEnumerable<IEntityType> EntityTypes { get; }

            public IReadOnlyDictionary<string, string> Parameters { get; }

            public IDictionary<string, object> RunData { get; }

            public IRunConfiguration RunConfiguration { get; }

            public ISegmentConfiguration SegmentConfiguration { get; }
        }

        private class ExternalSystem : IExternalSystem
        {
            private readonly Persistence.IExternalSystem externalSystem;

            public ExternalSystem(Persistence.IExternalSystem externalSystem)
            {
                this.externalSystem = externalSystem;
            }

            public Guid Id => this.externalSystem.Id;

            public string Name => this.externalSystem.Name;
        }

        private class EntityType : IEntityType
        {
            private static readonly IDictionary<Guid, Type> cachedEntityTypes;

            static EntityType()
            {
                cachedEntityTypes = new Dictionary<Guid, Type>();
            }

            private readonly Persistence.IEntityType entityType;

            private readonly IExternalSystem destinationSystem;

            public EntityType(
                Persistence.IEntityType entityType,
                IExternalSystem destinationSystem,
                IEnumerable<IExternalSystem> sharedIdentifierSourceSystems)
            {
                this.entityType = entityType;
                this.destinationSystem = destinationSystem;
                this.Type = this.GetEntityType(entityType);
                this.SharedIdentifierSourceSystems = sharedIdentifierSourceSystems;
            }

            public Guid Id => this.entityType.Id;

            public string Name => this.entityType.Name;

            public IExternalSystem DestinationSystem => this.destinationSystem;

            public Type Type { get; }

            public bool IsMutable => this.entityType.IsMutable;

            public bool IsDuplicable => this.entityType.IsDuplicable;

            public IEnumerable<IExternalSystem> SharedIdentifierSourceSystems { get; }

            private Type GetEntityType(Persistence.IEntityType entityType)
            {
                if(cachedEntityTypes.TryGetValue(entityType.Id, out Type type) == false)
                {
                    type = this.ExtractEntityType(entityType);
                    cachedEntityTypes.Add(entityType.Id, type);
                }
                return type;
            }

            private Type ExtractEntityType(Persistence.IEntityType entityType)
            {
                try
                {
                    return
                        AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .Where(assembly => assembly.IsDynamic == false)
                        .SelectMany(assembly => assembly.ExportedTypes)
                        .Single(type => type.AssemblyQualifiedName == entityType.TypeName);
                }
                catch
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.InvalidEntityType,
                        entityType.Name,
                        entityType.TypeName));
                }
            }
        }
    }
}
