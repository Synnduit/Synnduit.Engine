using Synnduit.Configuration;
using Synnduit.Deduplication;
using Synnduit.Events;
using Synnduit.Preprocessing;
using Synnduit.Properties;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Reflection;

namespace Synnduit
{
    /// <summary>
    /// Manages the dependency injection (MEF) container and provides access to interface
    /// implementations.
    /// </summary>
    internal class Bootstrapper : IBootstrapper
    {
        private static readonly Lazy<BootstrapperDependencyContainer> container =
            new(CreateContainer);

        private static BootstrapperDependencyContainer CreateContainer()
        {
            var container = new BootstrapperDependencyContainer();
            DependencyContainer.SetContainer(container);
            return container;
        }

        private readonly CompositionContainer compositionContainer;

        public Bootstrapper(
            IConfigurationProvider configurationProvider,
            IEnumerable<Assembly> explicitAssemblies)
        {
            this.compositionContainer = this.CreateCompositionContainer(
                configurationProvider, explicitAssemblies);
            container.Value.Bootstrapper = this;
        }

        /// <summary>
        /// Gets the implementation of the specified (interface) type.
        /// </summary>
        /// <typeparam name="T">
        /// The (interface) type whose implementation is requested.
        /// </typeparam>
        /// <returns>The implementation of the specified (interface) type.</returns>
        public T Get<T>()
        {
            return this.compositionContainer.GetExportedValue<T>();
        }

        /// <summary>
        /// Gets all implementations of the specified (interface) type.
        /// </summary>
        /// <typeparam name="T">
        /// The (interface) type whose implementations are requested.
        /// </typeparam>
        /// <returns>The implementations of the specified (interface) type.</returns>
        public IEnumerable<T> GetMany<T>()
        {
            return this.compositionContainer.GetExportedValues<T>();
        }

        /// <summary>
        /// Disposes the underlying composition container.
        /// </summary>
        public void Dispose()
        {
            this.compositionContainer.Dispose();
        }

        private CompositionContainer CreateCompositionContainer(
            IConfigurationProvider configurationProvider,
            IEnumerable<Assembly> explicitAssemblies)
        {
            try
            {
                CompositionContainer compositionContainer;
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(this.GetType().Assembly));
                foreach (var assembly in explicitAssemblies)
                {
                    catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                }
                catalog.Catalogs.Add(new DirectoryCatalog(
                    configurationProvider.ApplicationConfiguration.BinaryFilesDirectoryPath ??
                    throw new InvalidOperationException(
                        Resources.BinaryFilesDirectoryPathMustBeSpecified),
                    this.CreateRegistrationBuilder()));
                compositionContainer = new CompositionContainer(catalog);
                compositionContainer.ComposeParts();
                compositionContainer
                    .GetExportedValue<IWritableConfigurationProvider>()
                    .SetConfigurationProvider(configurationProvider);
                return compositionContainer;
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException(
                    Resources.DependencyInjectionContainerCompositionFailed,
                    exception);
            }
        }

        private RegistrationBuilder CreateRegistrationBuilder()
        {
            var builder = new RegistrationBuilder();
            this.ExportEntityTypeDefinitions(builder);
            this.ExportFeeds(builder);
            this.ExportSinks(builder);
            this.ExportCacheFeeds(builder);
            this.ExportPreprocessorOperations(builder);
            this.ExportDuplicationRules(builder);
            this.ExportEventReceivers(builder);
            this.ExportHomogenizers(builder);
            return builder;
        }

        private void ExportEntityTypeDefinitions(RegistrationBuilder builder)
        {
            var entityTypeRegistrationWrapper = new GenericExportRegistrationWrapper(
                typeof(EntityTypeDefinitionAttribute),
                typeof(IEntityTypeDefinition<>),
                false);
            entityTypeRegistrationWrapper.Export(builder);
        }

        private void ExportFeeds(RegistrationBuilder builder)
        {
            var feedRegistrationWrapper = new GenericExportRegistrationWrapper(
                typeof(FeedAttribute),
                typeof(IFeed<>),
                true);
            feedRegistrationWrapper.Export(builder);
        }

        private void ExportSinks(RegistrationBuilder builder)
        {
            var sinkRegistrationWrapper = new GenericExportRegistrationWrapper(
                typeof(SinkAttribute),
                typeof(ISink<>),
                true);
            sinkRegistrationWrapper.Export(builder);
        }

        private void ExportCacheFeeds(RegistrationBuilder builder)
        {
            var cacheFeedRegistrationWrapper = new GenericExportRegistrationWrapper(
                typeof(CacheFeedAttribute),
                typeof(ICacheFeed<>),
                true);
            cacheFeedRegistrationWrapper.Export(builder);
        }

        private void ExportPreprocessorOperations(RegistrationBuilder builder)
        {
            var preprocessorOperationRegistrationWrapper =
                new GenericExportRegistrationWrapper(
                    typeof(PreprocessorOperationAttribute),
                    typeof(IPreprocessorOperation<>),
                    true);
            preprocessorOperationRegistrationWrapper.Export(builder);
        }

