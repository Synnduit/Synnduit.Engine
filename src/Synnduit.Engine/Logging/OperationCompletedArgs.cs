using System;
using System.Collections.Generic;

namespace Synnduit.Events
{
    /// <summary>
    /// The base implementation for the <see cref="IOperationCompletedArgs" /> interface.
    /// </summary>
    internal abstract class OperationCompletedArgs : OperationArgs, IOperationCompletedArgs
    {
        private readonly IEnumerable<ILogMessage> logMessages;

        private readonly Exception exception;

        public OperationCompletedArgs(
            DateTimeOffset timeStamp,
            IEnumerable<ILogMessage> logMessages,
            Exception exception)
            : base(timeStamp)
        {
            this.logMessages = logMessages;
            this.exception = exception;
        }

        /// <summary>
        /// Gets the collection of log messages associated with the operation.
        /// </summary>
        public IEnumerable<ILogMessage> LogMessages
        {
            get { return this.logMessages; }
        }

        /// <summary>
        /// Gets the exception that the operation resulted in; a null reference will be
        /// returned if this is not applicable.
        /// </summary>
        public Exception Exception
        {
            get { return this.exception; }
        }
    }
}
