using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SystemTestingTools
{
    /// <summary>
    /// Contains a mock of usual instrumentation, containing logs and requests
    /// </summary>
    public static class MockInstrumentation
    {
        internal static Dictionary<string, List<MockEndpoint>> MockedEndpoints = new Dictionary<string, List<MockEndpoint>>();
        internal static Dictionary<string, List<LoggedEvent>> SessionLogs = new Dictionary<string, List<LoggedEvent>>();
        
        internal static Dictionary<string, List<HttpRequestMessageWrapper>> OutgoingRequests = new Dictionary<string, List<HttpRequestMessageWrapper>>();
        internal static IHttpContextAccessor context;

        internal static string GetSession()
        {
            // not in a seession, could be happening during app start up or scheduled process
            if (context?.HttpContext?.Request?.Headers == null) return null;

            return context.HttpContext.Request.Headers[Constants.headerName];
        }

        /// <summary>
        /// Get the logs not linked to any user session
        /// </summary>
        public static List<LoggedEvent> UnsessionedLogs = new List<LoggedEvent>();
    }
}
