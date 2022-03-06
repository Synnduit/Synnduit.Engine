using Synnduit.Events;

namespace Synnduit
{
    /// <summary>
    /// Represents a single value change performed during a merge.
    /// </summary>
    public class ValueChange : IValueChange
    {
        private readonly string valueName;

        private readonly object previousValue;

        private readonly object newValue;

        /// <summary>
        /// Initializes a new instance of the class and sets the values of its properties.
        /// </summary>
        /// <param name="valueName">The name of the value that was changed.</param>
        /// <param name="previousValue">The value before the change was applied.</param>
        /// <param name="newValue">The value after the change was applied.</param>
        public ValueChange(string valueName, object previousValue, object newValue)
        {
            this.valueName = valueName;
            this.previousValue = previousValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Gets the name of the value that was changed.
        /// </summary>
        public string ValueName
        {
            get { return this.valueName; }
        }

        /// <summary>
        /// Gets the value before the change was applied.
        /// </summary>
        public object PreviousValue
        {
            get { return this.previousValue; }
        }

        /// <summary>
        /// Gets the value after the change was applied.
        /// </summary>
        public object NewValue
        {
            get { return this.newValue; }
        }
    }
}
