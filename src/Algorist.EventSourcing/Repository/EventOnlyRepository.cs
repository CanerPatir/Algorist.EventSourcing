using System;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Repository
{
    public class EventOnlyRepository<TAggregate> : EventOnlyRepository<TAggregate, Guid>, IRepository<TAggregate>
        where TAggregate : AggregateRoot<Guid>, new()
    {
        public EventOnlyRepository(IClock clock, IEventStorageProvider<TAggregate, Guid> eventStorageProvider, IEventPublisher eventPublisher) 
            : base(clock, eventStorageProvider, eventPublisher)
        {
        }
    }

    public class EventOnlyRepository<TAggregate, TAggregateKey> : Repository<TAggregate, IMockSnapShot<TAggregateKey>, TAggregateKey>
        where TAggregate : AggregateRoot<TAggregateKey>, new()
    {
        public EventOnlyRepository(IClock clock, IEventStorageProvider<TAggregate, TAggregateKey> eventStorageProvider, IEventPublisher<TAggregateKey> eventPublisher) 
            : base(clock, eventStorageProvider, eventPublisher, null)
        {
        }
    }

    public interface IMockSnapShot<out TAggregateKey> : ISnapshot<TAggregateKey>
    {
    }
}