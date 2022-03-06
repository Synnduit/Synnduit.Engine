using System.Collections.Generic;
using Synnduit.Deduplication;
using Synnduit.Merge;
using Synnduit.Preprocessing;
using Synnduit.Serialization;

namespace Synnduit
{
    /// <summary>
    /// Provides access to configurable services - i.e., those services that are not
    /// directly dependency "injectionable", as multiple implementations may exists
    /// and the ones to be used are registered in the database or chosen in the
    /// configuration file.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    internal interface IServiceProvider<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the entity feed registered for the current source system.
        /// </summary>
        IFeed<TEntity> Feed { get; }

        /// <summary>
        /// Gets the entity sink.
        /// </summary>
        ISink<TEntity> Sink { get; }

        /// <summary>
        /// Gets the entity cache feed.
        /// </summary>
        ICacheFeed<TEntity> CacheFeed { get; }

        /// <summary>
        /// Gets the metadata provider.
        /// </summary>
        IMetadataProvider<TEntity> MetadataProvider { get; }

        /// <summary>
        /// Gets the entity serializer.
        /// </summary>
        ISerializer<TEntity> Serializer { get; }

        /// <summary>
        /// Gets the collection of preprocessor operations to be applied to source system
        /// entities.
        /// </summary>
        IEnumerable<IPreprocessorOperation<TEntity>>
            SourceSystemPreprocessorOperations { get; }

        /// <summary>
        /// Gets the collection of preprocessor operations to be applied to destination
        /// system entities.
        /// </summary>
        IEnumerable<IPreprocessorOperation<TEntity>>
            DestinationSystemPreprocessorOperations { get; }

        /// <summary>
        /// Gets the collection of value homogenizers to be used in deduplication.
        /// </summary>
        IEnumerable<IHomogenizer> Homogenizers { get; }

        /// <summary>
        /// Gets the entity merger.
        /// </summary>
        IMerger<TEntity> Merger { get; }
    }
}
