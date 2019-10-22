using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Exceptions;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Storage.InMemory
{
    public class InMemoryEventStorageProvider<TAggregate> : InMemoryEventStorageProvider<TAggregate, Guid>, IEventStorageProvider<TAggregate>
        where TAggregate : AggregateRoot<Guid>
    {
        public InMemoryEventStorageProvider() : this(string.Empty)
        {
        }

        public InMemoryEventStorageProvider(string memoryDumpFile) : base(memoryDumpFile)
        {
        }
    }

    public class InMemoryEventStorageProvider<TAggregate, TAggregateKey> : IEventStorageProvider<TAggregate, TAggregateKey>
        where TAggregate : AggregateRoot<TAggregateKey>
    {
        private readonly string _memoryDumpFile;

        private readonly Dictionary<TAggregateKey, List<IEvent<TAggregateKey>>> _eventStream =
            new Dictionary<TAggregateKey, List<IEvent<TAggregateKey>>>();

        public InMemoryEventStorageProvider() : this(string.Empty) { }

        public InMemoryEventStorageProvider(string memoryDumpFile)
        {
            _memoryDumpFile = memoryDumpFile;

            if (!string.IsNullOrWhiteSpace(_memoryDumpFile) && File.Exists(_memoryDumpFile))
            {
                _eventStream = SerializerHelper
                    .LoadListFromFile<Dictionary<TAggregateKey, List<IEvent<TAggregateKey>>>>(_memoryDumpFile).First();
            }
        }

        public Task<IEnumerable<IEvent<TAggregateKey>>> GetEventsAsync(TAggregateKey aggregateId, int start, int count)
        {
            try
            {
                IEnumerable<IEvent<TAggregateKey>> result = null;

                if (_eventStream.ContainsKey(aggregateId))
                {
                    //this is needed for make sure it doesn't fail when we have int.maxValue for count
                    if (count > int.MaxValue - start)
                    {
                        count = int.MaxValue - start;
                    }

                    result = _eventStream[aggregateId].Where(
                            o =>
                                (_eventStream[aggregateId].IndexOf(o) >= start) &&
                                (_eventStream[aggregateId].IndexOf(o) < (start + count)))
                        .ToArray();
                }
                else
                {
                    result = new List<IEvent<TAggregateKey>>();
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                throw new AggregateNotFoundException($"The aggregate with {aggregateId} was not found. Details {ex.Message}");
            }
        }

        public Task<IEvent<TAggregateKey>> GetLastEventAsync(TAggregateKey aggregateId)
        {
            return _eventStream.ContainsKey(aggregateId) ? Task.FromResult(_eventStream[aggregateId].Last()) : Task.FromResult((IEvent<TAggregateKey>) null);
        }

        public Task SaveAsync(AggregateRoot<TAggregateKey> aggregate)
        {
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                if (_eventStream.ContainsKey(aggregate.Id) == false)
                {
                    _eventStream.Add(aggregate.Id, events.ToList());
                }
                else
                {
                    _eventStream[aggregate.Id].AddRange(events);
                }
            }

            if (!string.IsNullOrWhiteSpace(_memoryDumpFile))
            {
                SerializerHelper.SaveListToFile(_memoryDumpFile, new[] {_eventStream});
            }

            return Task.CompletedTask;
        }
    }
}