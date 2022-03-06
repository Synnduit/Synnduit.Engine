using System.ComponentModel.Composition;

namespace Synnduit
{
    /// <summary>
    /// Validates that the information exposed by an <see cref="IContext" /> instance is
    /// valid for the <see cref="SegmentType.GarbageCollection" /> run segment type.
    /// </summary>
    [Export(typeof(IGarbageCollectionContextValidator))]
    internal class GarbageCollectionContextValidator : IGarbageCollectionContextValidator
    {
        /// <summary>
        /// Validates the specified <see cref="IContext" /> instance.
        /// </summary>
        /// <param name="context">The <see cref="IContext" /> instance to validate.</param>
        public void Validate(IContext context)
        {
        }
    }
}
