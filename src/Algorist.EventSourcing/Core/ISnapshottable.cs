using System;

namespace Algorist.EventSourcing.Core
{
    public interface ISnapshottable<TSnapshot> : ISnapshottable<TSnapshot, Guid> where TSnapshot : ISnapshot
    {
    }

    public interface ISnapshottable<TSnapshot, TAggregateKey> where TSnapshot : ISnapshot<TAggregateKey>
    {
        TSnapshot TakeSnapshot();
        void ApplySnapshot(TSnapshot snapshot);
    }
}