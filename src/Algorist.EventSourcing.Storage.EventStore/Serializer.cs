using System;
using System.Text;
using Newtonsoft.Json;

namespace Algorist.EventSourcing.Storage.EventStore
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
        T Deserialize<T>(byte[] data);
        
        object Deserialize(byte[] data, Type returnType);
    }

    public class Serializer : ISerializer
    {
        private Lazy<JsonSerializerSettings> DefaultSerializerSettings => new Lazy<JsonSerializerSettings>(GetSerializerSettings);

        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None
            };
        }

        public byte[] Serialize(object data) =>
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, DefaultSerializerSettings.Value));

        public T Deserialize<T>(byte[] data) =>
            JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data), DefaultSerializerSettings.Value);
        
        public object Deserialize(byte[] data, Type returnType) =>
            JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), returnType, DefaultSerializerSettings.Value);
    }
}