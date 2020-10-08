using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SystemTestingTools.Internal
{
    /// <summary>
    /// Use this handle to record responses you get, so you can use it for stubbing later
    /// </summary>
    internal class HttpCallInterceptor : DelegatingHandler
    {
        public HttpCallInterceptor(bool isWcf)
        {
            if (isWcf)
                InnerHandler = new HttpClientHandler(); // for some reason, WCF calls demand to have an InnerHandler, and HttpClient ones won't tolerate it            
        }

        /// <summary>
        /// Lets outgoing requests pass through, to log requests and responses
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ForwardHeader(request);

            if (Global.InterceptHttpBeforeSending)
                return await Before(request);

            return await After(request, cancellationToken);
        }

        private static void ForwardHeader(HttpRequestMessage request)
        {
            if (!request.Properties.ContainsKey("SystemTestingTools_Sent"))
                request.Properties.Add("SystemTestingTools_Sent", new List<DateTime> { DateTime.Now });
            else
                ((List<DateTime>)request.Properties["SystemTestingTools_Sent"]).Add(DateTime.Now);
        }

        private async Task<HttpResponseMessage> After(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();

            if (Global.InterceptionConfiguration.ForwardHeadersPrefix != null)
                if (Global.httpContextAccessor?.HttpContext?.Request?.Headers != null)
                    foreach (var keyValue in Global.httpContextAccessor.HttpContext.Request.Headers)
                        if (keyValue.Key.StartsWith(Global.InterceptionConfiguration.ForwardHeadersPrefix))
                            request.Headers.TryAddWithoutValidation(keyValue.Key, keyValue.Value.ToArray());

            InterceptedHttpCall call;
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                watch.Stop();

                call = new InterceptedHttpCall(request, TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds), response);
            }
            catch (Exception ex)
            {
                watch.Stop();

                call = new InterceptedHttpCall(request, TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds), ex);
            }

            if (Global.KeepListOfSentRequests)
            {
                var testSession = Global.TestStubs.GetSession();

                // outside a test session (in dev/test environments), this session will be null
                if (testSession != null)
                {
                    var clonedRequest = await request.Clone();
                    Global.TestStubs.OutgoingRequests[testSession].Add(clonedRequest);
                }
            }

            var afterReview = await Global.handlerAfterResponse(call);

            if (afterReview.ShouldKeepUnchanged)
            {
                if (afterReview.Exception != null) throw afterReview.Exception;
                if (afterReview.Response != null) return afterReview.Response;
            }

            if (afterReview.NewResponse == null)
                throw new Exception("Response not set, call a method like KeepUnchanged()");

            return await afterReview.NewResponse.Clone(); // we clone because if the response is used more than one time, the content stream has been read and can't be read again
        }

        private async Task<HttpResponseMessage> Before(HttpRequestMessage request)
        {
            var session = Global.TestStubs.GetSession();

            if (session == null) throw new ApplicationException("No session found");

            var clonedRequest = await request.Clone();
            Global.TestStubs.OutgoingRequests[session].Add(clonedRequest);

            var stubResponse = await FindStub(request, session);

            if (stubResponse == null)
                stubResponse = await FindStub(request, Constants.GlobalSession);

            if(stubResponse == null)
                throw new ApplicationException($"No stubs found for [{request.GetEndpoint()}] in session or globals, please make sure you called 'AppendHttpCallStub' or 'AppendGlobalHttpCallStub'");

            return stubResponse;
        }

        private static async Task<HttpResponseMessage> FindStub(HttpRequestMessage request, string session)
        {
            // there could be more than one stub for the same endpoint, perhaps returning different responses, we grab the first
            var match = Global.TestStubs.StubbedEndpoints[session].FirstOrDefault(c => c.IsMatch(request));

            if (match == null) return null;

            if (match.Counter >= 1)
            {
                if (match.Counter == 1) Global.TestStubs.StubbedEndpoints[session].Remove(match);
                match.Counter--;
            }

            if (match.Exception != null)
                throw match.Exception;

            return await match.Response.Clone(); // we clone because if the response is used more than one time, the content stream has been read and can't be read again
        }
    }
}
