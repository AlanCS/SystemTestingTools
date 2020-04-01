using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using System.Reflection;

namespace SystemTestingTools
{
    /// <summary>
    /// Contains a stub of usual instrumentation, containing logs and requests
    /// </summary>
    public static class ContextRepo
    {
        internal static Dictionary<string, List<StubEndpoint>> StubbedEndpoints = new Dictionary<string, List<StubEndpoint>>();
        internal static Dictionary<string, List<LoggedEvent>> SessionLogs = new Dictionary<string, List<LoggedEvent>>();
        
        internal static Dictionary<string, List<HttpRequestMessageWrapper>> OutgoingRequests = new Dictionary<string, List<HttpRequestMessageWrapper>>();
        internal static IHttpContextAccessor context;

        internal static string GetSession()
        {
            // not in a seession, could be happening during app start up or scheduled process
            if (context?.HttpContext?.Request?.Headers == null) return null;

            return context.HttpContext.Request.Headers[Constants.headerName];
        }

        internal static string GetUrl()
        {
            // not in a seession, could be happening during app start up or scheduled process
            if (context?.HttpContext?.Request == null) return "No httpcontext available";

            return $"{context.HttpContext.Request.Method} {context.HttpContext.Request.GetDisplayUrl()}";
        }

        private static readonly object padlock = new object();
        private static string AppNameAndVersion = null;
        internal static string GetAppNameAndVersion()
        {
            if (AppNameAndVersion == null)
                lock (padlock)
                    if (AppNameAndVersion == null) // double lock for the win :)
                    {
                        var assembly = Assembly.GetEntryAssembly().GetName();
                        AppNameAndVersion = $"{assembly.Name} {assembly.Version}";
                    }

            return AppNameAndVersion;
        }

        /// <summary>
        /// Get the logs not linked to any user session
        /// </summary>
        public static List<LoggedEvent> UnsessionedLogs = new List<LoggedEvent>();
    }
}
