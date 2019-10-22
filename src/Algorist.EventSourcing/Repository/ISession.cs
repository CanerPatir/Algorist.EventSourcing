
using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Repository
{
    public interface ISession<TAggregate> : ISession<TAggregate, Guid> 
        where TAggregate : AggregateRoot<Guid>, new()
    {
    }

    public interface ISession<TAggregate, in TAggregateKey> : IDisposable
        where TAggregate : AggregateRoot<TAggregateKey>, new()
    {
        Task<TAggregate> GetByIdAsync(TAggregateKey id);
        void Attach(TAggregate aggregate);
        void Detach(TAggregate aggregate);
        Task SaveAsync();
        void DetachAll();
    }
}
