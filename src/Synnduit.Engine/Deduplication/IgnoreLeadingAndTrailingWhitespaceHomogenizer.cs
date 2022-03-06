using System.ComponentModel.Composition;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// The homogenizer used to ensure that leading and trailing whitespace is ignored in
    /// string value comparisons.
    /// </summary>
    [Export(typeof(IHomogenizer))]
    public sealed class IgnoreLeadingAndTrailingWhitespaceHomogenizer : Homogenizer<string>
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        [ImportingConstructor]
        internal IgnoreLeadingAndTrailingWhitespaceHomogenizer()
        { }

        /// <summary>
        /// Trims the specified string.
        /// </summary>
        /// <param name="value">The value to trim.</param>
        /// <returns>The specified value trimmed.</returns>
        protected override object Homogenize(string value)
        {
            return value.Trim();
        }
    }
}
