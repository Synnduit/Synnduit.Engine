using System;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Represents a singular asset; i.e., an asset-representing type with a single asset
    /// attribute instance applied to it.
    /// </summary>
    /// <typeparam name="TAttribute">The asset attribute type.</typeparam>
    internal class SingularAsset<TAttribute>
    {
        public SingularAsset(Type assetType, TAttribute assetAttribute)
        {
            this.AssetType = assetType;
            this.AssetAttribute = assetAttribute;
        }

        /// <summary>
        /// Gets the asset-representing type.
        /// </summary>
        public Type AssetType { get; }

        /// <summary>
        /// Gets the asset attribute applied to the type.
        /// </summary>
        public TAttribute AssetAttribute { get; }
    }
}
