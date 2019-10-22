using System;

namespace Algorist.EventSourcing.Core.Domain
{
    public abstract class Event : Event<Guid>
    {
        protected Event(Guid aggregateId) : base(aggregateId)
        {
        }

        protected Event(Guid aggregateId, int targetVersion) : base(aggregateId, targetVersion)
        {
        }

        protected Event(Guid aggregateId, int targetVersion, int eventSchemaVersion, string correlationId) 
            : base(aggregateId, targetVersion, eventSchemaVersion, correlationId)
        {
        }
    }

    public abstract class Event<TAggregateKey> : IEvent<TAggregateKey>
    {
        protected Event(TAggregateKey aggregateId, int targetVersion = (int) StreamState.NoStream) : this(aggregateId, targetVersion, 0, string.Empty)
        {
        }

        protected Event(TAggregateKey aggregateId, int targetVersion, int eventSchemaVersion, string correlationId)
        {
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
            EventSchemaVersion = eventSchemaVersion;
            CorrelationId = correlationId;
        }

        public TAggregateKey AggregateId { get; set; }
        public int TargetVersion { get; set; }
        public DateTimeOffset EventCommittedTimestamp { get; set; }
        public int EventSchemaVersion { get; set; }
        public string CorrelationId { get; set; }
    }
}