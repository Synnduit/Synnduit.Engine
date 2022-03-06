namespace Synnduit
{
    /// <summary>
    /// Parses individual entity types' metadata information defined by attributes applied
    /// to the types' properties and via
    /// <see cref="IEntityTypeDefinitionContext{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IMetadataParser<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets entity type metadata for the class represented by the current instance.
        /// </summary>
        EntityTypeMetadata Metadata { get; }
    }
}
