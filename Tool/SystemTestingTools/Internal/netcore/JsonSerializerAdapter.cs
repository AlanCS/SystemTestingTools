using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTestingTools.Internal
{
    internal static class JsonSerializerAdapter
    {
        private static readonly object padlock = new object();

        private static JsonSerializerOptions options = null;
        private static JsonSerializerOptions GetJsonOptions()
        {
            if (options == null)
                lock (padlock)
                    if (options == null) // double lock for the win :)
                    {
                        options = new JsonSerializerOptions();
                        options.PropertyNameCaseInsensitive = true;
                        options.Converters.Add(new JsonStringEnumConverter());
                    }

            return options;
        }

        public static T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content, GetJsonOptions());
        }

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
