using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace SystemTestingTools.Internal
{
    internal static class StubsManager
    {
        private static string _SystemTestingToolsAppAndVersion = null;
        private static string SystemTestingToolsAppAndVersion
        {
            get
            {
                if (_SystemTestingToolsAppAndVersion != null) return _SystemTestingToolsAppAndVersion;

                var assembly = Assembly.GetExecutingAssembly();
                var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
                _SystemTestingToolsAppAndVersion = string.Format("{0} {1}", fileVersion.ProductName, fileVersion.FileVersion);

                return _SystemTestingToolsAppAndVersion;
            }
        }

        internal static async Task<RequestResponse> Summarize(HttpRequestMessage request, HttpResponseMessage response, long elapsedMilliseconds)
        {
            var result = new RequestResponse();

            result.Metadata.DateTime = DateTime.Now;
            result.Metadata.Timezone = TimeZoneInfo.Local.ToString();
            result.Metadata.RecordedFrom = $"{ContextRepo.GetAppNameAndVersion()} ({ContextRepo.GetUrl()})";

            result.Metadata.User = $"{Environment.UserDomainName}\\{Environment.UserName}";
            result.Metadata.LocalMachine = Environment.MachineName;
            result.Metadata.ToolUrl = Constants.Website;
            result.Metadata.ToolNameAndVersion = SystemTestingToolsAppAndVersion;
            result.Metadata.latencyMiliseconds = elapsedMilliseconds;

            result.Request.Method = request.Method;
            result.Request.Url = request.RequestUri.ToString();
            if (request.Content != null)
                result.Request.Body = await request.Content.ReadAsStringAsync();
            result.Request.Headers.ApppendHeaders(FormatHeaders(request.Headers));
            if (request.Content?.Headers != null)
                result.Request.Headers.ApppendHeaders(FormatHeaders(request.Content.Headers));

            result.Response.Status = response.StatusCode;
            result.Response.HttpVersion = response.Version;
            if (response.Content != null)
                result.Response.Body = await response.Content.ReadAsStringAsync();
            result.Response.Headers.ApppendHeaders(FormatHeaders(response.Headers));
            if (response.Content?.Headers != null)
                result.Response.Headers.ApppendHeaders(FormatHeaders(response.Content.Headers));

            return result;

            // new feature in C#7 (https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
            Dictionary<string, string> FormatHeaders(HttpHeaders headers)
            {
                var dic = headers.ToList().ToDictionary(c => c.Key, c => c.Value.SeparatedByComa());
                return dic;
            }
        }
    }
}
