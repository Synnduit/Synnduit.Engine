using System;
using System.Collections.Generic;

namespace Synnduit
{
    /// <summary>
    /// Manages the dependency injection (MEF) container and provides access to interface
    /// implementations.
    /// </summary>
    internal interface IBootstrapper : IDisposable
    {
        /// <summary>
        /// Gets the implementation of the specified (interface) type.
        /// </summary>
        /// <typeparam name="T">
        /// The (interface) type whose implementation is requested.
        /// </typeparam>
        /// <returns>The implementation of the specified (interface) type.</returns>
        T Get<T>();

        /// <summary>
        /// Gets all implementations of the specified (interface) type.
        /// </summary>
        /// <typeparam name="T">
        /// The (interface) type whose implementations are requested.
        /// </typeparam>
        /// <returns>The implementations of the specified (interface) type.</returns>
        IEnumerable<T> GetMany<T>();
    }
}
