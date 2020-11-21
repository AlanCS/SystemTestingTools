using Newtonsoft.Json;
using System.Collections.Generic;

namespace SystemTestingTools.Internal
{
    internal static class JsonSerializerAdapter
    {
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        public static T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, serializerSettings);
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, serializerSettings);
        }
    }
}
