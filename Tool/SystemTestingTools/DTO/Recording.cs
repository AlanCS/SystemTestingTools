using System;
using System.Net.Http;

namespace SystemTestingTools
{
    /// <summary>
    /// A recording of a request / response
    /// </summary>
    public class Recording
    {
        /// <summary>
        /// File (relative path) where it came from
        /// </summary>
        public string File = string.Empty;


        internal string FileFullPath = string.Empty;

        /// <summary>
        /// when the recording happened
        /// </summary>
        public DateTime DateTime = DateTime.MinValue;

        /// <summary>
        /// the request details
        /// </summary>
        public HttpRequestMessage Request;

        /// <summary>
        /// the response details
        /// </summary>
        public HttpResponseMessage Response;

        public override string ToString()
        {
            return $"{File} returns {Response.StatusCode} from {Request.GetEndpoint()}";
        }
    }
}
