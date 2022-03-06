using System;
using System.Collections.Generic;

namespace Synnduit
{
    /// <summary>
    /// Assembles the dictionary of parameters applicable to a run segment.
    /// </summary>
    internal interface IParametersAssembler
    {
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
        IReadOnlyDictionary<string, string> AssembleParameters(
            Guid destinationSystemId, Guid entityTypeId, Guid? sourceSystemId);
    }
}
