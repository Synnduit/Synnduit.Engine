using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Properties;

namespace Synnduit.Preprocessing
{
    /// <summary>
    /// Performs translations between CLR types representing entities and registered entity
    /// types.
    /// </summary>
    [Export(typeof(IEntityTypeTranslator))]
    internal class EntityTypeTranslator : IEntityTypeTranslator
    {
        private readonly IContext context;

        private readonly Lazy<IDictionary<Type, IEntityType>> entityTypes;

        [ImportingConstructor]
        public EntityTypeTranslator(IContext context)
        {
            this.context = context;
            this.entityTypes =
                new Lazy<IDictionary<Type, IEntityType>>(
                    this.GenerateEntityTypesDictionary);
        }

        /// <summary>
        /// Gets the registered entity type represented by the specified CLR type.
        /// </summary>
        /// <param name="type">The CLR type.</param>
        /// <returns>The entity type registered for the specified CLR type.</returns>
        public IEntityType GetEntityType(Type type)
        {
            if(this.entityTypes.Value.TryGetValue(
                type, out IEntityType entityType) == false)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.EntityTypeNotRegistered,
                    type.FullName));
            }
            return entityType;
        }

        private IDictionary<Type, IEntityType> GenerateEntityTypesDictionary()
        {
            return
                this
                .context
                .EntityTypes
                .GroupBy(entityType => entityType.Type)
                .Where(entityTypes => entityTypes.Count() == 1)
                .ToDictionary(
                    entityTypes => entityTypes.Key,
                    entityTypes => entityTypes.Single());
        }
    }
}
