using System;
using System.ComponentModel.Composition;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Validates that the information exposed by an <see cref="IContext" /> instance is
    /// valid for the <see cref="SegmentType.Migration" /> run segment type.
    /// </summary>
    [Export(typeof(IMigrationContextValidator))]
    internal class MigrationContextValidator : IMigrationContextValidator
    {
        /// <summary>
        /// Validates the specified <see cref="IContext" /> instance.
        /// </summary>
        /// <param name="context">The <see cref="IContext" /> instance to validate.</param>
        public void Validate(IContext context)
        {
            if(context.SourceSystem == null)
            {
                throw new InvalidOperationException(Resources.SourceSystemRequired);
            }
        }
    }
}
