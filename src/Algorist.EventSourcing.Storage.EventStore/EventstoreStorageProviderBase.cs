using System;
using EventStore.ClientAPI;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Storage.EventStore
{
    public abstract class EventStoreStorageProviderBase<TAggregateKey>
    {
        private readonly ISerializer _serializer;

        protected EventStoreStorageProviderBase(ISerializer serializer)
        {
            _serializer = serializer;
        }
        
        protected abstract string GetStreamNamePrefix();
        
        protected string AggregateIdToStreamName(Type t, string id)
        {
            //Ensure first character of type name is in lower case

            var prefix = GetStreamNamePrefix();
            return $"{char.ToLower(prefix[0])}{prefix.Substring(1)}{t.Name}{id}";
        }

        protected IEvent<TAggregateKey> DeserializeEvent(ResolvedEvent returnedEvent)
        {
            var header = _serializer.Deserialize<EventStoreMetaDataHeader>(returnedEvent.Event.Metadata);
            
            var returnType = Type.GetType(header.ClrType);

            return (IEvent<TAggregateKey>)_serializer.Deserialize(returnedEvent.Event.Data, returnType); 
        }

        protected EventData SerializeEvent(IEvent<TAggregateKey> @event, int commitNumber)
        {
            var header = new EventStoreMetaDataHeader()
            {
                ClrType = GetClrTypeName(@event),
                CommitNumber = commitNumber
            };

            return new EventData(Guid.NewGuid(), @event.GetType().Name, true, _serializer.Serialize(@event), _serializer.Serialize(header));
        }

        protected TSnapshot DeserializeSnapshotEvent<TSnapshot>(ResolvedEvent returnedEvent)
        {
            var header = _serializer.Deserialize<EventStoreMetaDataHeader>(returnedEvent.Event.Metadata);

            var returnType = Type.GetType(header.ClrType);

            return (TSnapshot) _serializer.Deserialize(returnedEvent.Event.Data, returnType);
        }

        protected EventData SerializeSnapshotEvent<TSnapshot>(TSnapshot snapshot, int commitNumber)
            where TSnapshot : ISnapshot<TAggregateKey>
        {
            var header = new EventStoreMetaDataHeader()
            {
                ClrType = GetClrTypeName(snapshot),
                CommitNumber = commitNumber
            };
            
            return new EventData(Guid.NewGuid(), snapshot.GetType().Name, true, _serializer.Serialize(snapshot), _serializer.Serialize(header));
        }

        private string GetClrTypeName(object @event)
        {
            return @event.GetType() + "," + @event.GetType().Assembly.GetName().Name;
        }
    }
}