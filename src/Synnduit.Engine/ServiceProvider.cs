using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Deduplication;
using Synnduit.Merge;
using Synnduit.Persistence;
using Synnduit.Preprocessing;
using Synnduit.Properties;
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
    [Export(typeof(IServiceProvider<>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class ServiceProvider<TEntity> : IServiceProvider<TEntity>
        where TEntity : class
    {
        private readonly IContext context;

        private readonly ISafeRepository safeRepository;

        private readonly IEnumerable<IFeed<TEntity>> feeds;

        private readonly IEnumerable<ISink<TEntity>> sinks;

        private readonly IEnumerable<ICacheFeed<TEntity>> cacheFeeds;

        private readonly IEnumerable<IMetadataProvider<TEntity>> metadataProviders;

        private readonly IEnumerable<ISerializer<TEntity>> serializers;

        private readonly IEnumerable<
            IPreprocessorOperation<TEntity>> preprocessorOperations;

        private readonly IEnumerable<IHomogenizer> homogenizers;

        private readonly IEnumerable<IMerger<TEntity>> mergers;

        private readonly Lazy<string> feedTypeFullName;

        private readonly Lazy<IFeed<TEntity>> feed;

        private readonly Lazy<Persistence.IEntityType> entityType;

        private readonly Lazy<ISink<TEntity>> sink;

        private readonly Lazy<ICacheFeed<TEntity>> cacheFeed;

        [ImportingConstructor]
        public ServiceProvider(
            IContext context,
            ISafeRepository dataRepository,
            [ImportMany] IEnumerable<IFeed<TEntity>> feeds,
            [ImportMany] IEnumerable<ISink<TEntity>> sinks,
            [ImportMany] IEnumerable<ICacheFeed<TEntity>> cacheFeeds,
            [ImportMany] IEnumerable<IMetadataProvider<TEntity>> metadataProviders,
            [ImportMany] IEnumerable<ISerializer<TEntity>> serializers,
            [ImportMany] IEnumerable<
                IPreprocessorOperation<TEntity>> preprocessorOperations,
            [ImportMany] IEnumerable<IHomogenizer> homogenizers,
            [ImportMany] IEnumerable<IMerger<TEntity>> mergers)
        {
            this.context = context;
            this.safeRepository = dataRepository;
            this.feeds = feeds;
            this.sinks = sinks;
            this.cacheFeeds = cacheFeeds;
            this.metadataProviders = metadataProviders;
            this.serializers = serializers;
            this.preprocessorOperations = preprocessorOperations;
            this.homogenizers = homogenizers;
            this.mergers = mergers;
            this.feedTypeFullName = new Lazy<string>(this.GetFeedTypeFullName);
            this.feed = new Lazy<IFeed<TEntity>>(this.GetFeed);
            this.entityType = new Lazy<Persistence.IEntityType>(this.GetEntityType);
            this.sink = new Lazy<ISink<TEntity>>(this.GetSink);
            this.cacheFeed = new Lazy<ICacheFeed<TEntity>>(this.GetCacheFeed);
        }

        /// <summary>
        /// Gets the entity feed.
        /// </summary>
        public IFeed<TEntity> Feed
        {
            get { return this.feed.Value; }
        }

        /// <summary>
        /// Gets the entity sink.
        /// </summary>
        public ISink<TEntity> Sink
        {
            get { return this.sink.Value; }
        }

        /// <summary>
        /// Gets the entity cache feed.
        /// </summary>
        public ICacheFeed<TEntity> CacheFeed
        {
            get { return this.cacheFeed.Value; }
        }

        /// <summary>
        /// Gets the metadata provider.
        /// </summary>
        public IMetadataProvider<TEntity> MetadataProvider
        {
            get { return this.metadataProviders.First(); }
        }

        /// <summary>
        /// Gets the entity serializer.
        /// </summary>
        public ISerializer<TEntity> Serializer
        {
            get { return this.serializers.First(); }
        }

        /// <summary>
        /// Gets the collection of preprocessor operations to be applied to source system
        /// entities.
        /// </summary>
        public IEnumerable<IPreprocessorOperation<TEntity>>
            SourceSystemPreprocessorOperations
        {
            get { return this.preprocessorOperations; }
        }

        /// <summary>
        /// Gets the collection of preprocessor operations to be applied to destination
        /// system entities.
        /// </summary>
        public IEnumerable<IPreprocessorOperation<TEntity>>
            DestinationSystemPreprocessorOperations
        {
            get { return this.preprocessorOperations; }
        }

        /// <summary>
        /// Gets the collection of value homogenizers to be used in deduplication.
        /// </summary>
        public IEnumerable<IHomogenizer> Homogenizers
        {
            get { return this.homogenizers; }
        }

        /// <summary>
        /// Gets the entity merger.
        /// </summary>
        public IMerger<TEntity> Merger
        {
            get { return this.mergers.First(); }
        }

        private string GetFeedTypeFullName()
        {
            string feedTypeFullName = this.safeRepository.GetFeedTypeFullName(
                this.context.EntityType.Id,
                this.context.SourceSystem.Id);
            if(feedTypeFullName == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.FeedNotRegistered,
                    this.context.EntityType.Name,
                    this.context.SourceSystem.Name));
            }
            return feedTypeFullName;
        }

        private IFeed<TEntity> GetFeed()
        {
            return this.GetService(
                this.feeds,
                this.feedTypeFullName.Value,
                Resources.FeedNotFound);
        }

        private Persistence.IEntityType GetEntityType()
        {
            return this.safeRepository.GetEntityType(this.context.EntityType.Id);
        }

        private ISink<TEntity> GetSink()
        {
            return this.GetService(
                this.sinks,
                this.entityType.Value.SinkTypeFullName,
                Resources.SinkNotFound);
        }

        private ICacheFeed<TEntity> GetCacheFeed()
        {
            ICacheFeed<TEntity> cacheFeed = null;
            if(this.entityType.Value.CacheFeedTypeFullName != null)
            {
                cacheFeed = this.GetService(
                    this.cacheFeeds,
                    this.entityType.Value.CacheFeedTypeFullName,
                    Resources.CacheFeedNotFound);
            }
            return cacheFeed;
        }

        private TService GetService<TService>(
            IEnumerable<TService> services,
            string typeName,
            string exceptionMessageFormat)
        {
            try
            {
                return
                    services
                    .Single(service =>
                        service.GetType().FullName == typeName ||
                        service.GetType().FullName.StartsWith(typeName + "`"));
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException(
                    string.Format(exceptionMessageFormat, typeName),
                    exception);
            }
        }
    }
}
