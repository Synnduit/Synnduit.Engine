﻿using Synnduit.Configuration;
using System.ComponentModel.Composition;

namespace Synnduit
{
    /// <summary>
    /// Handles entity-processing (migration/garbage collection) exceptions.
    /// </summary>
    [Export(typeof(IExceptionHandler))]
    internal class ExceptionHandler : IExceptionHandler
    {
        private const string RunExceptionCountKey = "RunExceptionCount";

        private readonly IContext context;

        private readonly IConfigurationProvider configurationProvider;

        [ImportingConstructor]
        public ExceptionHandler(IContext context, IConfigurationProvider configurationProvider)
        {
            this.context = context;
            this.configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Processes the specified entity transaction outcome.
        /// </summary>
        /// <param name="outcome">The entity transaction outcome.</param>
        /// <param name="segmentExceptionCount">
        /// The number of exceptions thrown in the current segment; will be incremented if
        /// <paramref name="outcome"/> is <see cref="EntityTransactionOutcome.ExceptionThrown"/>.
        /// </param>
        public void ProcessEntityTransactionOutcome(
            EntityTransactionOutcome outcome, ref int segmentExceptionCount)
        {
            if (outcome == EntityTransactionOutcome.ExceptionThrown)
            {
                this.HandleException(ref segmentExceptionCount);
            }
        }

        /// <summary>
        /// Processes the specified entity deletion outcome.
        /// </summary>
        /// <param name="outcome">The entity deletion outcome.</param>
        /// <param name="segmentExceptionCount">
        /// The number of exceptions thrown in the current segment; will be incremented if
        /// <paramref name="outcome"/> is <see cref="EntityDeletionOutcome.ExceptionThrown"/>.
        /// </param>
        public void ProcessEntityDeletionOutcome(
            EntityDeletionOutcome outcome, ref int segmentExceptionCount)
        {
            if (outcome == EntityDeletionOutcome.ExceptionThrown)
            {
                this.HandleException(ref segmentExceptionCount);
            }
        }

        private void HandleException(ref int segmentExceptionCount)
        {
            int runExceptionCount = IncrementRunExceptionCount();
            segmentExceptionCount++;
            if (runExceptionCount >=
                this
                .configurationProvider
                .ApplicationConfiguration
                .ExceptionHandling
                .RunAbortThreshold)
            {
                throw new RunExceptionThresholdReachedException((int)
                    this
                    .configurationProvider
                    .ApplicationConfiguration
                    .ExceptionHandling
                    .RunAbortThreshold);
            }
            int? segmentAbortThreshold =
                this.context.SegmentConfiguration.SegmentAbortThreshold ??
                this.configurationProvider
                    .ApplicationConfiguration
                    .ExceptionHandling
                    .SegmentAbortThreshold;
            if (segmentExceptionCount >= segmentAbortThreshold)
            {
                throw new SegmentExceptionThresholdReachedException((int)segmentAbortThreshold);
            }

            int IncrementRunExceptionCount()
            {
                this.context.RunData.TryGetValue(
                    RunExceptionCountKey, out object runExceptionCountDataValue);
                this.context.RunData[RunExceptionCountKey] =
                    runExceptionCountDataValue is int runExceptionCount
                    ? runExceptionCount + 1
                    : 1;
                return (int)this.context.RunData[RunExceptionCountKey];
            }
        }
    }
}
