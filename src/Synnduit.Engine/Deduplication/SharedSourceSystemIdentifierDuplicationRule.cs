using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Mappings;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// The duplication rule that deduplicates entities based on their identifiers in
    /// source systems that share identifiers with the current source system.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IDuplicationRule<>))]
    internal class SharedSourceSystemIdentifierDuplicationRule<TEntity> :
        IDuplicationRule<TEntity>
        where TEntity : class
    {
        private readonly IContext context;

        private readonly ISafeMetadataProvider<TEntity> safeMetadataProvider;

        private readonly IMapper mapper;

        [ImportingConstructor]
        public SharedSourceSystemIdentifierDuplicationRule(
            IContext context,
            ISafeMetadataProvider<TEntity> safeMetadataProvider,
            IMapper mapper)
        {
            this.context = context;
            this.safeMetadataProvider = safeMetadataProvider;
            this.mapper = mapper;
        }

        /// <summary>
        /// Gets the collection of duplicates for the specified entity.
        /// </summary>
        /// <param name="entity">The entity that is being deduplicated.</param>
        /// <returns>The collection of duplicates.</returns>
        public IEnumerable<Duplicate> GetDuplicates(TEntity entity)
        {
            return
                this
                .context
                .EntityType
                .SharedIdentifierSourceSystems
                .Select(system => this.GetDestinationSystemEntityId(entity, system.Id))
                .Where(id => id != null)
                .Distinct()
                .Select(id => new Duplicate(id, MatchWeight.Positive))
                .ToArray();
        }

        private EntityIdentifier GetDestinationSystemEntityId(
            TEntity entity, Guid sourceSystemId)
        {
            EntityIdentifier sourceSystemEntityId =
                this
                .safeMetadataProvider
                .GetSourceSystemEntityId(entity);
            return
                this
                .mapper
                .GetDestinationSystemEntityId(
                    this.context.EntityType.Id,
                    sourceSystemId,
                    sourceSystemEntityId);
        }
    }
}
