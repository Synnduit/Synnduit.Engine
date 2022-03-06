using System;
using System.ComponentModel.Composition;

namespace Synnduit.Mappings
{
    /// <summary>
    /// Maps source system entity IDs to their destination system counterparts. 
    /// </summary>
    [Export(typeof(IMapper))]
    internal class Mapper : IMapper
    {
        private readonly IContext context;

        private readonly IMappingDataRepository mappingDataRepository;

        [ImportingConstructor]
        public Mapper(
            IContext context,
            IMappingDataRepository mappingDataRepository)
        {
            this.context = context;
            this.mappingDataRepository = mappingDataRepository;
        }

        /// <summary>
        /// Maps the specified source system entity ID (of the specified type) to its
        /// destination system counterpart.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in the current source system.
        /// </param>
        /// <returns>
        /// The ID that uniquely identifies the entity in the destination system; if the
        /// specified source system entity ID isn't mapped, null will be returned.
        /// </returns>
        public EntityIdentifier GetDestinationSystemEntityId(
            Guid entityTypeId,
            EntityIdentifier sourceSystemEntityId)
        {
            return this.GetDestinationSystemEntityId(
                entityTypeId,
                this.context.SourceSystem.Id,
                sourceSystemEntityId);
        }

        /// <summary>
        /// Maps the specified source system entity ID (of the specified type) to its
        /// destination system counterpart.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the entity's source system.</param>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in the specified source system.
        /// </param>
        /// <returns>
        /// The ID that uniquely identifies the entity in the destination system; if the
        /// specified source system entity ID isn't mapped, null will be returned.
        /// </returns>
        public EntityIdentifier GetDestinationSystemEntityId(
            Guid entityTypeId,
            Guid sourceSystemId,
            EntityIdentifier sourceSystemEntityId)
        {
            // validate the sourceSystemEntityId parameter
            ArgumentValidator.EnsureArgumentNotNull(
                sourceSystemEntityId, nameof(sourceSystemEntityId));

            // get and return the destination system entity ID
            EntityIdentifier destinationSystemEntityId = null;
            MappingDataObject mappingData =
                this.mappingDataRepository.GetMappingData(
                    entityTypeId, sourceSystemId, sourceSystemEntityId);
            if(mappingData != null)
            {
                destinationSystemEntityId = mappingData.DestinationSystemEntityId;
            }
            return destinationSystemEntityId;
        }
    }
}
