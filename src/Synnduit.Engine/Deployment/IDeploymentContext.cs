using System;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Exposes deployment-related functionality.
    /// </summary>
    internal interface IDeploymentContext
    {
        /// <summary>
        /// Creates or updates the specified external system.
        /// </summary>
        /// <param name="id">The ID of the external system.</param>
        /// <param name="name">The name of the external system.</param>
        void ExternalSystem(Guid id, string name);

        /// <summary>
        /// Creates or updates the specified entity type.
        /// </summary>
        /// <param name="id">The ID of the entity type.</param>
        /// <param name="destinationSystemId">
        /// The ID of the entity type's parent destination (external) system.
        /// </param>
        /// <param name="name">The name of the entity type.</param>
        /// <param name="type">The type that represents the entity.</param>
        /// <param name="sinkType">
        /// The type that represents the entity type's sink.
        /// </param>
        /// <param name="cacheFeedType">
        /// The type that represents the entity type's cache feed; optional, may be null.
        /// </param>
        /// <param name="isMutable">
        /// A value indicating whether instances of the entity type are mutable; i.e.,
        /// whether or not they may change between runs.
        /// </param>
        /// <param name="isDuplicable">
        /// A value indicating whether source system instances of the entity type may be
        /// duplicates (i.e., represent the same destination system entity); in other
        /// words, this value indicates whether or not source system entity instances
        /// should be deduplicated.
        /// </param>
        void EntityType(
            Guid id,
            Guid destinationSystemId,
            string name,
            Type type,
            Type sinkType,
            Type cacheFeedType = null,
            bool isMutable = true,
            bool isDuplicable = true);

        /// <summary>
        /// Creates a group of source (external) systems that share identifiers for the
        /// specified entity type.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemIds">
        /// The array of IDs identifying the source (external) systems that share
        /// identifiers for the specified entity type.
        /// </param>
        void SharedSourceSystemIdentifiers(
            Guid entityTypeId, params Guid[] sourceSystemIds);

        /// <summary>
        /// Creates or updates the specified feed.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <param name="feedType">The type that represents the feed.</param>
        void Feed(
            Guid entityTypeId,
            Guid sourceSystemId,
            Type feedType);

        /// <summary>
        /// Creates or updates the specified destination system parameter.
        /// </summary>
        /// <param name="destinationSystemId">
        /// The ID of the destination (external) system.
        /// </param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        void DestinationSystemParameter(
            Guid destinationSystemId,
            string name,
            string value);

        /// <summary>
        /// Creates or updates the specified entity type parameter.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        void EntityTypeParameter(
            Guid entityTypeId,
            string name,
            string value);

        /// <summary>
        /// Creates or updates the specified source system parameter.
        /// </summary>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        void SourceSystemParameter(
            Guid sourceSystemId,
            string name,
            string value);

        /// <summary>
        /// Creates or updates the specified entity type/source system parameter.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        void EntityTypeSourceSystemParameter(
            Guid entityTypeId,
            Guid sourceSystemId,
            string name,
            string value);
    }
}
