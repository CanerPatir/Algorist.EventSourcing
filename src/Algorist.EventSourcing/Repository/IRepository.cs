using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Repository
{
    public interface IRepository<TAggregate> : IRepository<TAggregate, Guid> where TAggregate : AggregateRoot<Guid>, new()
    {
    }

    public interface IRepository<TAggregate, in TAggregateKey> where TAggregate: AggregateRoot<TAggregateKey>, new()
    {
        Task<TAggregate> GetByIdAsync(TAggregateKey id);
        Task SaveAsync(TAggregate aggregate);
    }
}
