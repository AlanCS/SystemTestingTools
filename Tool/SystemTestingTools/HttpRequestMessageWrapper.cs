using System;
using System.Net.Http;

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

        internal string GetEndpoint()
        {
            var endpoint = string.Format($"{Request.Method} {Request.RequestUri}");
            return endpoint;
        }
    }
}
