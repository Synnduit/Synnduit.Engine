using System;

namespace Synnduit
{
    /// <summary>
    /// Used to wrap exceptions thrown by destination systems (sinks, duplication rules,
    /// etc.).
    /// </summary>
    /// <remarks>
    /// Exceptions thrown by destination systems are logged individually and the migration
    /// segment within which they occur is allowed to continue (unlike other exceptions,
    /// which will result in a failure of the entire run).
    /// </remarks>
    internal class DestinationSystemException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="innerException">
        /// The exception thrown by the destination system.
        /// </param>
        public DestinationSystemException(Exception innerException)
            : base(innerException.Message, innerException)
        { }
    }
}
