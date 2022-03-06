using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Transactions;
using Synnduit.Events;
using Synnduit.Mappings;
using Synnduit.Persistence;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Manages information for the processing of a given operation.
    /// </summary>
    [Export(typeof(IOperationExecutive))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class OperationExecutive : IOperationExecutive
    {
        private const string CorrelationIdUpdateSourceSystemEntityIdDataKey =
            "CorrelationIdUpdateSourceSystemEntityId";

        private readonly IContext context;

        private readonly ISafeRepository safeRepository;

        private OperationScope currentOperation;

        [ImportingConstructor]
        public OperationExecutive(IContext context, ISafeRepository safeRepository)
        {
            this.context = context;
            this.safeRepository = safeRepository;
            this.currentOperation = null;
        }

        /// <summary>
        /// Creates a new operation and makes it the current one.
        /// </summary>
        /// <returns>
        /// The <see cref="IOperationScope" /> instance representing the operation created.
        /// </returns>
        public IOperationScope CreateOperation()
        {
            if(this.currentOperation != null)
            {
                throw new InvalidOperationException(Resources.OperationInProgress);
            }
            this.currentOperation = new OperationScope(this);
            return this.currentOperation;
        }

        /// <summary>
        /// Gets the <see cref="IOperationScope" /> representing the current operation.
        /// </summary>
        public IOperationScope CurrentOperation
        {
            get
            {
                if(this.currentOperation == null)
                {
                    throw new InvalidOperationException(Resources.OperationNotInProgress);
                }
                return this.currentOperation;
            }
        }

        private class OperationScope : IOperationScope
        {
            private readonly OperationExecutive parent;

            private readonly List<ILogMessage> logMessages;

            private readonly TransactionScope transactionScope;

            public OperationScope(OperationExecutive parent)
            {
                this.parent = parent;
                this.logMessages = new List<ILogMessage>();
                this.transactionScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        Timeout = TransactionManager.MaximumTimeout,
                        IsolationLevel = IsolationLevel.RepeatableRead
                    });
                this.Id = Guid.NewGuid();
                this.TimeStamp = DateTimeOffset.Now;
                this.Data = new Dictionary<string, object>();
            }

            public IEnumerable<ILogMessage> LogMessages
            {
                get { return this.logMessages; }
            }

            public IDictionary<string, object> Data { get; }

            public void UpdateIdentityCorrelationId(EntityIdentifier sourceSystemEntityId)
            {
                EntityIdentifier currentSourceSystemEntityId =
                    this.GetCorrelationIdUpdateSourceSystemEntityId();
                if(currentSourceSystemEntityId != null
                    && currentSourceSystemEntityId != sourceSystemEntityId)
                {
                    throw new InvalidOperationException(
                        Resources.CannotChangeCorrelationIdUpdateSourceSystemEntityId);
                }
                this.Data[CorrelationIdUpdateSourceSystemEntityIdDataKey]
                    = sourceSystemEntityId;
            }

            public void Complete()
            {
                this.PerformIdentityCorrelationIdUpdate();
                this.transactionScope.Complete();
            }

            public Guid Id { get; }

            public DateTimeOffset TimeStamp { get; }

            public void Log(MessageType type, string message)
            {
                this.ValidateMessageType(type);
                ArgumentValidator.EnsureArgumentNotNullOrWhiteSpace(
                    message, nameof(message));
                this.logMessages.Add(new LogMessage(type, message));
            }

            public void Dispose()
            {
                this.transactionScope.Dispose();
                this.parent.currentOperation = null;
            }

            private EntityIdentifier GetCorrelationIdUpdateSourceSystemEntityId()
            {
                this.Data.TryGetValue(
                    CorrelationIdUpdateSourceSystemEntityIdDataKey,
                    out object correlationIdUpdateSourceSystemEntityId);
                return (EntityIdentifier) correlationIdUpdateSourceSystemEntityId;
            }

            private void PerformIdentityCorrelationIdUpdate()
            {
                EntityIdentifier correlationIdUpdateSourceSystemEntityId
                    = this.GetCorrelationIdUpdateSourceSystemEntityId();
                if(correlationIdUpdateSourceSystemEntityId != null)
                {
                    this.parent
                        .safeRepository
                        .UpdateIdentityCorrelationId(
                            new OperationIdIdentity(
                                this.Id,
                                this.parent.context.EntityType.Id,
                                this.parent.context.SourceSystem.Id,
                                correlationIdUpdateSourceSystemEntityId));
                }
            }

            private void ValidateMessageType(MessageType type)
            {
                if(!Enum.IsDefined(typeof(MessageType), type))
                {
                    throw new ArgumentException(
                        string.Format(Resources.InvalidMessageType, type),
                        nameof(type));
                }
            }
        }

        private class LogMessage : ILogMessage
        {
            private readonly MessageType type;

            private readonly string message;

            public LogMessage(MessageType type, string message)
            {
                this.type = type;
                this.message = message;
            }

            public MessageType Type
            {
                get { return this.type; }
            }

            public string Message
            {
                get { return this.message; }
            }
        }

        private class OperationIdIdentity : IOperationIdIdentity
        {
            public OperationIdIdentity(
                Guid operationId,
                Guid entityTypeId,
                Guid sourceSystemId,
                string sourceSystemEntityId)
            {
                this.OperationId = operationId;
                this.Identity = new SourceSystemEntityIdentity(
                    entityTypeId, sourceSystemId, sourceSystemEntityId);
            }

            public Guid OperationId { get; }

            public ISourceSystemEntityIdentity Identity { get; }
        }
    }
}
