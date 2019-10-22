using System;
using System.Linq;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Exceptions;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Repository
{
    public class Repository<TAggregate, TSnapshot> : Repository<TAggregate, TSnapshot, Guid>, IRepository<TAggregate>
        where TAggregate : AggregateRoot<Guid>, new()
        where TSnapshot : ISnapshot<Guid>
    {
        public Repository(IClock clock,
            IEventStorageProvider<TAggregate, Guid> eventStorageProvider,
            IEventPublisher eventPublisher,
            ISnapshotStorageProvider<TSnapshot, Guid> snapshotStorageProvider) :
            base(clock, eventStorageProvider, eventPublisher, snapshotStorageProvider)
        {
        }
    }

    public class Repository<TAggregate, TSnapshot, TAggregateKey> : IRepository<TAggregate, TAggregateKey>
        where TAggregate : AggregateRoot<TAggregateKey>, new()
        where TSnapshot : ISnapshot<TAggregateKey>
    {
        private readonly IEventStorageProvider<TAggregate, TAggregateKey> _eventStorageProvider;
        private readonly ISnapshotStorageProvider<TSnapshot, TAggregateKey> _snapshotStorageProvider;
        private readonly IEventPublisher<TAggregateKey> _eventPublisher;
        private readonly IClock _clock;

        public Repository(IClock clock,
            IEventStorageProvider<TAggregate, TAggregateKey> eventStorageProvider,
            IEventPublisher<TAggregateKey> eventPublisher,
            ISnapshotStorageProvider<TSnapshot, TAggregateKey> snapshotStorageProvider)
        {
            _eventStorageProvider = eventStorageProvider;
            _snapshotStorageProvider = snapshotStorageProvider;
            _eventPublisher = eventPublisher;
            _clock = clock;
        }

        public async Task<TAggregate> GetByIdAsync(TAggregateKey id)
        {
            var item = default(TAggregate);
            var isSnapshottable =
                typeof(ISnapshottable<TSnapshot, TAggregateKey>).IsAssignableFrom(typeof(TAggregate));

            var snapshot = default(TSnapshot);

            if ((isSnapshottable) && (_snapshotStorageProvider != null))
            {
                snapshot = await _snapshotStorageProvider.GetSnapshotAsync(id);
            }

            if (snapshot != null)
            {
                item = CreateNewInstance();

                if (!(item is ISnapshottable<TSnapshot, TAggregateKey> snapshottableItem))
                {
                    throw new NullReferenceException(nameof(snapshottableItem));
                }

                item.HydrateFromSnapshot(snapshot);
                snapshottableItem.ApplySnapshot(snapshot);

                var events = await _eventStorageProvider.GetEventsAsync(id, snapshot.Version + 1, int.MaxValue);
                await item.LoadsFromHistoryAsync(events);
            }
            else
            {
                var events = (await _eventStorageProvider.GetEventsAsync(id, 0, int.MaxValue)).ToList();

                if (events.Any())
                {
                    item = CreateNewInstance();
                    await item.LoadsFromHistoryAsync(events);
                }
            }

            return item;
        }

        public async Task SaveAsync(TAggregate aggregate)
        {
            if (aggregate.HasUncommittedChanges())
            {
                await CommitChanges(aggregate);
            }
        }

        private async Task CommitChanges(AggregateRoot<TAggregateKey> aggregate)
        {
            var expectedVersion = aggregate.LastCommittedVersion;

            var item = await _eventStorageProvider.GetLastEventAsync(aggregate.Id);

            if ((item != null) && (expectedVersion == (int) StreamState.NoStream))
            {
                throw new AggregateCreationException(
                    $"Aggregate {item.CorrelationId} can't be created as it already exists with version {item.TargetVersion + 1}");
            }
            else if ((item != null) && ((item.TargetVersion + 1) != expectedVersion))
            {
                throw new ConcurrencyException(
                    $"Aggregate {item.CorrelationId} has been modified externally and has an updated state. Can't commit changes.");
            }

            var changesToCommit = aggregate
                .GetUncommittedChanges()
                .ToList();

            //perform pre commit actions
            foreach (var e in changesToCommit)
            {
                DoPreCommitTasks(e);
            }

            //CommitAsync events to storage provider
            await _eventStorageProvider.SaveAsync(aggregate);

            //Publish to event publisher asynchronously
            if (_eventPublisher != null)
            {
                foreach (var e in changesToCommit)
                {
                    await _eventPublisher.PublishAsync(e);
                }
            }

            //If the Aggregate implements snapshottable
            if ((aggregate is ISnapshottable<TSnapshot, TAggregateKey> snapshottable) &&
                (_snapshotStorageProvider != null))
            {
                //Every N events we save a snapshot
                if ((aggregate.CurrentVersion >= _snapshotStorageProvider.SnapshotFrequency) &&
                    (
                        (changesToCommit.Count >= _snapshotStorageProvider.SnapshotFrequency) ||
                        (aggregate.CurrentVersion % _snapshotStorageProvider.SnapshotFrequency <
                         changesToCommit.Count) ||
                        (aggregate.CurrentVersion % _snapshotStorageProvider.SnapshotFrequency == 0)
                    )
                )
                {
                    var snapshot = snapshottable.TakeSnapshot();
                    await _snapshotStorageProvider.SaveSnapshotAsync(snapshot);
                }
            }

            aggregate.MarkChangesAsCommitted();
        }

        private TAggregate CreateNewInstance()
        {
            return new TAggregate();
        }

        private void DoPreCommitTasks(IEvent<TAggregateKey> e)
        {
            e.EventCommittedTimestamp = _clock.Now();
        }
    }
}