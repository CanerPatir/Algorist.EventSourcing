using System;

namespace Algorist.EventSourcing.Core
{
    public interface ISnapshot : ISnapshot<Guid>
    {
    }

    public interface ISnapshot<out TAggregateKey>
    {
        TAggregateKey AggregateId { get; }
        int Version { get; }
    }
}