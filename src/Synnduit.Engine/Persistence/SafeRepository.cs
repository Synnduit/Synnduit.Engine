using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Synnduit.Mappings;
using Synnduit.Properties;

namespace Synnduit.Persistence
{
    /// <summary>
    /// A wrapper around the <see cref="IRepository" /> implementation; catches (and
    /// rethrows) exceptions, ensures return values aren't null references, etc.
    /// </summary>
    [Export(typeof(ISafeRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class SafeRepository : ISafeRepository
    {
        private static string[] ValidParameterMasks = { "100", "010", "011", "001" };

        private readonly IRepository repository;

        [ImportingConstructor]
        public SafeRepository(IRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Disposes the underlying <see cref="IRepository" /> instance.
        /// </summary>
        public void Dispose()
        {
            this.Invoke(
                () => this.repository.Dispose(),
                nameof(this.repository.Dispose));
        }

        /// <summary>
        /// Clears all shared source system identifier (group) records for the specified
        /// entity type.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        public void ClearSharedSourceSystemIdentifiers(Guid entityTypeId)
        {
            this.Invoke(
                () => this.repository.ClearSharedSourceSystemIdentifiers(entityTypeId),
                nameof(this.repository.ClearSharedSourceSystemIdentifiers));
        }

        /// <summary>
        /// Creates a new entity deletion.
        /// </summary>
        /// <param name="transaction">The transaction to create.</param>
        public void CreateEntityDeletion(IEntityDeletion transaction)
        {
            this.Invoke(
                () => this.repository.CreateEntityDeletion(transaction),
                nameof(this.repository.CreateEntityDeletion));
        }

        /// <summary>
        /// Creates a new message associated with the specified source system entity
        /// identity (always) and operation (if it exists).
        /// </summary>
        /// <param name="message">The message to create.</param>
        public void CreateIdentityOperationMessage(IIdentityOperationMessage message)
        {
            this.Invoke(
                () => this.repository.CreateIdentityOperationMessage(message),
                nameof(this.repository.CreateIdentityOperationMessage));
        }

        /// <summary>
        /// Creates a new identity entity transaction.
        /// </summary>
        /// <param name="transaction">The transaction to create.</param>
        public void CreateIdentityEntityTransaction(IIdentityEntityTransaction transaction)
        {
            this.Invoke(
                () => this.repository.CreateIdentityEntityTransaction(transaction),
                nameof(this.repository.CreateIdentityEntityTransaction));
        }

        /// <summary>
        /// Creates a new serialized source system entity associated with the specified
        /// source system entity identity (always) and operation (if it exists).
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        public void CreateIdentityOperationSourceSystemEntity(
            IIdentityOperationSourceSystemEntity entity)
        {
            this.Invoke(
                () => this.repository.CreateIdentityOperationSourceSystemEntity(entity),
                nameof(this.repository.CreateIdentityOperationSourceSystemEntity));
        }

        /// <summary>
        /// Creates a new entity source/destination system identifier mapping.
        /// </summary>
        /// <param name="mapping">The mapping to create.</param>
        public void CreateMapping(IMapping mapping)
        {
            this.Invoke(
                () => this.repository.CreateMapping(mapping),
                nameof(this.repository.CreateMapping));
        }

        /// <summary>
        /// Creates a new mapping entity transaction.
        /// </summary>
        /// <param name="transaction">The transaction to create.</param>
        public void CreateMappingEntityTransaction(IMappingEntityTransaction transaction)
        {
            this.Invoke(
                () => this.repository.CreateMappingEntityTransaction(transaction),
                nameof(this.repository.CreateMappingEntityTransaction));
        }

        /// <summary>
        /// Creates a new operation message.
        /// </summary>
        /// <param name="message">The message to create.</param>
        public void CreateOperationMessage(IOperationMessage message)
        {
            this.Invoke(
                () => this.repository.CreateOperationMessage(message),
                nameof(this.repository.CreateOperationMessage));
        }

        /// <summary>
        /// Creates or updates the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type to create or update.</param>
        public void CreateOrUpdateEntityType(IEntityType entityType)
        {
            this.Invoke(
                () => this.repository.CreateOrUpdateEntityType(entityType),
                nameof(this.repository.CreateOrUpdateEntityType));
        }

        /// <summary>
        /// Creates or updates the specified external system.
        /// </summary>
        /// <param name="externalSystem">The external system to create or update.</param>
        public void CreateOrUpdateExternalSystem(IExternalSystem externalSystem)
        {
            this.Invoke(
                () => this.repository.CreateOrUpdateExternalSystem(externalSystem),
                nameof(this.repository.CreateOrUpdateExternalSystem));
        }

        /// <summary>
        /// Creates or updates the specified feed.
        /// </summary>
        /// <param name="feed">The feed to create or update.</param>
        public void CreateOrUpdateFeed(IFeed feed)
        {
            this.Invoke(
                () => this.repository.CreateOrUpdateFeed(feed),
                nameof(this.repository.CreateOrUpdateFeed));
        }

        /// <summary>
        /// Creates the specified application parameter.
        /// </summary>
        /// <param name="parameter">The parameter to create.</param>
        public void CreateParameter(IParameter parameter)
        {
            this.Invoke(
                () => this.repository.CreateParameter(parameter),
                nameof(this.repository.CreateParameter));
        }

        /// <summary>
        /// Creates a new shared source system identifier record.
        /// </summary>
        /// <param name="sharedSourceSystemIdentifier">
        /// The shared source system identifier record to create.
        /// </param>
        public void CreateSharedSourceSystemIdentifier(
            ISharedSourceSystemIdentifier sharedSourceSystemIdentifier)
        {
            this.Invoke(
                () =>
                    this
                    .repository
                    .CreateSharedSourceSystemIdentifier(sharedSourceSystemIdentifier),
                nameof(this.repository.CreateSharedSourceSystemIdentifier));
        }

        /// <summary>
        /// Creates a new value change record.
        /// </summary>
        /// <param name="valueChange">The value change record to create.</param>
        public void CreateValueChange(IValueChange valueChange)
        {
            this.Invoke(
                () => this.repository.CreateValueChange(valueChange),
                nameof(this.repository.CreateValueChange));
        }

        /// <summary>
        /// Deletes the specified application parameter.
        /// </summary>
        /// <param name="id">The ID of the parameter.</param>
        public void DeleteParameter(Guid id)
        {
            this.Invoke(
                () => this.repository.DeleteParameter(id),
                nameof(this.repository.DeleteParameter));
        }

        /// <summary>
        /// Gets the dictionary of application parameters that apply to the specified
        /// destination system.
        /// </summary>
        /// <param name="destinationSystemId">
        /// The ID of the destination (external) system.
        /// </param>
        /// <returns>
        /// The dictionary (name/value) of application parameters that apply to the
        /// specified destination system.
        /// </returns>
        public IDictionary<string, string>
            GetDestinationSystemParameters(Guid destinationSystemId)
        {
            return this.GetParameters(
                () => this.repository.GetDestinationSystemParameters(destinationSystemId),
                nameof(this.repository.GetDestinationSystemParameters));
        }

        /// <summary>
        /// Gets all entity mappings for the specified destination system that are in one
        /// of the specified states.
        /// </summary>
        /// <param name="destinationSystemId">
        /// The ID of the destination (external) system.
        /// </param>
        /// <param name="states">
        /// The collection of requested mapping states (codes).
        /// </param>
        /// <returns>The collection of entity mappings.</returns>
        public IEnumerable<IEntityMapping> GetEntityMappings(
            Guid destinationSystemId, params int[] states)
        {
            return this.Invoke(
                () => this.repository.GetEntityMappings(destinationSystemId, states),
                this.GetValidateEntityMappingMethod(states),
                nameof(this.repository.GetEntityMappings));
        }

        /// <summary>
        /// Gets the specified entity type.
        /// </summary>
        /// <param name="id">The ID of the requested entity type.</param>
        /// <returns>The requested entity type.</returns>
        public IEntityType GetEntityType(Guid id)
        {
            IEntityType entityType = this.Invoke(
                () => this.repository.GetEntityType(id),
                nameof(this.repository.GetEntityType));
            this.GetValidateEntityTypeMethod(id)(
                entityType, nameof(this.repository.GetEntityType));
            return entityType;
        }

        /// <summary>
        /// Gets the dictionary of application parameters that apply to the specified
        /// entity type.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <returns>
        /// The dictionary of application parameters that apply to the specified entity
        /// type.
        /// </returns>
        public IDictionary<string, string> GetEntityTypeParameters(Guid entityTypeId)
        {
            return this.GetParameters(
                () => this.repository.GetEntityTypeParameters(entityTypeId),
                nameof(this.repository.GetEntityTypeParameters));
        }

        /// <summary>
        /// Gets the collection of all entity types.
        /// </summary>
        /// <returns>The collection of all entity types.</returns>
        public IEnumerable<IEntityType> GetEntityTypes()
        {
            return this.Invoke(
                () => this.repository.GetEntityTypes(),
                this.GetValidateEntityTypeMethod(),
                nameof(this.repository.GetEntityTypes));
        }

        /// <summary>
        /// Gets the dictionary of application parameters that apply to the specified
        /// entity type and source system.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <returns>
        /// The dictionary of application parameters that apply to the specified entity
        /// type and source system.
        /// </returns>
        public IDictionary<string, string> GetEntityTypeSourceSystemParameters(
            Guid entityTypeId, Guid sourceSystemId)
        {
            return this.GetParameters(
                () => this.repository.GetEntityTypeSourceSystemParameters(
                    entityTypeId, sourceSystemId),
                nameof(this.repository.GetEntityTypeSourceSystemParameters));
        }

        /// <summary>
        /// Gets the collection of all external systems.
        /// </summary>
        /// <returns>The collection of all external systems.</returns>
        public IEnumerable<IExternalSystem> GetExternalSystems()
        {
            return this.Invoke<IEnumerable<IExternalSystem>, IExternalSystem>(
                () => this.repository.GetExternalSystems(),
                this.ValidateExternalSystem,
                nameof(this.repository.GetExternalSystems));
        }

        /// <summary>
        /// Gets the full name of the type that represents the feed for the specified
        /// entity type and source system.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <returns>
        /// The full name of the type that represents the feed for the specified entity
        /// type and source system; if no feed is registered for the specified combination,
        /// a null reference will be returned.
        /// </returns>
        public string GetFeedTypeFullName(Guid entityTypeId, Guid sourceSystemId)
        {
            return this.Invoke(
                () => this.repository.GetFeedTypeFullName(entityTypeId, sourceSystemId),
                nameof(this.repository.GetFeedTypeFullName),
                true);
        }

        /// <summary>
        /// Creates a new serialized destination system entity associated with an
        /// operation.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        public void CreateOperationDestinationSystemEntity(
            IOperationSerializedEntity entity)
        {
            this.Invoke(
                () =>
                    this
                    .repository
                    .CreateOperationDestinationSystemEntity(entity),
                nameof(this.repository.CreateOperationDestinationSystemEntity));
        }

        /// <summary>
        /// Gets the collection of identifiers that uniquely identify entities of the
        /// specified type in the destination system along with associated mapping
        /// information.
        /// </summary>
        /// <param name="entityTypeId">The ID of the entity type.</param>
        /// <returns>The collection of destination system identifiers.</returns>
        public IEnumerable<IMappedEntityIdentifier>
            GetMappedEntityIdentifiers(Guid entityTypeId)
        {
            return this.Invoke<
                IEnumerable<IMappedEntityIdentifier>, IMappedEntityIdentifier>(
                () => this.repository.GetMappedEntityIdentifiers(entityTypeId),
                this.ValidateMappedEntityIdentifier,
                nameof(this.repository.GetMappedEntityIdentifiers));
        }

        /// <summary>
        /// Creates a new serialized source system entity associated with an operation.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        public void CreateOperationSourceSystemEntity(
            IOperationSerializedEntity entity)
        {
            this.Invoke(
                () => this.repository.CreateOperationSourceSystemEntity(entity),
                nameof(this.repository.CreateOperationSourceSystemEntity));
        }

        /// <summary>
        /// Gets the serialized entity saved for the specified mapping.
        /// </summary>
        /// <param name="mappingId">The ID of the mapping.</param>
        /// <returns>The serialized entity saved for the specified mapping.</returns>
        public byte[] GetMappingEntity(Guid mappingId)
        {
            return this.Invoke(
                () => this.repository.GetMappingEntity(mappingId),
                nameof(this.repository.GetMappingEntity));
        }

        /// <summary>
        /// Gets the collection of all application parameters.
        /// </summary>
        /// <returns>The collection of all application parameters.</returns>
        public IEnumerable<IParameter> GetParameters()
        {
            return this.Invoke<IEnumerable<IParameter>, IParameter>(
                () => this.repository.GetParameters(),
                this.ValidateParameter,
                nameof(this.repository.GetParameters));
        }

        /// <summary>
        /// Gets the collection of records specifying which source (external) systems share
        /// identifiers for individual entity type.
        /// </summary>
        /// <returns>
        /// The collection of records specifying which source (external) systems share
        /// identifiers for individual entity type.
        /// </returns>
        public IEnumerable<ISharedIdentifierSourceSystem>
            GetSharedIdentifierSourceSystems()
        {
            return this.Invoke<
                IEnumerable<ISharedIdentifierSourceSystem>, ISharedIdentifierSourceSystem>(
                () => this.repository.GetSharedIdentifierSourceSystems(),
                this.ValidateSharedIdentifierSourceSystem,
                nameof(this.repository.GetSharedIdentifierSourceSystems));
        }

        /// <summary>
        /// Gets the dictionary of application parameters that apply to the specified
        /// source system.
        /// </summary>
        /// <param name="sourceSystemId">The ID of the source (external) system.</param>
        /// <returns>
        /// The dictionary of application parameters that apply to the specified source
        /// system.
        /// </returns>
        public IDictionary<string, string> GetSourceSystemParameters(Guid sourceSystemId)
        {
            return this.GetParameters(
                () => this.repository.GetSourceSystemParameters(sourceSystemId),
                nameof(this.repository.GetSourceSystemParameters));
        }

        /// <summary>
        /// Sets the state of the specified mapping.
        /// </summary>
        /// <param name="mappingId">The ID of the mapping.</param>
        /// <param name="operation">The current operation.</param>
        /// <param name="state">The new state (code) of the mapping.</param>
        public void SetMappingState(Guid mappingId, IOperation operation, int state)
        {
            this.Invoke(
                () => this.repository.SetMappingState(mappingId, operation, state),
                nameof(this.repository.SetMappingState));
        }

        /// <summary>
        /// Sets the value of the specified application parameter.
        /// </summary>
        /// <param name="id">The ID of the parameter.</param>
        /// <param name="value">The new parameter value.</param>
        public void SetParameterValue(Guid id, string value)
        {
            this.Invoke(
                () => this.repository.SetParameterValue(id, value),
                nameof(this.repository.SetParameterValue));
        }

        /// <summary>
        /// Updates the last access correlation ID of the specified source system entity
        /// identity.
        /// </summary>
        /// <param name="operationIdIdentity">
        /// The current operation ID/source system entity identity combination.
        /// </param>
        public void UpdateIdentityCorrelationId(IOperationIdIdentity operationIdIdentity)
        {
            this.Invoke(
                () => this.repository.UpdateIdentityCorrelationId(operationIdIdentity),
                nameof(this.repository.UpdateIdentityCorrelationId));
        }

        /// <summary>
        /// Updates the serialized source system entity for the specified mapping.
        /// </summary>
        /// <param name="mappingId">The ID of the mapping.</param>
        /// <param name="operation">The current operation.</param>       
        /// <param name="sourceSystemEntity">
        /// The new serialized source system entity.
        /// </param>
        public void UpdateMappingEntity(
            Guid mappingId,
            IOperation operation,
            ISerializedEntity sourceSystemEntity)
        {
            this.Invoke(
                () => this.repository.UpdateMappingEntity(
                    mappingId, operation, sourceSystemEntity),
                nameof(this.repository.UpdateMappingEntity));
        }

        private IDictionary<string, string> GetParameters(
            Func<IDictionary<string, string>> method,
            string methodName)
        {
            return this.Invoke<IDictionary<string, string>, KeyValuePair<string, string>>(
                method,
                this.ValidateParameter,
                methodName);
        }

        private void Invoke(Action method, string methodName)
        {
            try
            {
                method();
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.RepositoryMethodThrewException, methodName),
                    exception);
            }
        }

        private T Invoke<T, TElement>(
            Func<T> method,
            Action<TElement, string> validateElement,
            string methodName)
            where T : IEnumerable<TElement>
        {
            T elements = this.Invoke(method, methodName);
            foreach(TElement element in elements)
            {
                if(element == null)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.RepositoryMethodReturnedNullReferenceElement,
                        methodName));
                }
                validateElement(element, methodName);
            }
            return elements;
        }

        private T Invoke<T>(
            Func<T> method,
            string methodName,
            bool allowNullReturnValue = false)
        {
            T result;

            // get the result
            try
            {
                result = method();
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.RepositoryMethodThrewException, methodName),
                    exception);
            }

            // verify the result isn't a null reference (unless null references are valid
            // return values) and return it
            if(!allowNullReturnValue && result == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedNullReference, methodName));
            }
            return result;
        }

        private void ValidateParameter(
            KeyValuePair<string, string> parameter, string methodName)
        {
            if(string.IsNullOrWhiteSpace(parameter.Key))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedEmptyParameterName, methodName));
            }
            if(parameter.Value == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedNullParameterValue, methodName));
            }
        }

        private Action<IEntityMapping, string>
            GetValidateEntityMappingMethod(IEnumerable<int> states)
        {
            var validator = new EntityMappingValidator(this, states);
            return validator.ValidateEntityMapping;
        }

        private Action<IEntityType, string> GetValidateEntityTypeMethod(Guid? id = null)
        {
            var validator = new EntityTypeValidator(id);
            return validator.ValidateEntityType;
        }

        private void ValidateExternalSystem(
            IExternalSystem externalSystem, string methodName)
        {
            if(externalSystem.Id == Guid.Empty ||
                string.IsNullOrWhiteSpace(externalSystem.Name))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedInvalidExternalSystem,
                    methodName));
            }
        }

        private void ValidateMappedEntityIdentifier(
            IMappedEntityIdentifier mappedEntityIdentifier, string methodName)
        {
            if(string.IsNullOrWhiteSpace(
                mappedEntityIdentifier.DestinationSystemEntityId) ||
                !this.IsValidOrigin(mappedEntityIdentifier.Origin) ||
                !this.IsValidState(mappedEntityIdentifier.State))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedInvalidMappedEntityIdentifier,
                    methodName));
            }
        }

        private void ValidateParameter(IParameter parameter, string methodName)
        {
            if(parameter.Id == Guid.Empty ||
                !this.IsValidNullableGuid(parameter.DestinationSystemId) ||
                !this.IsValidNullableGuid(parameter.EntityTypeId) ||
                !this.IsValidNullableGuid(parameter.SourceSystemId) ||
                !this.IsValidParameterMask(parameter) ||
                string.IsNullOrWhiteSpace(parameter.Name) ||
                string.IsNullOrWhiteSpace(parameter.Value))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedInvalidParameter,
                    methodName));
            }
        }

        private void ValidateSharedIdentifierSourceSystem(
            ISharedIdentifierSourceSystem sharedIdentifierSourceSystem,
            string methodName)
        {
            if(sharedIdentifierSourceSystem.SourceSystemId == Guid.Empty ||
                sharedIdentifierSourceSystem.EntityTypeId == Guid.Empty ||
                sharedIdentifierSourceSystem.SharedIdentifierSourceSystemId == Guid.Empty)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.RepositoryMethodReturnedInvalidSharedIdentifierSourceSystem,
                    methodName));
            }
        }

        private bool IsValidOrigin(int origin)
        {
            return Enum.IsDefined(typeof(MappingOrigin), (MappingOrigin) origin);
        }

        private bool IsValidState(int state)
        {
            return Enum.IsDefined(typeof(MappingState), (MappingState) state);
        }

        private bool IsValidNullableGuid(Guid? value)
        {
            bool isValid = true;
            if(value.HasValue && value.Value == Guid.Empty)
            {
                isValid = false;
            }
            return isValid;
        }

        private bool IsValidParameterMask(IParameter parameter)
        {
            string mask =
                $"{this.Bool2Bit(parameter.DestinationSystemId.HasValue)}" +
                $"{this.Bool2Bit(parameter.EntityTypeId.HasValue)}" +
                $"{this.Bool2Bit(parameter.SourceSystemId.HasValue)}";
            return ValidParameterMasks.Contains(mask);
        }

        private string Bool2Bit(bool value)
        {
            return value ? "1" : "0";
        }

        private KeyValuePair<string, string>
            CopyKeyValuePair(KeyValuePair<string, string> value)
        {
            return value;
        }

        private class EntityMappingValidator
        {
            private readonly SafeRepository parent;

            private readonly IEnumerable<int> states;

            public EntityMappingValidator(SafeRepository parent, IEnumerable<int> states)
            {
                this.parent = parent;
                this.states = states;
            }

            public void ValidateEntityMapping(
                IEntityMapping entityMapping, string methodName)
            {
                if(entityMapping.Id == Guid.Empty ||
                    entityMapping.EntityTypeId == Guid.Empty ||
                    entityMapping.SourceSystemId == Guid.Empty ||
                    entityMapping.SourceSystemEntityId == null ||
                    entityMapping.DestinationSystemEntityId == null ||
                    !this.parent.IsValidOrigin(entityMapping.Origin) ||
                    !this.IsValidState(entityMapping.State) ||
                    string.IsNullOrWhiteSpace(entityMapping.SerializedEntityHash))
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.RepositoryMethodReturnedInvalidEntityMapping,
                        methodName));
                }
            }

            private bool IsValidState(int state)
            {
                return this.states.Contains(state);
            }
        }

        private class EntityTypeValidator
        {
            private readonly Guid? id;

            public EntityTypeValidator(Guid? id)
            {
                this.id = id;
            }

            public void ValidateEntityType(IEntityType entityType, string methodName)
            {
                if(!this.IsIdValid(entityType.Id) ||
                    string.IsNullOrWhiteSpace(entityType.Name) ||
                    string.IsNullOrWhiteSpace(entityType.TypeName) ||
                    string.IsNullOrWhiteSpace(entityType.SinkTypeFullName))
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.RepositoryMethodReturnedInvalidEntityType,
                        methodName));
                }
            }

            private bool IsIdValid(Guid id)
            {
                bool isValid = true;
                if(id == Guid.Empty)
                {
                    isValid = false;
                }
                else if(this.id.HasValue && id != this.id.Value)
                {
                    isValid = false;
                }
                return isValid;
            }
        }
    }
}
