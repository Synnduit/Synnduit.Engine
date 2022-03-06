using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Allows individual classes to register for invocation during the initialization
    /// process.
    /// </summary>
    [Export(typeof(IInitializer))]
    internal class Initializer : IInitializer
    {
        private readonly IInvocableInitializer invocableInitializer;

        [ImportingConstructor]
        public Initializer(IInvocableInitializer invocableInitializer)
        {
            this.invocableInitializer = invocableInitializer;
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
            ArgumentValidator.EnsureArgumentNotNull(instance, nameof(instance));
            this.ValidateParameters(suppressEvents, message, data);
            this.invocableInitializer.Register(
                instance, suppressEvents, message, data);
        }

        private void ValidateParameters(
            bool suppressEvents,
            string message,
            IDictionary<string, object> data)
        {
            if(suppressEvents && (message != null || data != null))
            {
                throw new InvalidOperationException(
                    Resources.InvalidInitializeParameters);
            }
        }
    }
}