        private void ExportDuplicationRules(RegistrationBuilder builder)
        {
            var duplicationRuleRegistrationWrapper = new GenericExportRegistrationWrapper(
                typeof(DuplicationRuleAttribute),
                typeof(IDuplicationRule<>),
                true);
            duplicationRuleRegistrationWrapper.Export(builder);
        }

        private void ExportEventReceivers(RegistrationBuilder builder)
        {
            var eventReceiverRegistrationWrapper = new GenericExportRegistrationWrapper(
                typeof(EventReceiverAttribute),
                typeof(IEntityTypeEventReceiver<>),
                true);
            eventReceiverRegistrationWrapper.Export(builder);
        }

        private void ExportHomogenizers(RegistrationBuilder builder)
        {
            var homogenizerRegistrationWrapper = new NonGenericExportRegistrationWrapper(
                typeof(HomogenizerAttribute), typeof(IHomogenizer));
            homogenizerRegistrationWrapper.Export(builder);
        }

        private class GenericExportRegistrationWrapper : ExportRegistrationWrapper
        {
            private readonly bool supportsMultiple;

            public GenericExportRegistrationWrapper(
                Type attributeType, Type interfaceType, bool supportsMultiple)
                : base(attributeType, interfaceType)
            {
                this.supportsMultiple = supportsMultiple;
            }

            public void Export(RegistrationBuilder builder)
            {
                builder
                    .ForTypesMatching(this.IsTypeMatch)
                    .ExportInterfaces(this.IsInterfaceTypeMatch);
            }

            private bool IsTypeMatch(Type type)
            {
                bool isMatch = false;
                if(this.IsAttributeApplied(type))
                {
                    if(type.ContainsGenericParameters)
                    {
                        this.ValidateGenericType(type);
                    }
                    else
                    {
                        this.ValidateNonGenericType(type);
                    }
                    isMatch = true;
                }
                return isMatch;
            }

            private bool IsInterfaceTypeMatch(Type interfaceType)
            {
                return
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == this.InterfaceType;
            }

            private void ValidateGenericType(Type type)
            {
                bool isValid = false;
                if(this.supportsMultiple)
                {
                    Type entityType = this.ExtractEntityType(type);
                    Type interfaceType = this.ExtractInterfaceType(type);
                    isValid = interfaceType ==
                        this.InterfaceType.MakeGenericType(entityType);
                }
                if(!isValid)
                {
                    throw this.CreateException(type);
                }
            }

            private void ValidateNonGenericType(Type type)
            {
                int implementedInterfaceCount =
                    this.GetImplementedInterfaces(type).Count();
                if((!this.supportsMultiple && implementedInterfaceCount != 1) ||
                    implementedInterfaceCount < 1)
                {
                    throw this.CreateException(type);
                }
            }

            private Type ExtractEntityType(Type type)
            {
                return this.ExtractType(type, type.GetGenericArguments());
            }

            private Type ExtractInterfaceType(Type type)
            {
                return this.ExtractType(
                    type, this.GetImplementedInterfaces(type));
            }

            private Type ExtractType(Type type, IEnumerable<Type> types)
            {
                Type extractedType;
                if(types.Count() == 1)
                {
                    extractedType = types.Single();
                }
                else
                {
                    throw this.CreateException(type);
                }
                return extractedType;
            }

            private IEnumerable<Type> GetImplementedInterfaces(Type type)
            {
                return type.GetInterfaces().Where(this.IsInterfaceTypeMatch);
            }

            private InvalidOperationException CreateException(Type type)
            {
                return new InvalidOperationException(string.Format(
                    Resources.TypeNotValidForInterfaceExport,
                    type.FullName,
                    this.InterfaceType.FullName));
            }
        }

        private class NonGenericExportRegistrationWrapper : ExportRegistrationWrapper
        {
            public NonGenericExportRegistrationWrapper(
                Type attributeType, Type interfaceType)
                : base(attributeType, interfaceType)
            { }

            public void Export(RegistrationBuilder builder)
            {
                builder
                    .ForTypesMatching(this.ImplementsInterface)
                    .ExportInterfaces(this.IsInterfaceTypeMatch);
            }

            private bool ImplementsInterface(Type type)
            {
                return
                    type
                    .GetInterfaces()
                    .Where(this.IsInterfaceTypeMatch)
                    .Count() == 1;
            }

            private bool IsInterfaceTypeMatch(Type interfaceType)
            {
                return interfaceType == this.InterfaceType;
            }
        }

        private abstract class ExportRegistrationWrapper
        {
            protected ExportRegistrationWrapper(
                Type attributeType, Type interfaceType)
            {
                this.AttributeType = attributeType;
                this.InterfaceType = interfaceType;
            }

            protected Type AttributeType { get; }

            protected Type InterfaceType { get; }

            protected bool IsAttributeApplied(Type type) =>
                type.GetCustomAttributes(this.AttributeType, false).Length > 0;
        }

        private class BootstrapperDependencyContainer : IDependencyContainer
        {
            public IBootstrapper Bootstrapper { get; set; }

            public bool IsPermanent => true;

            public T Get<T>()
            {
                return this.Bootstrapper.Get<T>();
            }
        }
    }
}
