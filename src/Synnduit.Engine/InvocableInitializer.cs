using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Synnduit.Events;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Represents the initializer that exposes the Initialize method, in addition to
    /// allowing individual classes to register for initialization.
    /// </summary>
    [Export(typeof(IInvocableInitializer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class InvocableInitializer : IInvocableInitializer
    {
        private readonly List<InitializableInstanceWrapper> initializableInstances;

        [ImportingConstructor]
        public InvocableInitializer()
        {
            this.initializableInstances = new List<InitializableInstanceWrapper>();
        }

        /// <summary>
        /// Registers the specified instance for invocation during the initialization
        /// process; should be called in individual classes' constructors.
        /// </summary>
        /// <param name="instance">The initializable instance to be invoked.</param>
        /// <param name="suppressEvents">
        /// A value indicating whether initialization events (i.e., initializing and
        /// initialized) should be suppressed.
        /// </param>
        /// <param name="message">
        /// An optional message to be sent to the initialization event receivers.
        /// </param>
        /// <param name="data">
        /// An optional dictionary of data values to be sent to the initialization event
        /// receivers.
        /// </param>
        public void Register(
            IInitializable instance,
            bool suppressEvents,
            string message,
            IDictionary<string, object> data)
        {
            this.initializableInstances.Add(
                new InitializableInstanceWrapper(
                    instance, suppressEvents, message, data));
        }

        /// <summary>
        /// Initializes individual registered classes.
        /// </summary>
        /// <param name="eventDispatcher">
        /// The <see cref="IEventDispatcher" /> instance to use.
        /// </param>
        public void Initialize(IEventDispatcher eventDispatcher)
        {
            foreach(InitializableInstanceWrapper
                initializableInstance in this.initializableInstances)
            {
                var context = new InitializationContext(
                    initializableInstance.SuppressEvents,
                    initializableInstance.Data);
                this.OnInitializing(eventDispatcher, initializableInstance);
                this.Initialize(initializableInstance, context);
                this.OnInitialized(eventDispatcher, initializableInstance, context);
            }
        }

        private void OnInitializing(
            IEventDispatcher eventDispatcher,
            InitializableInstanceWrapper initializableInstance)
        {
            if(!initializableInstance.SuppressEvents)
            {
                eventDispatcher.Initializing(new InitializingArgs(
                    initializableInstance.Instance.GetType(),
                    initializableInstance.Data,
                    initializableInstance.Message));
            }
        }

        private void Initialize(
            InitializableInstanceWrapper initializableInstance,
            InitializationContext context)
        {
            try
            {
                initializableInstance.Instance.Initialize(context);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    Resources.InitializableThrewException, exception);
            }
        }

        private void OnInitialized(
            IEventDispatcher eventDispatcher,
            InitializableInstanceWrapper initializableInstance,
            InitializationContext context)
        {
            if(!initializableInstance.SuppressEvents)
            {
                eventDispatcher.Initialized(new InitializedArgs(
                    initializableInstance.Instance.GetType(),
                    context.Data,
                    initializableInstance.Message,
                    context.Message));
            }
        }

        private class InitializableInstanceWrapper
        {
            public InitializableInstanceWrapper(
                IInitializable instance,
                bool suppressEvents,
                string message,
                IDictionary<string, object> data)
            {
                this.Instance = instance;
                this.SuppressEvents = suppressEvents;
                this.Message = message;
                this.Data =
                    data != null
                    ? new Dictionary<string, object>(data)
                    : new Dictionary<string, object>();
            }

            public IInitializable Instance { get; }

            public bool SuppressEvents { get; }

            public string Message { get; }

            public IDictionary<string, object> Data { get; }
        }

        private class InitializationContext : IInitializationContext
        {
            private readonly bool suppressEvents;

            private string message;

            private readonly IDictionary<string, object> data;

            public InitializationContext(
                bool suppressEvents, IDictionary<string, object> data)
            {
                this.suppressEvents = suppressEvents;
                this.message = null;
                this.data = data;
            }

            public string Message
            {
                get
                {
                    this.VerifyEventsNotSuppressed();
                    return this.message;
                }
                set
                {
                    this.VerifyEventsNotSuppressed();
                    this.message = value;
                }
            }

            public IDictionary<string, object> Data
            {
                get
                {
                    this.VerifyEventsNotSuppressed();
                    return this.data;
                }
            }

            private void VerifyEventsNotSuppressed()
            {
                if(this.suppressEvents)
                {
                    throw new InvalidOperationException(
                        Resources.InitializationEventsSuppressed);
                }
            }
        }

        private abstract class InitializationArgs : IInitializationArgs
        {
            private readonly Type initializableType;

            private readonly IReadOnlyDictionary<string, object> data;

            protected InitializationArgs(
                Type initializableType, IDictionary<string, object> data)
            {
                this.initializableType = initializableType;
                this.data = new ReadOnlyDictionary<string, object>(data);
            }

            public Type InitializableType
            {
                get { return this.initializableType; }
            }

            public IReadOnlyDictionary<string, object> Data
            {
                get { return this.data; }
            }
        }

        private class InitializingArgs : InitializationArgs, IInitializingArgs
        {
            private readonly string message;

            public InitializingArgs(
                Type initializableType,
                IDictionary<string, object> data,
                string message)
                : base(initializableType, data)
            {
                this.message = message;
            }

            public string Message
            {
                get { return this.message; }
            }
        }

        private class InitializedArgs : InitializationArgs, IInitializedArgs
        {
            private readonly string initializingMessage;

            private readonly string message;

            public InitializedArgs(
                Type initializableType,
                IDictionary<string, object> data,
                string initializingMessage,
                string message)
                : base(initializableType, data)
            {
                this.initializingMessage = initializingMessage;
                this.message = message;
            }

            public string InitializingMessage
            {
                get { return this.initializingMessage; }
            }

            public string Message
            {
                get { return this.message; }
            }
        }
    }
}
