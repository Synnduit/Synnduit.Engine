using System.ComponentModel.Composition;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// The homogenizer used to ensure string values are compared in a case-insensitive
    /// fashion.
    /// </summary>
    [Export(typeof(IHomogenizer))]
    public sealed class CaseInsensitiveComparisonHomogenizer : Homogenizer<string>
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        [ImportingConstructor]
        internal CaseInsensitiveComparisonHomogenizer()
        { }

        /// <summary>
        /// Converts the specified string to uppercase.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The specified value converted to uppercase.</returns>
        protected override object Homogenize(string value)
        {
            return value.ToUpper();
        }
    }
}
