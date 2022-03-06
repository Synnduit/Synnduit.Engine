using System;

namespace Synnduit.Deployment
{
    /// <summary>
    /// Contains extension methods for the <see cref="Type" /> class.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets the full name of the type stripped of any generic parameter notation.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The full name of the type stripped of any generic parameter notation.
        /// </returns>
        public static string GetStrippedFullName(this Type type)
        {
            string fullName = null;
            if(type != null)
            {
                fullName = type.FullName;
                int indexOfAccentMark = fullName.IndexOf('`');
                if(indexOfAccentMark >= 0)
                {
                    fullName = fullName.Substring(0, indexOfAccentMark);
                }
            }
            return fullName;
        }
    }
}
