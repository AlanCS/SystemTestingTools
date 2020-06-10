using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTestingTools
{
    internal static class Constants
    {
        internal static string headerName = "SystemTestingTools_Session";
        internal static string Website = "https://github.com/AlanCS/SystemTestingTools/";

        private static readonly object padlock = new object();
        private static JsonSerializerOptions options = null;
        internal static JsonSerializerOptions GetJsonOptions()
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
    }
}
