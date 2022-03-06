using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Synnduit.Properties;

namespace Synnduit
{
    /// <summary>
    /// Performs conversions between <see cref="EntityIdentifier" /> instances and
    /// compatible types (i.e., those types that implicit or explicit cast operators
    /// to/from EntityIdentifier exist for).
    /// </summary>
    [Export(typeof(IEntityIdentifierConverter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class EntityIdentifierConverter : IEntityIdentifierConverter
    {
        private const string ImplicitOperatorMethodName = "op_Implicit";

        private const string ExplicitOperatorMethodName = "op_Explicit";

        private IDictionary<Type, Func<object, EntityIdentifier>> fromValueConverters;

        private IDictionary<Type, Func<EntityIdentifier, object>> toValueConverters;

        [ImportingConstructor]
        public EntityIdentifierConverter()
        {
            this.fromValueConverters = this.CreateFromValueConverters();
            this.toValueConverters = this.CreateToValueConverters();
        }

        /// <summary>
        /// Creates an <see cref="EntityIdentifier" /> instance by converting the specified
        /// <see cref="object" /> instance.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// A <see cref="EntityIdentifier" /> instance created by converting the specified
        /// <see cref="object" /> instance.
        /// </returns>
        public EntityIdentifier FromValue(object value)
        {
            EntityIdentifier identifier = null;
            if(value != null)
            {
                Func<object, EntityIdentifier> valueConverter =
                    this.GetFromValueConverter(value.GetType());
                if(valueConverter == null)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.ConversionFromNotSupported,
                        value.GetType().FullName));
                }
                identifier = valueConverter(value);
            }
            return identifier;
        }

        /// <summary>
        /// Converts the specified <see cref="EntityIdentifier" /> instance to an instance
        /// of the specified type.
        /// </summary>
        /// <param name="identifier">
        /// The <see cref="EntityIdentifier" /> instance to convert.
        /// </param>
        /// <param name="type">The target type.</param>
        /// <returns>
        /// The specified <see cref="EntityIdentifier" /> converted to the specified type.
        /// </returns>
        public object ToValue(EntityIdentifier identifier, Type type)
        {
            // validate the type parameter
            ArgumentValidator.EnsureArgumentNotNull(type, nameof(type));

            // convert the identifier and return the value
            object value = null;
            if(identifier != null)
            {
                Func<EntityIdentifier, object>
                    valueConverter = this.GetToValueConverter(type);
                if(valueConverter == null)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.ConversionToNotSupported,
                        type.FullName));
                }
                value = valueConverter(identifier);
            }
            return value;
        }

        private Func<object, EntityIdentifier> GetFromValueConverter(Type type)
        {
            return this.GetValueConverter(
                type,
                this.fromValueConverters,
                this.AddTypeCastOperatorFromValueConverters);
        }

        private Func<EntityIdentifier, object> GetToValueConverter(Type type)
        {
            return this.GetValueConverter(
                type,
                this.toValueConverters,
                this.AddTypeCastOperatorToValueConverters);
        }

        private TValueConverter GetValueConverter<TValueConverter>(
            Type type,
            IDictionary<Type, TValueConverter> valueConverters,
            Action<IDictionary<Type, TValueConverter>, Type> addValueConverters)
        {
            if(!valueConverters.ContainsKey(type))
            {
                addValueConverters(valueConverters, type);
            }
            valueConverters.TryGetValue(type, out TValueConverter valueConverter);
            return valueConverter;
        }

        private IDictionary<
            Type, Func<object, EntityIdentifier>> CreateFromValueConverters()
        {
            var fromValueConverters =
                new Dictionary<Type, Func<object, EntityIdentifier>>()
                {
                    {
                        typeof(EntityIdentifier),
                        this.IdentityFromValueConverter
                    }
                };
            this.AddTypeCastOperatorFromValueConverters(
                fromValueConverters,
                typeof(EntityIdentifier));
            return fromValueConverters;
        }

        private IDictionary<
            Type, Func<EntityIdentifier, object>> CreateToValueConverters()
        {
            var toValueConverters =
                new Dictionary<Type, Func<EntityIdentifier, object>>()
                {
                    {
                        typeof(EntityIdentifier),
                        this.IdentityToValueConverter
                    }
                };
            this.AddTypeCastOperatorToValueConverters(
                toValueConverters,
                typeof(EntityIdentifier));
            return toValueConverters;
        }

        private void AddTypeCastOperatorFromValueConverters(
            IDictionary<Type, Func<object, EntityIdentifier>> fromValueConverters,
            Type type)
        {
            foreach(MethodInfo castOperator in
                this
                .GetCastOperators(type)
                .Where(m => m.ReturnType == typeof(EntityIdentifier)))
            {
                fromValueConverters.Add(
                    castOperator.GetParameters().Single().ParameterType,
                    this.CreateFromValueConverter(castOperator));
            }
        }

        private void AddTypeCastOperatorToValueConverters(
            IDictionary<Type, Func<EntityIdentifier, object>> toValueConverters,
            Type type)
        {
            foreach(MethodInfo castOperator in
                this
                .GetCastOperators(type)
                .Where(m =>
                    m.GetParameters().Single().ParameterType == typeof(EntityIdentifier)))
            {
                Func<EntityIdentifier, object> toValueConverter =
                    this.CreateToValueConverter(castOperator);
                toValueConverters.Add(castOperator.ReturnType, toValueConverter);
                if(castOperator.ReturnType.IsValueType)
                {
                    toValueConverters.Add(
                        typeof(Nullable<>).MakeGenericType(castOperator.ReturnType),
                        toValueConverter);
                }
            }
        }

        private IEnumerable<MethodInfo> GetCastOperators(Type type)
        {
            return
                type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m =>
                    m.Name == ImplicitOperatorMethodName ||
                    m.Name == ExplicitOperatorMethodName);
        }

        private Func<object, EntityIdentifier>
            CreateFromValueConverter(MethodInfo castOperator)
        {
            return value => (EntityIdentifier) this.Invoke(castOperator, value);
        }

        private Func<EntityIdentifier, object>
            CreateToValueConverter(MethodInfo castOperator)
        {
            return identifier => this.Invoke(castOperator, identifier);
        }

        private EntityIdentifier IdentityFromValueConverter(object value)
        {
            return (EntityIdentifier) value;
        }

        private object IdentityToValueConverter(EntityIdentifier identifier)
        {
            return identifier;
        }

        private object Invoke(MethodInfo castOperator, object parameter)
        {
            try
            {
                return castOperator.Invoke(null, new[] { parameter });
            }
            catch(TargetInvocationException exception)
            {
                throw new InvalidOperationException(
                    Resources.CastOperatorThrewException,
                    exception.InnerException);
            }
        }
    }
}
