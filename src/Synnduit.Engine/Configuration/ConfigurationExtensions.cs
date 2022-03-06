using Synnduit.Properties;

namespace Synnduit.Configuration
{
    /// <summary>
    /// Contains configuration-related extension methods.
    /// </summary>
    internal static class ConfigurationExtensions
    {
        /// <summary>
        /// Converts the specified collection to an array; if the collection is null, an empty
        /// array is returned; all null elements of the collection are excluded from the result.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="collection">The collection to convert.</param>
        /// <returns>The converted collection (array).</returns>
        public static T[] SafeToArray<T>(this IEnumerable<T> collection)
            where T : class =>
            (collection ?? Enumerable.Empty<T>())
            .Where(x => x != null)
            .ToArray();

        /// <summary>
        /// Parses the <see cref="IMigrationLoggingConfiguration.ExcludedOutcomes"/> value and
        /// returns it as a collection of <see cref="EntityTransactionOutcome"/> values.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IMigrationLoggingConfiguration"/> instance to retrieve the
        /// <see cref="IMigrationLoggingConfiguration.ExcludedOutcomes"/> value from.
        /// </param>
        /// <returns>
        /// The parsed collection of <see cref="EntityTransactionOutcome"/> values.
        /// </returns>
        public static IEnumerable<EntityTransactionOutcome> GetExcludedOutcomes(
            this IMigrationLoggingConfiguration configuration) =>
            ParseEnumValues<EntityTransactionOutcome>(configuration.ExcludedOutcomes);

        /// <summary>
        /// Parses the <see cref="IGarbageCollectionLoggingConfiguration.ExcludedOutcomes"/> value
        /// and returns it as a collection of <see cref="EntityDeletionOutcome"/> values.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IGarbageCollectionLoggingConfiguration"/> instance to retrieve the
        /// <see cref="IGarbageCollectionLoggingConfiguration.ExcludedOutcomes"/> value from.
        /// </param>
        /// <returns>
        /// The parsed collection of <see cref="EntityDeletionOutcome"/> values.
        /// </returns>
        public static IEnumerable<EntityDeletionOutcome> GetExcludedOutcomes(
            this IGarbageCollectionLoggingConfiguration configuration) =>
            ParseEnumValues<EntityDeletionOutcome>(configuration.ExcludedOutcomes);

        private static IEnumerable<TEnum> ParseEnumValues<TEnum>(string value)
        {
            var enumValues = Array.Empty<TEnum>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                enumValues =
                    value
                    .Split(',')
                    .Select(ParseEnumValue<TEnum>)
                    .ToArray();
            }
            return enumValues;
        }

        private static TEnum ParseEnumValue<TEnum>(string value)
        {
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), value.Trim());
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.InvalidEnumValue, value, typeof(TEnum).Name),
                    exception);
            }
        }
    }
}
