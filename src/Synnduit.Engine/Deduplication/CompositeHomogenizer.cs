using System.ComponentModel.Composition;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// A homogenizer that applies a collection of homogenizer to the value being
    /// homogenized.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(ICompositeHomogenizer<>))]
    internal class CompositeHomogenizer<TEntity> : ICompositeHomogenizer<TEntity>
        where TEntity : class
    {
        private readonly IServiceProvider<TEntity> serviceProvider;

        [ImportingConstructor]
        public CompositeHomogenizer(IServiceProvider<TEntity> serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Applies the current collection of homogenizers to the specified value.
        /// </summary>
        /// <param name="value">The value to homogenize.</param>
        /// <returns>The homogenized value.</returns>
        public object Homogenize(object value)
        {
            object homogenizedValue = value;
            foreach(IHomogenizer homogenizer in this.serviceProvider.Homogenizers)
            {
                homogenizedValue = homogenizer.Homogenize(homogenizedValue);
            }
            return homogenizedValue;
        }
    }
}
