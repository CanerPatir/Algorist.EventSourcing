using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Tests.Integration.Mocks
{
    public class MockEventPublisher : MockEventPublisher<Guid>, IEventPublisher
    {
    }

    public class MockEventPublisher<TAggregateKey> : IEventPublisher<TAggregateKey>
    {
        public readonly Dictionary<TAggregateKey, IList<IEvent<TAggregateKey>>> Events =
            new Dictionary<TAggregateKey, IList<IEvent<TAggregateKey>>>();

        public Task PublishAsync(IEvent<TAggregateKey> @event)
        {
            var list = Events.ContainsKey(@event.AggregateId)
                ? Events[@event.AggregateId].ToList()
                : new List<IEvent<TAggregateKey>>();

            list.Add(@event);
            Events[@event.AggregateId] = list;
            return Task.CompletedTask;
        }
    }
}