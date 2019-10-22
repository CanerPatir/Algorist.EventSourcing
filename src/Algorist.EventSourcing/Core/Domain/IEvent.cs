using System;

namespace Algorist.EventSourcing.Core.Domain
{
    public interface IEvent : IEvent<Guid>
    {
    }

    public interface IEvent<TAggregateKey>
    {
//        TEventKey Id { get; }
        TAggregateKey AggregateId { get; set; }
        int TargetVersion { get; set; }
        DateTimeOffset EventCommittedTimestamp { get; set; }
        int EventSchemaVersion { get; set; }
        string CorrelationId { get; set; }
    }
}