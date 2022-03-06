namespace Synnduit
{
    /// <summary>
    /// Manages information for the processing of a given operation.
    /// </summary>
    internal interface IOperationExecutive
    {
        /// <summary>
        /// Creates a new operation and makes it the current one.
        /// </summary>
        /// <returns>
        /// The <see cref="IOperationScope" /> instance representing the operation created.
        /// </returns>
        IOperationScope CreateOperation();

        /// <summary>
        /// Gets the <see cref="IOperationScope" /> representing the current operation.
        /// </summary>
        IOperationScope CurrentOperation { get; }
    }
}
