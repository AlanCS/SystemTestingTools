using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SystemTestingTools.Internal.Enums;
using static SystemTestingTools.RequestResponse;

namespace SystemTestingTools.Internal
{
    internal static class RecordingFormatter
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

        public static string Format(RequestResponse log)
        {
            var content = new StringBuilder();

            // details about request
            content.AppendLine($"SystemTestingTools_Recording_V2");
            content.AppendLine($"Observations: !! EXPLAIN WHY THIS EXAMPLE IS IMPORTANT HERE !!");
            content.AppendLine($"Date: {log.Metadata.DateTime:yyyy-MM-dd HH:mm:ss.fff} {log.Metadata.Timezone}");
            content.AppendLine($"Recorded from: {log.Metadata.RecordedFrom}");
            content.AppendLine($"Local machine: {log.Metadata.LocalMachine}");
            content.AppendLine($"User: {log.Metadata.User}");
            content.AppendLine($"Using tool: {log.Metadata.ToolNameAndVersion} ({log.Metadata.ToolUrl})");
            content.AppendLine($"Duration: {log.Metadata.latencyMiliseconds} ms");

            content.AppendLine();
            content.AppendLine($"REQUEST");
            content.AppendLine($"{log.Request.Method.ToString().ToLower()} {log.Request.Url}");
            AddHeadersAndBody(log.Request);

            // lots of thought went into the bellow lines, because a separator needed to be created to split the response that
            // the method ResponseFactory.FromFiddlerLikeResponseFile can read, from (optional) comments
            // it has been decided that a new line + "--!?@Divider: some text here" + new line would be an acceptable separator
            // as we want to keep files in human readable/editable format
            // we still have the risk (very low) that such string could be found in real use case, it seemed like an acceptable 
            // risk/downside comparing to the upsides

            // we could have put the metadata + request details in a different file, but that would risk the 2 files from getting
            // separated, which defeats the purpose of a high quality documentation file of what request lead to what response
            content.AppendLine();
            content.AppendLine("--!?@Divider: Any text BEFORE this line = comments, AFTER = response in Fiddler like format");
            content.AppendLine();

            // details about response
            content.AppendLine($"HTTP/{log.Response.HttpVersion} {(int)log.Response.Status} {log.Response.Status}");
            AddHeadersAndBody(log.Response);

            return content.ToString();

            void AddHeadersAndBody(RequestResponseInfo requestOrResponse)
            {
                var contentType = Helper.GetKnownContentType(requestOrResponse.Headers.GetValueOrDefault("Content-Type"));

                bool IsResponseXml = requestOrResponse is ResponseInfo && contentType == KnownContentTypes.Xml;

                foreach (var header in requestOrResponse.Headers.ToList())
                {
                    // we skip Content-Length for XML responses (liked WCF), because for some reason when trying to read XML responses (that have been changed by formatting)
                    // the content-length being smaller seems to throw WCF readers out and cause exceptions
                    if (header.Key.ToLower() == "content-length" && IsResponseXml) continue;

                    content.AppendLine($"{header.Key}:{header.Value}");
                }
                content.AppendLine();

                if (requestOrResponse.Body == null)
                {
                    content.AppendLine(); // a NoContent (204) could return this, we mark an empty file
                    return;
                }

                switch (contentType)
                {
                    case KnownContentTypes.Json:
                        content.AppendLine(requestOrResponse.Body.FormatJson());
                        break;
                    case KnownContentTypes.Xml:
                        content.AppendLine(requestOrResponse.Body.FormatXml());
                        break;
                    case KnownContentTypes.Other:
                        content.AppendLine(requestOrResponse.Body);
                        break;
                }
            }
        }

        public static async Task<RequestResponse> Summarize(HttpRequestMessage request, HttpResponseMessage response, TimeSpan duration)
        {
            var result = new RequestResponse();

            result.Metadata.DateTime = DateTime.Now;
            result.Metadata.Timezone = TimeZoneInfo.Local.ToString();
            result.Metadata.RecordedFrom = $"{Global.GetAppNameAndVersion()} ({Global.TestStubs.GetUrl()})";

            result.Metadata.User = $"{Environment.UserDomainName}\\{Environment.UserName}";
            result.Metadata.LocalMachine = Environment.MachineName;
            result.Metadata.ToolUrl = Constants.Website;
            result.Metadata.ToolNameAndVersion = SystemTestingToolsAppAndVersion;
            result.Metadata.latencyMiliseconds = duration.TotalMilliseconds;

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

        public static bool IsValid(string content)
        {
            return content.Contains("--!?@Divider: ");
        }

        // part 1 = date time of the recording
        // part 2 = request details
        // part 3 = response details
        private static Regex RecordingRegex = new Regex(@".+?\nDate:(.+?)\n.+?REQUEST.+?\n(.+?)--\!\?@Divider:.+?\n(.*)", RegexOptions.Compiled | RegexOptions.Singleline);

        private static Regex DateRegex = new Regex(@"(2.+?)\(", RegexOptions.Compiled | RegexOptions.Singleline);



        public static Recording Read(string content)
        {
            var match = RecordingRegex.Match(content);

            if (!match.Success) return null;

            var result = new Recording();

            result.DateTime = DateTime.Parse(DateRegex.Match(match.Groups[1].Value).Groups[1].Value);
            result.Request = GetRequest(match.Groups[2].Value);
            result.Response = ResponseFactory.ReadFiddlerResponseFormat(match.Groups[3].Value);

            return result;
        }

        // http method
        // url
        // headers
        // white space
        // body (if any)
        private static Regex RequestRegex = new Regex(@"^(.+?) (.+?)\n(.+?)(\r\r|\n\n|\r\n\r\n)(.*)", RegexOptions.Compiled | RegexOptions.Singleline);
        private static HttpRequestMessage GetRequest(string requestContent)
        {
            var match = RequestRegex.Match(requestContent.TrimStart());            

            if (!match.Success) throw new ApplicationException($"Could not parse request data");

            var result = new HttpRequestMessage();

            var method = match.Groups[1].Value.Trim();
            try
            {
                result.Method = new HttpMethod(method);
            }
            catch (System.FormatException ex)
            {
                throw new Exception($"Method [{method}] is invalid, {requestContent}", ex);
            }

            result.RequestUri = new Uri(match.Groups[2].Value);

            result.Content = Helper.ParseHeadersAndBody(match.Groups[3].Value, match.Groups[5].Value, result.Headers);

            return result;
        }
    }
}
