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
    }
}
