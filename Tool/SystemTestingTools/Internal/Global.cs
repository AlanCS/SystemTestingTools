using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    internal partial class Global
    {
        private static readonly object padlock = new object();

        public static bool InterceptHttpBeforeSending = false;
        public static RecordingManager GlobalRecordingManager = null;
        public static InterceptionConfiguration InterceptionConfiguration = null;

        public static Func<InterceptedHttpCall, Task<InterceptedHttpCall>> handlerAfterResponse;

        internal static class TestStubs
        {
            internal static Dictionary<string, List<StubEndpoint>> StubbedEndpoints = new Dictionary<string, List<StubEndpoint>>()
            {
                {  Constants.GlobalSession, new List<StubEndpoint>() }
            };

            internal static Dictionary<string, List<LoggedEvent>> Logs = new Dictionary<string, List<LoggedEvent>>();
            internal static Dictionary<string, List<HttpRequestMessage>> OutgoingRequests = new Dictionary<string, List<HttpRequestMessage>>();
            internal static Dictionary<string, List<string>> Events = new Dictionary<string, List<string>>();

            internal static string GetSession()
            {
                return HttpContextProxy.GetHeaderValue(Constants.sessionHeaderName);
            }

            internal static string GetUrl()
            {
                return HttpContextProxy.GetUrl();
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
