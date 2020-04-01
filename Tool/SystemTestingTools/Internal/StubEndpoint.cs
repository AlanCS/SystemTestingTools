using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;

namespace SystemTestingTools
{
    [DebuggerDisplay("{Endpoint}")]
    internal class StubEndpoint 
    {
        private string endpoint;
        private readonly Dictionary<string, string> headerMatches;

        internal readonly HttpResponseMessage Response;
        internal readonly Exception Exception;

        public StubEndpoint(HttpMethod httpMethod, System.Uri url, HttpResponseMessage response, Dictionary<string, string> headerMatches)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response));
            this.headerMatches = headerMatches;
            SetEndpoint(httpMethod, url);
        }

        public StubEndpoint(HttpMethod httpMethod, System.Uri url, Exception exception, Dictionary<string, string> headerMatches)
        {
            this.Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.headerMatches = headerMatches;
            SetEndpoint(httpMethod, url);
        }

        private void SetEndpoint(HttpMethod httpMethod, System.Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            this.endpoint = string.Format($"{httpMethod} {url}");
        }

        public bool IsMatch(HttpRequestMessageWrapper request)
        {
            if (endpoint != request.GetEndpoint()) return false;

            if (headerMatches == null) return true; // nothing else to match, endpoint already matches

            foreach (var key in headerMatches.Keys)
            {
                if (!request.Request.Headers.Contains(key)) return false; // mandatory header doesn't exist

                if (request.Request.Headers.GetValues(key).FirstOrDefault() != headerMatches[key]) return false; // mandatory header doesn't exist
            }

            return true; // all headers match
        }
    }
}
