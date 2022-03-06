using System;
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;

namespace Synnduit.Serialization
{
    /// <summary>
    /// Provides access to the hashing algorithm used by the application.
    /// </summary>
    [Export(typeof(IHashFunction))]
    internal class HashFunction : IHashFunction
    {
        private readonly MD5 hashFunction;

        [ImportingConstructor]
        public HashFunction()
        {
            this.hashFunction = MD5.Create();
        }

        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="data">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public string ComputeHash(byte[] data)
        {
            return Convert.ToBase64String(this.hashFunction.ComputeHash(data));
        }

        /// <summary>
        /// Computes the hash value for the specified Unicode string.
        /// </summary>
        /// <param name="value">The Unicode string to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public string ComputeHash(string value)
        {
            return this.ComputeHash(Encoding.Unicode.GetBytes(value));
        }
    }
}
