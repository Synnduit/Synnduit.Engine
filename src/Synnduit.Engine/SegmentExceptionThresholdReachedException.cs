﻿namespace Synnduit
{
    /// <summary>
    /// The exception thrown when the configurable exception threshold to abort a segment is
    /// reached.
    /// </summary>
    internal class SegmentExceptionThresholdReachedException : CountThresholdAbortException
    {
        public SegmentExceptionThresholdReachedException(int threshold)
            : base(threshold)
        { }
    }
}
