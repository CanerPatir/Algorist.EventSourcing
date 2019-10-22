using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Algorist.EventSourcing.Storage.EventStore
{
    public interface IEventStoreStorageConnectionProvider
    {
        Task<IEventStoreConnection> GetConnectionAsync();
        string EventStreamPrefix { get; }
        string SnapshotStreamPrefix { get; }
        int SnapshotFrequency { get; }
        int PageSize { get; }
    }

    public class EventStoreStorageConnectionProvider : IEventStoreStorageConnectionProvider, IDisposable
    {
        private IEventStoreConnection _connection;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly Func<IEventStoreConnection> _eventStoreConnectionFactoryMethod;
        
        public EventStoreStorageConnectionProvider()
        {
            _eventStoreConnectionFactoryMethod = () => EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
        }

        public EventStoreStorageConnectionProvider(Func<IEventStoreConnection> eventStoreConnectionFactory)
        {
            _eventStoreConnectionFactoryMethod = eventStoreConnectionFactory;
        }

        public async Task<IEventStoreConnection> GetConnectionAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (_connection != null) return _connection;
                _connection = _eventStoreConnectionFactoryMethod.Invoke();
                await _connection.ConnectAsync();

                return _connection;
            }
            finally
            {
                _lock.Release();
            }
        }

        public string EventStreamPrefix => "Event-";
        public string SnapshotStreamPrefix => "Snapshot-";
        public int SnapshotFrequency => 2;
        public int PageSize => 200;

        public void CloseConnection()
        {
            _lock.Wait();
            try
            {
                if (_connection == null) return;
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}