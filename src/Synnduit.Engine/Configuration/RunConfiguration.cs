namespace Synnduit.Configuration
{
    /// <summary>
    /// Represents the configuration of a single run.
    /// </summary>
    public class RunConfiguration : ConfigurationBase, IRunConfiguration
    {
        private SegmentConfiguration[] segments = null;

        /// <summary>
        /// Gets or sets the name of the run.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ordered array of the segments that comprise the run.
        /// </summary>
        public SegmentConfiguration[] Segments
        {
            get => this.segments.SafeToArray();
            set => this.segments = value;
        }

        IEnumerable<ISegmentConfiguration> IRunConfiguration.Segments => this.Segments;
    }
}
