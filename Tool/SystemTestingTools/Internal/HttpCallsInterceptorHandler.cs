using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SystemTestingTools
{
    /// <summary>
    /// The handler that will allow SystemTestingTools to intercept OUTGOING http calls and return stubs
    /// </summary>
    internal class HttpCallsInterceptorHandler : DelegatingHandler
    {
        /// <summary>
        /// intercepts the http request that was about to hit a downstream server and return a stub instead
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var session = ContextRepo.GetSession();

            if (session == null) throw new ApplicationException("No session found");

            var requestWrapper = new HttpRequestMessageWrapper(request);

            ContextRepo.OutgoingRequests[session].Add(requestWrapper);

            // there could be more than one stub for the same endpoint, perhaps returning different responses, we grab the first
            var match = ContextRepo.StubbedEndpoints[session].FirstOrDefault(c => c.IsMatch(requestWrapper));

            if (match == null)
                throw new ApplicationException($"No stubs found for [{requestWrapper.GetEndpoint()}], please make sure you called 'AppendHttpCallStub'");

            ContextRepo.StubbedEndpoints[session].Remove(match);

            if (match.Exception != null)
                throw match.Exception;

            return match.Response;
        }
    }
}
