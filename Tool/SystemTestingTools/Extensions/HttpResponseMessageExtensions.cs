using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Useful extensions for the microsoft HttpResponseMessage
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Parse the response body as a class, change it and store it again in the response
        /// </summary>
        public static async void ModifyJsonBody<T>(this HttpResponseMessage response, Action<T> dtoModifier) where T : class
        {
            var dto = await response.ReadJsonBody<T>();

            dtoModifier(dto);

            var enconding = Encoding.GetEncoding(response.Content.Headers.ContentType.CharSet);
            var mediaType = response.Content.Headers.ContentType.MediaType.ToString();

            response.Content = new StringContent(JsonSerializer.Serialize(dto), enconding, mediaType);
        }

        /// <summary>
        /// Read the response body and parse it as a given class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        public static async Task<T> ReadJsonBody<T>(this HttpResponseMessage httpResponse) where T : class
        {
            var content = await httpResponse.ReadBody() ?? throw new ArgumentNullException("Body is null or empty");
            var dto = JsonSerializer.Deserialize<T>(content, Constants.GetJsonOptions());
            return dto;
        }

        /// <summary>
        /// Read the response body as string
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        public static async Task<string> ReadBody(this HttpResponseMessage httpResponse)
        {
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            return responseBody;
        }

        /// <summary>
        /// Get the values of a header, null if not present
        /// </summary>
        /// <param name="key"></param>
        /// <param name="separator">the separator string to join multiple values</param>
        /// <returns></returns>
        public static string GetHeaderValue(this HttpResponseMessage response, string key, string separator = Constants.DefaultSeparator)
        {
            return response.Headers.GetHeaderValue(key, separator);
        }

        /// <summary>
        /// Get the values of a header, null if not present
        /// </summary>
        /// <param name="key"></param>
        /// <param name="separator">the separator string to join multiple values</param>
        /// <returns></returns>
        public static string GetHeaderValue(this HttpContent content, string key, string separator = Constants.DefaultSeparator)
        {
            return content.Headers.GetHeaderValue(key, separator);
        }
    }
}
