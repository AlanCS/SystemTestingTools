using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SystemTestingTools
{
    /// <summary>
    /// Wraps requests
    /// </summary>
    public class HttpRequestMessageWrapper
    {
        /// <summary>
        /// The outgoing http request
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// When the Http request happened
        /// </summary>
        public DateTime RequestTime { get; private set; }

        internal HttpRequestMessageWrapper(HttpRequestMessage request)
        {
            this.Request = request;
            this.RequestTime = DateTime.Now;
        }

        /// <summary>
        /// Get the full endpoint, in the format "HttpVerb FullUrl"
        /// </summary>
        /// <returns></returns>
        public string GetEndpoint()
        {
            var endpoint = string.Format($"{Request.Method} {Request.RequestUri}");
            return endpoint;
        }

        /// <summary>
        /// Get the values of a header separated by comma, null if not present
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetHeaderValue(string key)
        {
            if (!Request.Headers.Contains(key)) return null;

            return string.Join(",", Request.Headers.GetValues(key));
        }

        /// <summary>
        /// Read the request body as string
        /// </summary>
        public async Task<string> ReadBody()
        {
            var requestBody = await Request.Content.ReadAsStringAsync();
            return requestBody;
        }

        /// <summary>
        /// Read the request body and parse it as a given class
        /// </summary>
        public async Task<T> ReadJsonBody<T>() where T : class
        {
            var content = await ReadBody() ?? throw new ArgumentNullException("Body is null or empty");
            var dto = JsonSerializer.Deserialize<T>(content, Constants.GetJsonOptions());
            return dto;
        }
    }
}
