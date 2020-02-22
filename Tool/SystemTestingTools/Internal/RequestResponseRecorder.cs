using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SystemTestingTools
{
    /// <summary>
    /// Use this handle to record responses you get, so you can use it for mocking later
    /// </summary>
    internal class RequestResponseRecorder : DelegatingHandler
    {
        static internal IFileWriter FileWriter = null;
        private readonly string _callerPath;
        private string _toolNameAndVersion;

        /// <summary>
        /// Hander to save request and responses information
        /// </summary>
        /// <param name="Folder">Folder full path where the response text files will be saved</param>
        /// <param name="CallerPath">Please don't pass this parameter, it will be used by .net to track the file that called this method</param>
        public RequestResponseRecorder(string Folder, [CallerFilePath]string CallerPath = "")
        {
            if(FileWriter == null) FileWriter = new FileWriter(Folder);

            _callerPath = CallerPath;

            SetupToolNameVersion();
        }

        private void SetupToolNameVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            _toolNameAndVersion = string.Format("{0} {1}", fileVersion.ProductName, fileVersion.FileVersion);
        }

        /// <summary>
        /// Lets outgoing requests pass through, to log requests and responses
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            // we do all the work AFTER the request is sent, so we have the response and if an exception is throw we don't need to run any logic

            var summary = await Summarize(request, response);

            FileWriter.Write(summary);

            return response;
        }

        internal async Task<RequestResponse> Summarize(HttpRequestMessage request, HttpResponseMessage response)
        {
            var result = new RequestResponse();

            result.Metadata.DateTime = DateTime.Now;
            result.Metadata.Timezone = TimeZoneInfo.Local.ToString();
            result.Metadata.RequestedByCode = _callerPath;
            result.Metadata.User = $"{Environment.UserDomainName}\\{Environment.UserName}";
            result.Metadata.LocalMachine = Environment.MachineName;
            result.Metadata.ToolUrl = Constants.Website;
            result.Metadata.ToolNameAndVersion = _toolNameAndVersion;

            result.Request.Method = request.Method;
            result.Request.Url = request.RequestUri.ToString();
            if(request.Content != null)
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
                var dic = headers.ToList().ToDictionary(c=> c.Key, c=> c.Value.SeparatedByComa());
                return dic;
            }
        }
    }
}
