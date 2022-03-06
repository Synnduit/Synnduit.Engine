namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the application's migration logging behavior configuration.
    /// </summary>
    public class MigrationLoggingConfiguration : IMigrationLoggingConfiguration
    {
        /// <summary>
        /// The default value of the <see cref="ExcludedOutcomes" /> property.
        /// </summary>
        public const string DefaultExcludedOutcomesValue = "Skipped, NoChangesDetected";

        /// <summary>
        /// The default value of the <see cref="SourceSystemEntity" /> property.
        /// </summary>
        public const bool DefaultSourceSystemEntityValue = true;

        /// <summary>
        /// The default value of the <see cref="DestinationSystemEntity" /> property.
        /// </summary>
        public const bool DefaultDestinationSystemEntityValue = false;

        /// <summary>
        /// The default value of the <see cref="ValueChanges" /> property.
        /// </summary>
        public const bool DefaultValueChangesValue = true;

        /// <summary>
        /// The default value of the <see cref="AlwaysLogMessages" /> property.
        /// </summary>
        public const bool DefaultAlwaysLogMessagesValue = false;

        private string excludedOutcomes = null;

        private bool? sourceSystemEntity = null;

        private bool? destinationSystemEntity = null;

        private bool? valueChanges = null;

        private bool? alwaysLogMessages = null;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public MigrationLoggingConfiguration()
        { }

        /// <summary>
        /// Gets or sets a comma-separated list of
        /// <see cref="EntityTransactionOutcome"/> values identifying entity transactions
        /// that should be excluded from logging; the default is
        /// <see cref="EntityTransactionOutcome.Skipped" /> and
        /// <see cref="EntityTransactionOutcome.NoChangesDetected"/>.
        /// </summary>
        public string ExcludedOutcomes
        {
            get => this.excludedOutcomes ?? DefaultExcludedOutcomesValue;
            set => this.excludedOutcomes = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the source system entity should be
        /// recorded in the log; the default is true.
        /// </summary>
        public bool SourceSystemEntity
        {
            get => this.sourceSystemEntity ?? DefaultSourceSystemEntityValue;
            set => this.sourceSystemEntity = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the destination system entity should be
        /// recorded in the log; the default is false.
        /// </summary>
        public bool DestinationSystemEntity
        {
            get => this.destinationSystemEntity ?? DefaultDestinationSystemEntityValue;
            set => this.destinationSystemEntity = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether individual value changes should be
        /// recorded in the log; the default is true.
        /// </summary>
        public bool ValueChanges
        {
            get => this.valueChanges ?? DefaultValueChangesValue;
            set => this.valueChanges = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether an entity transaction should always be
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
