using System;
using System.ComponentModel.Composition;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// The homogenizer used to ensure that time of day is ignored in comparisons involving
    /// <see cref="DateTime" /> values.
    /// </summary>
    [Export(typeof(IHomogenizer))]
    public sealed class IgnoreDateTimeTimeHomogenizer : Homogenizer<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        [ImportingConstructor]
        internal IgnoreDateTimeTimeHomogenizer()
        { }

        /// <summary>
        /// Trims the time of day component of the specified <see cref="DateTime" /> value.
        /// </summary>
        /// <param name="value">The value to trim.</param>
        /// <returns>The value trimmed of its time of day component.</returns>
        protected override object Homogenize(DateTime value)
        {
            return value.Date;
        }
    }
}
