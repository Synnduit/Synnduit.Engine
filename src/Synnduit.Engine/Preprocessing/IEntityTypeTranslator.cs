using System;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Performs translations between CLR types representing entities and registered entity
    /// types.
    /// </summary>
    internal interface IEntityTypeTranslator
    {
        /// <summary>
        /// Gets the registered entity type represented by the specified CLR type.
        /// </summary>
        /// <param name="type">The CLR type.</param>
        /// <returns>The entity type registered for the specified CLR type.</returns>
        IEntityType GetEntityType(Type type);
    }
}
