﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algorist.EventSourcing.Exceptions;
using Algorist.EventSourcing.Util;

namespace Algorist.EventSourcing.Core.Domain
{
    public abstract class AggregateRoot : AggregateRoot<Guid>
    {
    }

    public abstract class AggregateRoot<TAggregateKey>
    {
        private readonly SemaphoreSlim _applyLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<Type, string> _eventHandlerCache;
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

        private readonly IList<IEvent<TAggregateKey>> _uncommittedChanges = new List<IEvent<TAggregateKey>>();

        protected AggregateRoot()
        {
            CurrentVersion = (int) StreamState.NoStream;
            LastCommittedVersion = (int) StreamState.NoStream;
            _eventHandlerCache = ReflectionHelper.FindEventHandlerMethodsInAggregate<TAggregateKey>(GetType());
        }

        public TAggregateKey Id { get; private set; }
        public int CurrentVersion { get; private set; }
        public int LastCommittedVersion { get; private set; }
        public StreamState StreamState => CurrentVersion == -1 ? StreamState.NoStream : StreamState.HasStream;

        public bool HasUncommittedChanges()
        {
            lock (_uncommittedChanges)
            {
                return _uncommittedChanges.Any();
            }
        }

        public ReadOnlyCollection<IEvent<TAggregateKey>> GetUncommittedChanges()
        {
            lock (_uncommittedChanges)
            {
                return new ReadOnlyCollection<IEvent<TAggregateKey>>(_uncommittedChanges);
            }
        }

        public void MarkChangesAsCommitted()
        {
            lock (_uncommittedChanges)
            {
                _uncommittedChanges.Clear();
                LastCommittedVersion = CurrentVersion;
            }
        }

        public async Task LoadsFromHistoryAsync(IEnumerable<IEvent<TAggregateKey>> history)
        {
            await _loadLock.WaitAsync();
            try
            {
                foreach (var e in history)
                    //We call ApplyEvent with isNew parameter set to false as we are replaying a historical event
                    await ApplyEventAsync(e, false);

                LastCommittedVersion = CurrentVersion;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        protected void ApplyEvent(IEvent<TAggregateKey> @event)
        {
            ApplyEventAsync(@event).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // This method is only used when applying new events
        protected async Task ApplyEventAsync(IEvent<TAggregateKey> @event)
        {
            await _loadLock.WaitAsync();
            try
            {
                await ApplyEventAsync(@event, true);
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private async Task ApplyEventAsync(IEvent<TAggregateKey> @event, bool isNew)
        {
            //All state changes to AggregateRoot must happen via the Apply method
            //Make sure the right Apply method is called with the right type.
            //We can you use dynamic objects or reflection for this.

            await _applyLock.WaitAsync();
            try
            {
                if (CanApply(@event))
                {
                    await DoApplyAsync(@event);

                    if (isNew)
                        lock (_uncommittedChanges)
                        {
                            _uncommittedChanges.Add(@event); //only add to the events collection if it's a new event
                        }
                }
                else
                {
                    throw new AggregateStateMismatchException(
                        $"The event target version is {@event.AggregateId}.(Version {@event.TargetVersion}) and " +
                        $"AggregateRoot version is {Id}.(Version {CurrentVersion})");
                }
            }
            finally
            {
                _applyLock.Release();
            }
        }

        private bool CanApply(IEvent<TAggregateKey> @event)
        {
            //Check to see if event is applying against the right Aggregate and matches the target version
            return (StreamState == StreamState.NoStream || Id.Equals(@event.AggregateId)) && CurrentVersion == @event.TargetVersion;
        }

        private async Task DoApplyAsync(IEvent<TAggregateKey> @event)
        {
            if (StreamState == StreamState.NoStream) Id = @event.AggregateId; //This is only needed for the very first event as every other event CAN ONLY apply to matching ID

            if (_eventHandlerCache.ContainsKey(@event.GetType()))
                await @event.InvokeOnAggregateAsync(this, _eventHandlerCache[@event.GetType()]);
            else
                throw new AggregateEventOnApplyMethodMissingException($"No event handler specified for {@event.GetType()} on {GetType()}");

            CurrentVersion++;
        }

        internal void HydrateFromSnapshot(ISnapshot<TAggregateKey> snapshot)
        {
            Id = snapshot.AggregateId;
            CurrentVersion = snapshot.Version;
            LastCommittedVersion = snapshot.Version;
        }
    }
}