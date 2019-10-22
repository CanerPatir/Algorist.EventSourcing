using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core;

namespace Algorist.EventSourcing.Storage
{
    // This class is for storing snapshots as a series of events

    public interface IEventSnapshotStorageProvider<TSnapshot> : IEventSnapshotStorageProvider<TSnapshot, Guid> where TSnapshot : ISnapshot<Guid>
    {
    }

    public interface IEventSnapshotStorageProvider<TSnapshot, in TAggregateKey> : ISnapshotStorageProvider<TSnapshot, TAggregateKey> 
        where TSnapshot : ISnapshot<TAggregateKey>
    {
        Task<TSnapshot> GetSnapshotAsync(TAggregateKey aggregateId, int version);
    }
}