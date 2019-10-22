using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Storage
{
    public interface IEventStorageProvider<TAggregate> : IEventStorageProvider<TAggregate, Guid> where TAggregate : AggregateRoot<Guid>
    {
    }

    public interface IEventStorageProvider<TAggregate, TAggregateKey> where TAggregate: AggregateRoot<TAggregateKey>
    {
        Task<IEnumerable<IEvent<TAggregateKey>>> GetEventsAsync(TAggregateKey aggregateId, int start, int count);
        Task<IEvent<TAggregateKey>> GetLastEventAsync(TAggregateKey aggregateId);
        Task SaveAsync(AggregateRoot<TAggregateKey> aggregate);
    }
}