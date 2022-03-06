using System;
using System.Collections.Generic;
using Synnduit.Events;
using Synnduit.Persistence;

namespace Synnduit
{
    /// <summary>
    /// An operation scope that makes a code block part of the processing of a single
    /// operation.
    /// </summary>
    internal interface IOperationScope : IOperation, IMessageLogger, IDisposable
    {
        /// <summary>
        /// Gets the collection of log messages associated with the current operation.
        /// </summary>
        IEnumerable<ILogMessage> LogMessages { get; }

        /// <summary>
        /// Gets the data associated with the current operation; allows individual
        /// components to record arbitrary pieces of data that will be discarded once
        /// the current operation finishes (whether successfully or not).
        /// </summary>
        IDictionary<string, object> Data { get; }

        /// <summary>
        /// Ensures that the specified source system entity identity's last access
        /// correlation ID is updated at the conclusion of the current operation
        /// (deferred).
        /// </summary>
        /// <param name="sourceSystemEntityId">
        /// The ID that uniquely identifies the entity in its source system.
        /// </param>
        void UpdateIdentityCorrelationId(EntityIdentifier sourceSystemEntityId);

        /// <summary>
        /// Completes the underlying ambient transaction.
        /// </summary>
        void Complete();
    }
}
