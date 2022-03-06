namespace Synnduit
{
    /// <summary>
    /// Performs individual runs.
    /// </summary>
    public interface IRunner
    {
        /// <summary>
        /// Performs the run that the current instance is configured for.
        /// </summary>
        void Run();
    }
}
