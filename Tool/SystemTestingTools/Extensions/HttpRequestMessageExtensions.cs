using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// HttpRequestMessage extension methods
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Get the dates this request was sent (if a retry was configured, there might be more than one). If it was intercepted by SystemTestingTools, this will be present
        /// </summary>
        public static List<DateTime> GetDatesSent(this HttpRequestMessage request)
        {
            if (!request.Properties.TryGetValue("SystemTestingTools_Sent", out object datesObj)) return new List<DateTime>();
            return (List<DateTime>)datesObj;
        }

        /// <summary>
        /// Get the full endpoint, in the format "HttpVerb FullUrl"
        /// </summary>
        public static string GetEndpoint(this HttpRequestMessage request)
        {
            var endpoint = string.Format($"{request.Method} {request.RequestUri}");
            return endpoint;
        }


        /// <summary>
        /// Get the values of a header (joined by 'separator' if more than one), null if not present
        /// </summary>
        /// <param name="key">header key</param>
        /// <param name="separator">the separator string to join multiple values</param>
        public static string GetHeaderValue(this HttpRequestMessage request, string key, string separator = Constants.DefaultSeparator)
        {
            return request.Headers.GetHeaderValue(key, separator);
        }

        /// <summary>
        /// Gets the SOAP Action header value for a SOAP (WCF) call
        /// this can be useful to identify the method called, as the URL is the same for all methods
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetSoapAction(this HttpRequestMessage request)
        {
            var result = request.GetHeaderValue("SOAPAction");

            if (result == null) return null;

            return result.Trim('"');
        }

        /// <summary>
        /// Read the request body as string
        /// </summary>
        public static async Task<string> ReadBody(this HttpRequestMessage request)
        {
            var requestBody = await request.Content.ReadAsStringAsync();
            return requestBody;
        }

        /// <summary>
        /// Read the request body and parse it as a given class
        /// </summary>
        public static async Task<T> ReadJsonBody<T>(this HttpRequestMessage request) where T : class
        {
            var content = await request.ReadBody() ?? throw new ArgumentNullException("Body is null or empty");
            var dto = JsonSerializerAdapter.Deserialize<T>(content);
            return dto;
        }

        /// <summary>
        /// Gets the value for a parameter in the query string
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetQueryValue(this HttpRequestMessage request, string key)
        {
            if (QueryHelpers.ParseQuery(request.RequestUri.Query).TryGetValue(key, out var result))
                return result;

            return null;
        }
    }
}
