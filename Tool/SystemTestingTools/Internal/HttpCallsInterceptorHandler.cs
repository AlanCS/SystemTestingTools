using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SystemTestingTools
{
    /// <summary>
    /// The handler that will allow SystemTestingTools to intercept OUTGOING http calls and return mocks
    /// </summary>
    internal class HttpCallsInterceptorHandler : DelegatingHandler
    {
        /// <summary>
        /// intercepts the http request that was about to hit a downstream server and return a mock instead
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var session = MockInstrumentation.GetSession();

            if (session == null) throw new ApplicationException("No session found");

            var requestWrapper = new HttpRequestMessageWrapper(request);

            MockInstrumentation.OutgoingRequests[session].Add(requestWrapper);

            // there could be more than one mock for the same endpoint, perhaps returning different responses, we grab the first
            var match = MockInstrumentation.MockedEndpoints[session].FirstOrDefault(c => c.Endpoint == requestWrapper.GetEndpoint());

            if (match == null)
                throw new ApplicationException($"No mocks found for [{requestWrapper.GetEndpoint()}], please make sure you called 'AppendMockHttpCall'");

            MockInstrumentation.MockedEndpoints[session].Remove(match);

            if (match.Exception != null)
                throw match.Exception;

            return match.Response;
        }
    }
}
