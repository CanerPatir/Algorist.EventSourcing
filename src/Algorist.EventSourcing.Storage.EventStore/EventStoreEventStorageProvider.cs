using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Storage.EventStore
{
    public class EventStoreEventStorageProvider<TAggregate> : EventStoreEventStorageProvider<TAggregate, Guid>, IEventStorageProvider<TAggregate>
        where TAggregate : AggregateRoot<Guid>
    {
        public EventStoreEventStorageProvider(IEventStoreStorageConnectionProvider eventStoreStorageConnectionProvider, ISerializer serializer)
            : base(eventStoreStorageConnectionProvider, serializer)
        {
        }
    }

    public class EventStoreEventStorageProvider<TAggregate, TAggregateKey> : EventStoreStorageProviderBase<TAggregateKey>, IEventStorageProvider<TAggregate, TAggregateKey> 
        where TAggregate : AggregateRoot<TAggregateKey>
    {
        private readonly IEventStoreStorageConnectionProvider _eventStoreStorageConnectionProvider;

        public EventStoreEventStorageProvider(IEventStoreStorageConnectionProvider eventStoreStorageConnectionProvider, ISerializer serializer) : base(serializer)
        {
            _eventStoreStorageConnectionProvider = eventStoreStorageConnectionProvider;
        }

        private Task<IEventStoreConnection> GetEventStoreConnectionAsync() => _eventStoreStorageConnectionProvider.GetConnectionAsync();

        protected override string GetStreamNamePrefix() => _eventStoreStorageConnectionProvider.EventStreamPrefix;

        public async Task<IEnumerable<IEvent<TAggregateKey>>> GetEventsAsync(TAggregateKey aggregateId, int start, int count)
        {
            var connection = await GetEventStoreConnectionAsync();
            var events = await ReadEvents(typeof(TAggregate), connection, aggregateId, start, count);

            return events;
        }

        public async Task<IEvent<TAggregateKey>> GetLastEventAsync(TAggregateKey aggregateId)
        {
            var connection = await GetEventStoreConnectionAsync();
            var results = await connection.ReadStreamEventsBackwardAsync($"{AggregateIdToStreamName(typeof(TAggregate), aggregateId.ToString())}", StreamPosition.End, 1, false);

            if (results.Status == SliceReadStatus.Success && results.Events.Any())
            {
                return DeserializeEvent(results.Events.First());
            }

            return null;
        }

        public async Task SaveAsync(AggregateRoot<TAggregateKey> aggregate)
        {
            var connection = await GetEventStoreConnectionAsync();
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                var lastVersion = aggregate.LastCommittedVersion;
                var lstEventData = events.Select(@event => SerializeEvent(@event, aggregate.LastCommittedVersion + 1)).ToList();

                await connection.AppendToStreamAsync($"{AggregateIdToStreamName(aggregate.GetType(), aggregate.Id.ToString())}",
                    (lastVersion < 0 ? ExpectedVersion.NoStream : lastVersion), lstEventData);
            }
        }

        private async Task<IEnumerable<IEvent<TAggregateKey>>> ReadEvents(Type aggregateType, IEventStoreConnection connection, TAggregateKey aggregateId, int start, int count)
        {
            var streamEvents = new List<ResolvedEvent>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = start < 0 ? StreamPosition.Start : start;

            //Read the stream using pagesize which was set before.
            //We only need to read the full page ahead if expected results are larger than the page size
            do
            {
                var nextReadCount = count - streamEvents.Count;

                if (nextReadCount > _eventStoreStorageConnectionProvider.PageSize)
                {
                    nextReadCount = _eventStoreStorageConnectionProvider.PageSize;
                }

                currentSlice = await connection.ReadStreamEventsForwardAsync($"{AggregateIdToStreamName(aggregateType, aggregateId.ToString())}", nextSliceStart, nextReadCount, false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            //Deserialize and add to events list

            return streamEvents.Select(DeserializeEvent).ToList();
        }
    }
}