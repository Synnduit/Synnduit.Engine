namespace Synnduit
{
    /// <summary>
    /// Provides read-write information relevant to the run segment that's currently being
    /// executed.
    /// </summary>
    internal interface IWritableContext : IContext
    {
        /// <summary>
        /// Sets the underlying <see cref="IContext" /> implementation instance; the
        /// context data will be retrieved from this instance.
        /// </summary>
        /// <param name="context">
        /// The underlying <see cref="IContext" /> implementation instance.
        /// </param>
        void SetContext(IContext context);
    }
}
