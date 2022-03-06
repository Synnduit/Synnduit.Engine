using System;
using Synnduit.Properties;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Extension methods for the <see cref="IDeploymentContext" /> interface.
    /// </summary>
    internal static class DeploymentContextExtensions
    {
        /// <summary>
        /// The name of the parameter that specifies orphan mapping behavior.
        /// </summary>
        public const string OrphanMappingBehaviorParameterName = "OrphanMappingBehavior";

        /// <summary>
        /// The name of the parameter that specifies garbage collection behavior.
        /// </summary>
        public const string
            GarbageCollectionBehaviorParameterName = "GarbageCollectionBehavior";

        /// <summary>
        /// Sets the orphan mapping behavior for the specified destination system.
        /// </summary>
        /// <param name="context">The deployment context.</param>
        /// <param name="destinationSystemId">
        /// The ID of the destination (external) system.
        /// </param>
        /// <param name="behavior">The orphan mapping behavior.</param>
        public static void DestinationSystemOrphanMappingBehavior(
            this IDeploymentContext context,
            Guid destinationSystemId,
            OrphanMappingBehavior behavior)
        {
            ArgumentValidator.EnsureArgumentNotNull(context, nameof(context));
            ArgumentValidator.EnsureArgumentNotEmpty(
                destinationSystemId, nameof(destinationSystemId));
            ValidateOrphanMappingBehavior(behavior);
            context.DestinationSystemParameter(
                destinationSystemId,
                OrphanMappingBehaviorParameterName,
                behavior.ToString());
        }

        /// <summary>
        /// Sets the orphan mapping behavior for the specified entity type.
        /// </summary>
        /// <param name="context">The deployment context.</param>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="behavior">The orphan mapping behavior.</param>
        public static void EntityTypeOrphanMappingBehavior(
            this IDeploymentContext context,
            Guid entityTypeId,
            OrphanMappingBehavior behavior)
        {
            ArgumentValidator.EnsureArgumentNotNull(context, nameof(context));
            ArgumentValidator.EnsureArgumentNotEmpty(entityTypeId, nameof(entityTypeId));
            ValidateOrphanMappingBehavior(behavior);
            context.EntityTypeParameter(
                entityTypeId,
                OrphanMappingBehaviorParameterName,
                behavior.ToString());
        }

        /// <summary>
        /// Sets the orphan mapping behavior for the specified source system.
        /// </summary>
        /// <param name="context">The deployment context.</param>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <param name="behavior">The orphan mapping behavior.</param>
        public static void SourceSystemOrphanMappingBehavior(
            this IDeploymentContext context,
            Guid sourceSystemId,
            OrphanMappingBehavior behavior)
        {
            ArgumentValidator.EnsureArgumentNotNull(context, nameof(context));
            ArgumentValidator.EnsureArgumentNotEmpty(
                sourceSystemId, nameof(sourceSystemId));
            ValidateOrphanMappingBehavior(behavior);
            context.SourceSystemParameter(
                sourceSystemId,
                OrphanMappingBehaviorParameterName,
                behavior.ToString());
        }

        /// <summary>
        /// Sets the orphan mapping behavior for the specified entity type/source system.
        /// </summary>
        /// <param name="context">The deployment context.</param>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <param name="behavior">The orphan mapping behavior.</param>
        public static void OrphanMappingBehavior(
            this IDeploymentContext context,
            Guid entityTypeId,
            Guid sourceSystemId,
            OrphanMappingBehavior behavior)
        {
            ArgumentValidator.EnsureArgumentNotNull(context, nameof(context));
            ArgumentValidator.EnsureArgumentNotEmpty(entityTypeId, nameof(entityTypeId));
            ArgumentValidator.EnsureArgumentNotEmpty(
                sourceSystemId, nameof(sourceSystemId));
            ValidateOrphanMappingBehavior(behavior);
            context.EntityTypeSourceSystemParameter(
                entityTypeId,
                sourceSystemId,
                OrphanMappingBehaviorParameterName,
                behavior.ToString());
        }

        /// <summary>
        /// Sets the garbage collection behavior for the specified destination system.
        /// </summary>
        /// <param name="context">The deployment context.</param>
        /// <param name="destinationSystemId">
        /// The ID of the destination (external) system.
        /// </param>
        /// <param name="behavior">The garbage collection behavior.</param>
        public static void DestinationSystemGarbageCollectionBehavior(
            this IDeploymentContext context,
            Guid destinationSystemId,
            GarbageCollectionBehavior behavior)
        {
            ArgumentValidator.EnsureArgumentNotNull(context, nameof(context));
            ArgumentValidator.EnsureArgumentNotEmpty(
                destinationSystemId, nameof(destinationSystemId));
            ValidateGarbageCollectionBehavior(behavior);
            context.DestinationSystemParameter(
                destinationSystemId,
                GarbageCollectionBehaviorParameterName,
                behavior.ToString());
        }

        /// <summary>
        /// Sets the garbage collection behavior for the specified entity type.
        /// </summary>
        /// <param name="context">The deployment context.</param>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="behavior">The garbage collection behavior.</param>
        public static void EntityTypeGarbageCollectionBehavior(
            this IDeploymentContext context,
            Guid entityTypeId,
            GarbageCollectionBehavior behavior)
        {
            ArgumentValidator.EnsureArgumentNotNull(context, nameof(context));
            ArgumentValidator.EnsureArgumentNotEmpty(entityTypeId, nameof(entityTypeId));
            ValidateGarbageCollectionBehavior(behavior);
            context.EntityTypeParameter(
                entityTypeId,
                GarbageCollectionBehaviorParameterName,
                behavior.ToString());
        }

        private static void ValidateOrphanMappingBehavior(OrphanMappingBehavior behavior)
        {
            ValidateBehavior(behavior, Resources.InvalidOrphanMappingBehavior);
        }

        private static void
            ValidateGarbageCollectionBehavior(GarbageCollectionBehavior behavior)
        {
            ValidateBehavior(behavior, Resources.InvalidGarbageCollectionBehavior);
        }

        private static void ValidateBehavior<TBehavior>(
            TBehavior behavior, string exceptionMessageFormat)
        {
            if(Enum.IsDefined(typeof(TBehavior), behavior) == false)
            {
                throw new ArgumentException(
                    string.Format(exceptionMessageFormat, behavior), nameof(behavior));
            }
        }
    }
}
