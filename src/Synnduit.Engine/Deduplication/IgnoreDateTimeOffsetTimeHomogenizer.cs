using System;
using System.ComponentModel.Composition;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// The homogenizer used to ensure that time of day is ignored in comparisons involving
    /// <see cref="DateTimeOffset" /> values.
    /// </summary>
    [Export(typeof(IHomogenizer))]
    public sealed class IgnoreDateTimeOffsetTimeHomogenizer : Homogenizer<DateTimeOffset>
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        [ImportingConstructor]
        internal IgnoreDateTimeOffsetTimeHomogenizer()
        { }

        /// <summary>
        /// Trims the time of day component of the specified <see cref="DateTimeOffset" />
        /// value.
        /// </summary>
        /// <param name="value">The value to trim.</param>
        /// <returns>The value trimmed of its time of day component.</returns>
        protected override object Homogenize(DateTimeOffset value)
        {
            return value.Date;
        }
    }
}
