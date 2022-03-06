using System;

namespace Synnduit.Events
{
    /// <summary>
    /// The base implementation for the <see cref="IOperationArgs" /> interface.
    /// </summary>
    internal abstract class OperationArgs : IOperationArgs
    {
        private readonly DateTimeOffset timeStamp;

        protected OperationArgs(DateTimeOffset timeStamp)
        {
            this.timeStamp = timeStamp;
        }

        /// <summary>
        /// Gets the operation's time stamp.
        /// </summary>
        public DateTimeOffset TimeStamp
        {
            get { return this.timeStamp; }
        }
    }
}
