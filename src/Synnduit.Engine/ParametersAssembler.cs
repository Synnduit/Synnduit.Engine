using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Synnduit.Persistence;

namespace Synnduit
{
    /// <summary>
    /// Assembles the dictionary of parameters applicable to a run segment.
    /// </summary>
    [Export(typeof(IParametersAssembler))]
    internal class ParametersAssembler : IParametersAssembler
    {
        private readonly ISafeRepository safeRepository;

        [ImportingConstructor]
        public ParametersAssembler(ISafeRepository safeRepository)
        {
            this.safeRepository = safeRepository;
        }

        /// <summary>
        /// Assembles the dictionary of parameters for a run segment involving the
        /// specified destination system, entity type, and source system.
        /// </summary>
        /// <param name="destinationSystemId">
        /// The ID of the destination (external) system.
        /// </param>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the source (external) systems.</param>
        /// <returns>
        /// The dictionary of parameters for a run segment involving the specified
        /// destination system, entity type, and source system.
        /// </returns>
        public IReadOnlyDictionary<string, string> AssembleParameters(
            Guid destinationSystemId, Guid entityTypeId, Guid? sourceSystemId)
        {
            var parameters = new Dictionary<string, string>();
            this.ApplyDestinationSystemParameters(parameters, destinationSystemId);
            this.ApplyEntityTypeParameters(parameters, entityTypeId);
            if(sourceSystemId.HasValue)
            {
                this.ApplySourceSystemParameters(parameters, sourceSystemId.Value);
                this.ApplyEntityTypeSourceSystemParameters(
                    parameters, entityTypeId, sourceSystemId.Value);
            }
            return new ReadOnlyDictionary<string, string>(parameters);
        }

        private void ApplyDestinationSystemParameters(
            Dictionary<string, string> parameters, Guid destinationSystemId)
        {
            this.ApplyParameters(
                parameters,
                this.safeRepository.GetDestinationSystemParameters(destinationSystemId));
        }

        private void ApplyEntityTypeParameters(
            Dictionary<string, string> parameters, Guid entityTypeId)
        {
            this.ApplyParameters(
                parameters,
                this.safeRepository.GetEntityTypeParameters(entityTypeId));
        }

        private void ApplySourceSystemParameters(
            Dictionary<string, string> parameters, Guid sourceSystemId)
        {
            this.ApplyParameters(
                parameters,
                this.safeRepository.GetSourceSystemParameters(sourceSystemId));
        }

        private void ApplyEntityTypeSourceSystemParameters(
            Dictionary<string, string> parameters,
            Guid entityTypeId,
            Guid sourceSystemId)
        {
            this.ApplyParameters(
                parameters,
                this.safeRepository.GetEntityTypeSourceSystemParameters(
                    entityTypeId, sourceSystemId));
        }

        private void ApplyParameters(
            Dictionary<string, string> parameters,
            IDictionary<string, string> applicableParameters)
        {
            foreach(KeyValuePair<string, string>
                applicableParameter in applicableParameters)
            {
                parameters[applicableParameter.Key] = applicableParameter.Value;
            }
        }
    }
}
