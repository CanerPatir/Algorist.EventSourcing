using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Algorist.EventSourcing.Storage.InMemory
{
    /// <summary>
    /// todo: https://github.com/dotnet/corefx/issues/36639
    /// </summary>
    public static class SerializerHelper
    {
        private static void SaveToJson(string strFile, List<object> objects)
        {
            var serializerSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var content = JsonConvert.SerializeObject(objects, serializerSetting);

            File.WriteAllText(strFile, content);
        }

        private static IEnumerable<object> LoadFromJson(string strFile)
        {
            var serializerSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var content = File.ReadAllText(strFile);
            var obj = JsonConvert.DeserializeObject<List<object>>(content, serializerSetting);

            return obj;
        }

        public static void SaveListToFile<T>(string file, IEnumerable<T> items)
        {
            var objects = items.Select(o => (object)o).ToList();
            SaveToJson(file, objects);
        }

        public static IList<T> LoadListFromFile<T>(string file)
        {
            var results = LoadFromJson(file);
            return results.Select(o => (T)o).ToList();
        }
    }
}
