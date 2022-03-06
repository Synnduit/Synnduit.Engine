using System;
using System.Collections.Generic;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Represents a combined asset; i.e., an asset-representing type with a one or more
    /// asset attribute instances applied to it.
    /// </summary>
    /// <typeparam name="TAttribute">The asset attribute type.</typeparam>
    internal class CombinedAsset<TAttribute>
    {
        public CombinedAsset(Type assetType, IEnumerable<TAttribute> assetAttributes)
        {
            this.AssetType = assetType;
            this.AssetAttributes = assetAttributes;
        }

        /// <summary>
        /// Gets the asset-representing type.
        /// </summary>
        public Type AssetType { get; }

        /// <summary>
        /// Gets the asset attributes applied to the type.
        /// </summary>
        public IEnumerable<TAttribute> AssetAttributes { get; }
    }
}
