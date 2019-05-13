using System;
using System.Diagnostics;
using System.Net.Http;

namespace SystemTestingTools
{
    [DebuggerDisplay("{Endpoint}")]
    internal class MockEndpoint 
    {
        internal string Endpoint { get; private set; }
        internal readonly HttpResponseMessage Response;
        internal readonly Exception Exception;

        public MockEndpoint(HttpMethod httpMethod, System.Uri url, HttpResponseMessage response)
        {
            this.Response = response ?? throw new ArgumentNullException(nameof(response));
            SetEndpoint(httpMethod, url);
        }

        public MockEndpoint(HttpMethod httpMethod, System.Uri url, Exception exception)
        {
            this.Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            SetEndpoint(httpMethod, url);
        }

        private void SetEndpoint(HttpMethod httpMethod, System.Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            this.Endpoint = string.Format($"{httpMethod} {url}");
        }
    }
}
