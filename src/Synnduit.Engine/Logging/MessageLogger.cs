using System.ComponentModel.Composition;

namespace Synnduit.Events
{
    /// <summary>
    /// Logs messages associated with the operation that is being processed at a given
    /// time.
    /// </summary>
    [Export(typeof(IMessageLogger))]
    internal class MessageLogger : IMessageLogger
    {
        private readonly IOperationExecutive operationExecutive;

        [ImportingConstructor]
        public MessageLogger(IOperationExecutive operationExecutive)
        {
            this.operationExecutive = operationExecutive;
        }

        /// <summary>
        /// Logs a message associated with the operation that is currently being processed.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="message">The message text.</param>
        public void Log(MessageType type, string message)
        {
            this.operationExecutive.CurrentOperation.Log(type, message);
        }
    }
}
