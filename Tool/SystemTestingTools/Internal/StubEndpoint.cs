using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;

namespace SystemTestingTools.Internal
{
    [DebuggerDisplay("{Endpoint}")]
    internal class StubEndpoint
    {
        private string endpoint;
        private readonly Dictionary<string, string> headerMatches;

        internal readonly HttpResponseMessage Response;
        internal readonly Exception Exception;
        internal int Counter = 1;

        public StubEndpoint(HttpMethod httpMethod, Uri url, HttpResponseMessage response, Dictionary<string, string> headerMatches, int counter)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
            this.headerMatches = headerMatches;
            SetEndpoint(httpMethod, url);
            Counter = counter;
        }

        public StubEndpoint(HttpMethod httpMethod, Uri url, Exception exception, Dictionary<string, string> headerMatches, int counter)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.headerMatches = headerMatches;
            SetEndpoint(httpMethod, url);
            Counter = counter;
        }

        private void SetEndpoint(HttpMethod httpMethod, Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            endpoint = string.Format($"{httpMethod} {url}");
        }

        public bool IsMatch(HttpRequestMessage request)
        {
            if (endpoint != request.GetEndpoint()) return false;

            if (headerMatches == null) return true; // nothing else to match, endpoint already matches

            foreach (var key in headerMatches.Keys)
            {
                if (!request.Headers.Contains(key)) return false; // mandatory header doesn't exist

                if (request.Headers.GetValues(key).FirstOrDefault() != headerMatches[key]) return false; // mandatory header doesn't exist
            }

            return true; // all headers match
        }
    }
}
