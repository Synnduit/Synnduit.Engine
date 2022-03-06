namespace Synnduit
{
    /// <summary>
    /// Validates information exposed by an <see cref="IContext" /> implementation for a
    /// specific run segment type.
    /// </summary>
    /// <remarks>
    /// Different run segment types may require different pieces of information to be
    /// exposed; individual classes implementing this interface contain the intelligence to
    /// determine whether a specific <see cref="IContext" /> is in a valid state for the
    /// run segment type they represent.
    /// </remarks>
    internal interface IContextValidator
    {
        /// <summary>
        /// Validates the specified <see cref="IContext" /> instance.
        /// </summary>
        /// <param name="context">The <see cref="IContext" /> instance to validate.</param>
        /// <remarks>
        /// The method should throw an exception, if the specified instance is in an
        /// invalid state.
        /// </remarks>
        void Validate(IContext context);
    }
}
