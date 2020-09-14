using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SystemTestingTools.Internal;


namespace SystemTestingTools
{
    internal class Global
    {
        private static readonly object padlock = new object();

        private static JsonSerializerOptions options = null;
        internal static JsonSerializerOptions GetJsonOptions()
        {
            if (options == null)
                lock (padlock)
                    if (options == null) // double lock for the win :)
                    {
                        options = new JsonSerializerOptions();
                        options.PropertyNameCaseInsensitive = true;
                        options.Converters.Add(new JsonStringEnumConverter());
                    }

            return options;
        }

        public static bool InterceptHttpBeforeSending = false;
        public static RecordingManager GlobalRecordingManager = null;
        public static InterceptionConfiguration InterceptionConfiguration = null;

        public static Func<InterceptedHttpCall, Task<InterceptedHttpCall>> handlerAfterResponse;

        internal static IHttpContextAccessor httpContextAccessor;

        internal static void AddResponseHeader(string reason)
        {
            // not in a seession, could be happening during app start up or scheduled process
            if (httpContextAccessor?.HttpContext?.Response?.Headers == null) return;

            httpContextAccessor.HttpContext.Response.Headers.Append(Constants.responseHeaderName, reason);
        }

        internal static class TestStubs
        {
            internal static Dictionary<string, List<StubEndpoint>> StubbedEndpoints = new Dictionary<string, List<StubEndpoint>>()
            {
                {  Constants.GlobalSession, new List<StubEndpoint>() }
            };

            internal static Dictionary<string, List<LoggedEvent>> SessionLogs = new Dictionary<string, List<LoggedEvent>>();
            internal static Dictionary<string, List<HttpRequestMessage>> OutgoingRequests = new Dictionary<string, List<HttpRequestMessage>>();

            internal static string GetSession()
            {
                // not in a seession, could be happening during app start up or scheduled process
                if (httpContextAccessor?.HttpContext?.Request?.Headers == null) return null;

                return httpContextAccessor.HttpContext.Request.Headers[Constants.sessionHeaderName];
            }

            internal static string GetUrl()
            {
                // not in a seession, could be happening during app start up or scheduled process
                if (httpContextAccessor?.HttpContext?.Request == null) return "No httpcontext available";

                return $"{httpContextAccessor.HttpContext.Request.Method} {httpContextAccessor.HttpContext.Request.GetDisplayUrl()}";
            }

        }

        private static string AppNameAndVersion = null;
        internal static bool KeepListOfSentRequests = true;

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
    }
}
