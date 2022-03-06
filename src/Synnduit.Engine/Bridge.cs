using System;
using System.Reflection;
using Synnduit.Events;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Represents a "bridge" between those parts of the system that require a generic
    /// entity type parameter (TEntity) and those that don't have it.
    /// </summary>
    internal class Bridge : IBridge
    {
        private readonly SegmentTypes segmentTypes;

        private readonly Type entityType;

        private readonly IBootstrapper bootstrapper;

        private readonly Lazy<IEventDispatcher> eventDispatcher;

        private readonly Lazy<IContextValidator> contextValidator;

        public Bridge(
            SegmentType segmentType,
            Type entityType,
            IBootstrapper bootstrapper)
        {
            this.segmentTypes = this.GetSegmentTypes(segmentType);
            this.entityType = entityType;
            this.bootstrapper = bootstrapper;
            this.eventDispatcher =
                new Lazy<IEventDispatcher>(this.CreateEventDispatcher);
            this.contextValidator =
                new Lazy<IContextValidator>(this.CreateContextValidator);
        }

        /// <summary>
        /// Gets the <see cref="IEventDispatcher" /> implementation to be used for the
        /// current run segment.
        /// </summary>
        public IEventDispatcher EventDispatcher
        {
            get { return this.eventDispatcher.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IContextValidator" /> implementation to be used for the
        /// current run segment.
        /// </summary>
        public IContextValidator ContextValidator
        {
            get { return this.contextValidator.Value; }
        }

        /// <summary>
        /// Creates a new instance that implements the <see cref="ISegmentRunner" />
        /// interface.
        /// </summary>
        public ISegmentRunner CreateSegmentRunner()
        {
            return this.Get<ISegmentRunner>(
                this.segmentTypes.SegmentRunnerType);
        }

        private SegmentTypes GetSegmentTypes(SegmentType segmentType) =>
            segmentType switch
            {
                SegmentType.Migration =>
                    new SegmentTypes(
                        typeof(IMigrationContextValidator),
                        typeof(IMigrationSegmentRunner<>)),
                SegmentType.GarbageCollection =>
                    new SegmentTypes(
                        typeof(IGarbageCollectionContextValidator),
                        typeof(IGarbageCollectionSegmentRunner<>)),
                _ => throw new InvalidOperationException(
                    string.Format(
                        Resources.InvalidSegmentType, segmentType))
            };

        private IEventDispatcher CreateEventDispatcher()
        {
            return this.Get<IEventDispatcher>(typeof(IEventDispatcher<>));
        }

        private IContextValidator CreateContextValidator()
        {
            return this.Get<IContextValidator>(
                this.segmentTypes.ContextValidatorType);
        }

        private TInterface Get<TInterface>(Type type)
        {
            Type fullType =
                type.ContainsGenericParameters
                ? type.MakeGenericType(this.entityType)
                : type;
            MethodInfo getMethod =
                typeof(IBootstrapper)
                .GetMethod("Get")
                .MakeGenericMethod(fullType);
            return (TInterface) getMethod.Invoke(this.bootstrapper, null);
        }

        private class SegmentTypes
        {
            public SegmentTypes(Type contextValidatorType, Type segmentRunnerType)
            {
                this.ContextValidatorType = contextValidatorType;
                this.SegmentRunnerType = segmentRunnerType;
            }

            public Type ContextValidatorType { get; }

            public Type SegmentRunnerType { get; }
        }
    }
}
