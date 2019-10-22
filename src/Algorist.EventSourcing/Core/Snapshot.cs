using System;

namespace Algorist.EventSourcing.Core
{
    public abstract class Snapshot : Snapshot<Guid>, ISnapshot
    {
        protected Snapshot(Guid aggregateId, int version) : base(aggregateId, version)
        {
        }
    }

    public abstract class Snapshot<TAggregateKey> : ISnapshot<TAggregateKey>
    {
        protected Snapshot(TAggregateKey aggregateId, int version)
        {
//            Id = id;
            AggregateId = aggregateId;
            Version = version;
        }

//        public Guid Id { get; }
        public TAggregateKey AggregateId { get; }
        public int Version { get; }
    }
}