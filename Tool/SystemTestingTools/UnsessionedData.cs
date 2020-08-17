using System;
using System.Collections.Generic;
using System.Net.Http;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Contains data not associated with any session
    /// </summary>
    public static class UnsessionedData
    {
        /// <summary>
        /// Get the logs not linked to any user session
        /// </summary>
        public static List<LoggedEvent> UnsessionedLogs = new List<LoggedEvent>();

        /// <summary>
        /// Add a global stub, so it can be used infinitely if a stub is not found for a session. Basically a 'default' 
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="Url"></param>
        /// <param name="response">You can create your response, or use ResponseFactory to create one for you</param>
        /// <param name="headerMatches">Optional headers that must match for the response to be returned</param>
        public static void AppendGlobalHttpCallStub(HttpMethod httpMethod, Uri Url, HttpResponseMessage response, Dictionary<string, string> headerMatches = null)
        {
            Constants.TestStubs.StubbedEndpoints[Constants.GlobalSession].Add(new StubEndpoint(httpMethod, Url, response, headerMatches, 0));
        }

        /// <summary>
        /// Add a global stub to return an exception when a match is found, so it can be used infinitely nothing is found for a session. Basically a 'default' 
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="Url"></param>
        /// <param name="exception">The exception that will be throw when HttpClient.SendAsync gets called</param>
        /// <param name="headerMatches">Optional headers that must match for the response to be returned</param>
        public static void AppendGlobalHttpCallStub(HttpMethod httpMethod, Uri Url, Exception exception, Dictionary<string, string> headerMatches = null)
        {
            Constants.TestStubs.StubbedEndpoints[Constants.GlobalSession].Add(new StubEndpoint(httpMethod, Url, exception, headerMatches, 0));
        }
    }
}
