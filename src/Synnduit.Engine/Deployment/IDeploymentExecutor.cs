namespace Synnduit.Deployment
{
    /// <summary>
    /// Executes individual deployment steps to ensure that the database is ready for
    /// migration runs.
    /// </summary>
    public interface IDeploymentExecutor
    {
        /// <summary>
        /// Executes individual deployment steps to ensure that the database is ready for
        /// migration runs.
        /// </summary>
        void Deploy();
    }
}
