using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Core
{
    public interface IEventPublisher : IEventPublisher<Guid>
    {
    }

    public interface IEventPublisher<TAggregateKey>
    {
        Task PublishAsync(IEvent<TAggregateKey> @event);
    }
}