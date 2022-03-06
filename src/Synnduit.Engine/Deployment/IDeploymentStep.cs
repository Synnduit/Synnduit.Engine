namespace Synnduit.Deployment
{
    /// <summary>
    /// To be implemented and exported by classes that need to deploy external systems,
    /// entity types, etc.
    /// </summary>
    internal interface IDeploymentStep
    {
        /// <summary>
        /// Executes the deployment step represented by the current instance.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IDeploymentContext" /> instance to use.
        /// </param>
        void Execute(IDeploymentContext context);
    }
}
