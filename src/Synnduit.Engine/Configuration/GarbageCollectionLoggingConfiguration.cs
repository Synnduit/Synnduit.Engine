namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the application's garbage collection logging behavior configuration.
    /// </summary>
    public class GarbageCollectionLoggingConfiguration : IGarbageCollectionLoggingConfiguration
    {
        /// <summary>
        /// The default value of the <see cref="Entity" /> property.
        /// </summary>
        public const bool DefaultEntityValue = true;

        /// <summary>
        /// The default value of the <see cref="AlwaysLogMessages" /> property.
        /// </summary>
        public const bool DefaultAlwaysLogMessagesValue = false;

        private bool? entity = null;

        private bool? alwaysLogMessages = null;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public GarbageCollectionLoggingConfiguration()
        { }

        /// <summary>
        /// Gets or sets a comma-separated list of <see cref="EntityDeletionOutcome" />
        /// values identifying entity deletions that should be excluded from logging; the
        /// default is null (i.e., all entity deletions are logged by default).
        /// </summary>
        public string ExcludedOutcomes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the destination system entity that is
        /// being deleted should be recorded in the log; the default is true.
        /// </summary>
        public bool Entity
        {
            get => this.entity ?? DefaultEntityValue;
            set => this.entity = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether an entity deletion should always be
        /// logged if there is at least one message associated with it, (possibly)
        /// effectively overriding the <see cref="ExcludedOutcomes" /> values; the default
        /// is false.
        /// </summary>
        public bool AlwaysLogMessages
        {
            get => this.alwaysLogMessages ?? DefaultAlwaysLogMessagesValue;
            set => this.alwaysLogMessages = value;
        }
    }
}
