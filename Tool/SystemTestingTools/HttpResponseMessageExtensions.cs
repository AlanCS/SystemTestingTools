using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <param name="dtoModifier"></param>
        public static async void ModifyJsonBody<T>(this HttpResponseMessage response, Action<T> dtoModifier) where T : class
        {
            var dto = await response.ReadJsonBody<T>();

            dtoModifier(dto);

            var enconding = Encoding.GetEncoding(response.Content.Headers.ContentType.CharSet);
            var mediaType = response.Content.Headers.ContentType.MediaType.ToString();

            response.Content = new StringContent(JsonSerializer.Serialize(dto), enconding, mediaType);
        }

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Read the response body and parset as a given class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        public static async Task<T> ReadJsonBody<T>(this HttpResponseMessage httpResponse) where T : class
        {
            var content = await httpResponse.ReadBody() ?? throw new ArgumentNullException("Body is null or empty");
            var dto = JsonSerializer.Deserialize<T>(content, options);
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
    }
}
