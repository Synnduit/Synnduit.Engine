namespace Synnduit.Deployment
{
    /// <summary>
    /// Assembles the suite of assets (i.e., external systems, entity types, feeds, etc.)
    /// found in all currently loaded assemblies.
    /// </summary>
    internal interface IAssetAssembler
    {
        /// <summary>
        /// Assembles the suite of assets (i.e., external systems, entity types, feeds,
        /// etc.) found in all currently loaded assemblies.
        /// </summary>
        /// <returns>The suite of assets assembled.</returns>
        AssetSuite AssembleAssets();
    }
}
