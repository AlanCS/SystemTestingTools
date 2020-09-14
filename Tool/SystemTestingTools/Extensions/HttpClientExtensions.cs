using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// extensions to HttpClient to allow stubbing and assertions
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Create a new session, so logs and requests and responses can be tracked
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns>the SessionId, can be used to interact with other tools that might need a session</returns>
        public static string CreateSession(this HttpClient httpClient)
        {
            var sessionId = Guid.NewGuid().ToString();
            httpClient.DefaultRequestHeaders.Add(Constants.sessionHeaderName, sessionId);
            Global.TestStubs.StubbedEndpoints.Add(sessionId, new List<StubEndpoint>());
            Global.TestStubs.SessionLogs.Add(sessionId, new List<LoggedEvent>());
            Global.TestStubs.OutgoingRequests.Add(sessionId, new List<HttpRequestMessage>());
            return sessionId;
        }

        /// <summary>
        /// Get Session ID allocated to httpClient, can be useful to interact with other tools
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static string GetSessionId(this HttpClient httpClient)
        {
            var values = httpClient.DefaultRequestHeaders.GetValues(Constants.sessionHeaderName);

            if (values.Count() != 1) throw new ApplicationException("You need to call 'CreateSession' first");

            return values.First();
        }

        /// <summary>
        /// Will return the response when a matching call gets fired, but only once
        /// if you expect this endpoint to be called X times, add X stub endpoints
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="Url"></param>
        /// <param name="response">You can create your response, or use ResponseFactory to create one for you</param>
        /// <param name="headerMatches">Optional headers that must match for the response to be returned</param>
        /// <param name="counter">How many times should this response be returned, if requested one more time than the limit, it will throw an exception. choose 0 for infinite</param>
        public static void AppendHttpCallStub(this HttpClient httpClient, HttpMethod httpMethod, Uri Url, HttpResponseMessage response, Dictionary<string, string> headerMatches = null, int counter = 1)
        {
            var sessionId = GetSessionId(httpClient);

            Global.TestStubs.StubbedEndpoints[sessionId].Add(new StubEndpoint(httpMethod, Url, response, headerMatches, counter));
        }

        /// <summary>
        /// Will throw an exception when a matching call gets fired, but only once
        /// if you expect this endpoint to be called X times, add X stub endpoints
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="httpMethod"></param>
        /// <param name="Url"></param>
        /// <param name="exception">The exception that will be throw when HttpClient.SendAsync gets called</param>
        /// <param name="headerMatches">Optional headers that must match for the response to be returned</param>
        /// /// <param name="counter">How many times should this response be returned, if requested one more time than the limit, it will throw an exception. choose 0 for infinite</param>
        public static void AppendHttpCallStub(this HttpClient httpClient, HttpMethod httpMethod, System.Uri Url, Exception exception, Dictionary<string, string> headerMatches = null, int counter = 1)
        {
            var sessionId = GetSessionId(httpClient);

            Global.TestStubs.StubbedEndpoints[sessionId].Add(new StubEndpoint(httpMethod, Url, exception, headerMatches, counter));
        }

        /// <summary>
        /// Get all logs related to the current session
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static List<LoggedEvent> GetSessionLogs(this HttpClient httpClient)
        {
            var sessionId = GetSessionId(httpClient);

            return Global.TestStubs.SessionLogs[sessionId];
        }

        /// <summary>
        /// Get all outgoing Http requests related to the current session
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static List<HttpRequestMessage> GetSessionOutgoingRequests(this HttpClient httpClient)
        {
            if (!Global.KeepListOfSentRequests) return null;

            var sessionId = GetSessionId(httpClient);

            return Global.TestStubs.OutgoingRequests[sessionId];
        }

        [Obsolete("This call has been renamed to AppendHttpCallStub", true)]
        public static void AppendMockHttpCall(this HttpClient httpClient, HttpMethod httpMethod, System.Uri Url, HttpResponseMessage response, Dictionary<string, string> headerMatches = null)
        {
            // old method, renamed it because Stub is a better word for what it does
        }

        [Obsolete("This call has been renamed to AppendHttpCallStub", true)]
        public static void AppendMockHttpCall(this HttpClient httpClient, HttpMethod httpMethod, System.Uri Url, Exception exception, Dictionary<string, string> headerMatches = null)
        {
            // old method, renamed it because Stub is a better word for what it does
        }
    }
}
