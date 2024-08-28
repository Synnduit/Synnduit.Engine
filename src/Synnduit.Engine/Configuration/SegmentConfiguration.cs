namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the configuration of a single run segment.
    /// </summary>
    public class SegmentConfiguration : ConfigurationBase, ISegmentConfiguration
    {
        /// <summary>
        /// Gets or sets the segment type.
        /// </summary>
        public SegmentType Type { get; set; }

        /// <summary>
        /// Gets or sets the entity type.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets the number of exceptions that will trigger an abort; this overrides the global
        /// <see cref="IExceptionHandlingConfiguration.SegmentAbortThreshold"/> value.
        /// </summary>
        public int? SegmentAbortThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the abort triggered by the
        /// <see cref="OrphanMappingPercentageAbortThreshold"/> should be applied to the entire
        /// run; if false or unset, the abort will only apply to the segment; this overrides the
        /// global
        /// <see cref="IExceptionHandlingConfiguration.OrphanMappingPercentageThresholdAbortsRun"/>
        /// value.
        /// </summary>
        public bool? OrphanMappingPercentageThresholdAbortsRun { get; set; }

        /// <summary>
        /// Gets the percentage of orphan mappings that will trigger an abort; this overrides the
        /// global
        /// <see cref="IExceptionHandlingConfiguration.OrphanMappingPercentageAbortThreshold"/>
        /// value.
        /// </summary>
        public double? OrphanMappingPercentageAbortThreshold { get; set; }

        /// <summary>
        /// Gets the percentage of entities identified for garbage collection that will trigger an
        /// abort; this overrides the global
        /// <see cref="IExceptionHandlingConfiguration.GarbageCollectionPercentageAbortThreshold"/>
        /// value.
        /// </summary>
        public double? GarbageCollectionPercentageAbortThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the abort triggered by the
        /// <see cref="GarbageCollectionPercentageAbortThreshold"/> should be applied to the entire
        /// run; if false or unset, the abort will only apply to the segment; this overrides the
        /// global
        /// <see cref="IExceptionHandlingConfiguration.GarbageCollectionPercentageThresholdAbortsRun"/>
        /// value.
        /// </summary>
        public bool? GarbageCollectionPercentageThresholdAbortsRun { get; set; }
    }
}
