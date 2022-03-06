namespace Synnduit.Serialization
{
    /// <summary>
    /// Provides access to the hashing algorithm used by the application.
    /// </summary>
    internal interface IHashFunction
    {
        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="data">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        string ComputeHash(byte[] data);

        /// <summary>
        /// Computes the hash value for the specified Unicode string.
        /// </summary>
        /// <param name="value">The Unicode string to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        string ComputeHash(string value);
    }
}
