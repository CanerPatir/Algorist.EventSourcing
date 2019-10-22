using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Storage.EventStore
{
    public class EventStoreSnapshotStorageProvider<TAggregate, TSnapshot> : EventStoreSnapshotStorageProvider<TAggregate, TSnapshot, Guid>, ISnapshotStorageProvider<TSnapshot>
        where TAggregate : AggregateRoot<Guid>
        where TSnapshot : ISnapshot<Guid>
    {
        public EventStoreSnapshotStorageProvider(IEventStoreStorageConnectionProvider eventStoreStorageConnectionProvider, ISerializer serializer) : base(eventStoreStorageConnectionProvider, serializer)
        {
        }
    }

    public class EventStoreSnapshotStorageProvider<TAggregate, TSnapshot, TAggregateKey> : EventStoreStorageProviderBase<TAggregateKey>, ISnapshotStorageProvider<TSnapshot, TAggregateKey>
        where TAggregate : AggregateRoot<TAggregateKey> 
        where TSnapshot : ISnapshot<TAggregateKey>
    {
        private readonly IEventStoreStorageConnectionProvider _eventStoreStorageConnectionProvider;

        public EventStoreSnapshotStorageProvider(IEventStoreStorageConnectionProvider eventStoreStorageConnectionProvider, ISerializer serializer) : base(serializer)
        {
            _eventStoreStorageConnectionProvider = eventStoreStorageConnectionProvider;
        }

        private Task<IEventStoreConnection> GetEventStoreConnectionAsync() => _eventStoreStorageConnectionProvider.GetConnectionAsync();

        protected override string GetStreamNamePrefix() => _eventStoreStorageConnectionProvider.SnapshotStreamPrefix;

        public int SnapshotFrequency => _eventStoreStorageConnectionProvider.SnapshotFrequency;

        public async Task<TSnapshot> GetSnapshotAsync(TAggregateKey aggregateId)
        {
            TSnapshot snapshot = default;
            var connection = await GetEventStoreConnectionAsync();

            var streamEvents = await connection.ReadStreamEventsBackwardAsync($"{AggregateIdToStreamName(typeof(TAggregate), aggregateId.ToString())}", StreamPosition.End, 1, false);

            if (!streamEvents.Events.Any()) return default(TSnapshot);
            var result = streamEvents.Events.FirstOrDefault();
            snapshot = DeserializeSnapshotEvent<TSnapshot>(result);

            return snapshot;
        }

        public async Task SaveSnapshotAsync(TSnapshot snapshot)
        {
            var connection = await GetEventStoreConnectionAsync();
            var snapshotEvent = SerializeSnapshotEvent(snapshot, snapshot.Version);

            await connection.AppendToStreamAsync($"{AggregateIdToStreamName(typeof(TAggregate), snapshot.AggregateId.ToString())}", ExpectedVersion.Any, snapshotEvent);
        }
    }
}
