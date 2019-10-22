using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core;

namespace Algorist.EventSourcing.Storage
{
    public interface ISnapshotStorageProvider<TSnapshot> : ISnapshotStorageProvider<TSnapshot, Guid> where TSnapshot : ISnapshot<Guid>
    {
    }

    public interface ISnapshotStorageProvider<TSnapshot, in TAggregateKey> where TSnapshot: ISnapshot<TAggregateKey>
    {
        int SnapshotFrequency { get; }
        Task<TSnapshot> GetSnapshotAsync(TAggregateKey aggregateId);
        Task SaveSnapshotAsync(TSnapshot snapshot);
    }
}