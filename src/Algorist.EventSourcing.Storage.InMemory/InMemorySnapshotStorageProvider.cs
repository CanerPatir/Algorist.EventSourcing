using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Storage.InMemory
{
    public class InMemorySnapshotStorageProvider<TSnapshot> : InMemorySnapshotStorageProvider<TSnapshot, Guid>, ISnapshotStorageProvider<TSnapshot>
        where TSnapshot : ISnapshot<Guid>
    {
        public InMemorySnapshotStorageProvider(int frequency, string memoryDumpFile) : base(frequency, memoryDumpFile)
        {
        }
    }

    public class InMemorySnapshotStorageProvider<TSnapshot, TAggregateKey> : ISnapshotStorageProvider<TSnapshot, TAggregateKey>
        where TSnapshot : ISnapshot<TAggregateKey>
    {
        private readonly Dictionary<TAggregateKey, TSnapshot> _items = new Dictionary<TAggregateKey, TSnapshot>();

        private readonly string _memoryDumpFile;
        public int SnapshotFrequency { get; }

        public InMemorySnapshotStorageProvider(int frequency) : this(frequency, string.Empty)
        {
        }

        public InMemorySnapshotStorageProvider(int frequency, string memoryDumpFile)
        {
            SnapshotFrequency = frequency;
            _memoryDumpFile = memoryDumpFile;

            if (!string.IsNullOrWhiteSpace(_memoryDumpFile) && File.Exists(_memoryDumpFile))
            {
                _items = SerializerHelper.LoadListFromFile<Dictionary<TAggregateKey, TSnapshot>>(_memoryDumpFile).First();
            }
        }

        public Task<TSnapshot> GetSnapshotAsync(TAggregateKey aggregateId)
        {
            return Task.FromResult(_items.ContainsKey(aggregateId) ? _items[aggregateId] : default(TSnapshot));
        }

        public Task SaveSnapshotAsync(TSnapshot snapshot)
        {
            if (_items.ContainsKey(snapshot.AggregateId))
            {
                _items[snapshot.AggregateId] = snapshot;
            }
            else
            {
                _items.Add(snapshot.AggregateId, snapshot);
            }

            if (!string.IsNullOrWhiteSpace(_memoryDumpFile))
            {
                SerializerHelper.SaveListToFile(_memoryDumpFile, new[] { _items });
            }

            return Task.CompletedTask;
        }
    }
}
