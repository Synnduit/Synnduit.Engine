using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Transactions;
using Synnduit.Properties;

namespace Synnduit.Deduplication
{
    /// <summary>
    /// Finds duplicates of source system entities in the destination system.
    /// </summary>
    /// <typeparam name="TEntity">The type representing the entity.</typeparam>
    [Export(typeof(IDeduplicator<>))]
    internal class Deduplicator<TEntity> : IDeduplicator<TEntity>
        where TEntity : class
    {
        private readonly IEnumerable<IDuplicationRule<TEntity>> rules;

        [ImportingConstructor]
        public Deduplicator(
            [ImportMany] IEnumerable<IDuplicationRule<TEntity>> rules)
        {
            this.rules = rules;
        }

        /// <summary>
        /// Deduplicates the specified source system entity.
        /// </summary>
        /// <param name="entity">The entity to deduplicate.</param>
        /// <returns>The result of the deduplication process.</returns>
        public DeduplicationResult Deduplicate(TEntity entity)
        {
            Dictionary<EntityIdentifier, MatchWeight>
                duplicates = this.ApplyRules(entity);
            DeduplicationResult result = this.GetResult(duplicates);
            return result;
        }

        private Dictionary<EntityIdentifier, MatchWeight> ApplyRules(TEntity entity)
        {
            var duplicates = new Dictionary<EntityIdentifier, MatchWeight>();
            foreach(IDuplicationRule<TEntity> rule in this.rules)
            {
                this.ApplyRule(entity, rule, duplicates);
            }
            return duplicates;
        }

        private void ApplyRule(
            TEntity entity,
            IDuplicationRule<TEntity> rule,
            Dictionary<EntityIdentifier, MatchWeight> duplicates)
        {
            IEnumerable<Duplicate> matches = this.GetDuplicates(entity, rule);
            foreach(Duplicate match in matches)
            {
                if(duplicates.ContainsKey(match.DestinationSystemEntityId) == false)
                {
                    duplicates.Add(match.DestinationSystemEntityId, match.Weight);
                }
                else if(duplicates[match.DestinationSystemEntityId] < match.Weight)
                {
                    duplicates[match.DestinationSystemEntityId] = match.Weight;
                }
            }
        }

        private IEnumerable<Duplicate> GetDuplicates(
            TEntity entity, IDuplicationRule<TEntity> rule)
        {
            IEnumerable<Duplicate> duplicates;
            try
            {
                using(var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    duplicates = rule.GetDuplicates(entity);
                }
            }
            catch(Exception exception)
            {
                throw new DestinationSystemException(exception);
            }
            this.ValidateDuplicates(duplicates, rule);
            return duplicates;
        }

        private void ValidateDuplicates(
            IEnumerable<Duplicate> duplicates, IDuplicationRule<TEntity> rule)
        {
            if(duplicates == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.DuplicationRuleReturnedNull,
                    rule.GetType().FullName));
            }
        }

        private DeduplicationResult GetResult(
            Dictionary<EntityIdentifier, MatchWeight> duplicates)
        {
            DeduplicationResult result;
            if(duplicates.Count == 0)
            {
                // no duplicates found => this is a new entity
                result = new DeduplicationResult(DeduplicationStatus.NewEntity);
            }
            else if(duplicates.Count(d => d.Value == MatchWeight.Inconsistent) >= 1)
            {
                // at least one duplicate inconsistent => refer for manual inspection
                result = new DeduplicationResult(
                    DeduplicationStatus.ManualInspectionRequired,
                    this.GetDuplicatesCollection(duplicates));
            }
            else if(duplicates.Count(d => d.Value == MatchWeight.Positive) == 1)
            {
                // a single positive duplicate found
                EntityIdentifier duplicateId =
                    duplicates.First(d => d.Value == MatchWeight.Positive).Key;
                result = new DeduplicationResult(
                    DeduplicationStatus.DuplicateFound, duplicateId);
            }
            else
            {
                // a number of candidate duplicates (or multiple positives) found
                // => refer for manual inspection
                result = new DeduplicationResult(
                    DeduplicationStatus.ManualInspectionRequired,
                    this.GetDuplicatesCollection(duplicates));
            }
            return result;
        }

        private IEnumerable<Duplicate> GetDuplicatesCollection(
            Dictionary<EntityIdentifier, MatchWeight> duplicates)
        {
            return duplicates.Select(d => new Duplicate(d.Key, d.Value));
        }
    }
}
