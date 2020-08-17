using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Use this handle to record responses you get, so you can use it for stubbing later
    /// </summary>
    internal class RequestResponseRecorder : DelegatingHandler
    {
        static internal IFileWriter FileWriter = null;


        public RequestResponseRecorder(string Folder, bool isWcf)
        {
            if(FileWriter == null) FileWriter = new FileWriter(Folder);

            if(isWcf)
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
            var watch = Stopwatch.StartNew();

            var response = await base.SendAsync(request, cancellationToken);

            watch.Stop();

            // we do all the work AFTER the request is sent, so we have the response and if an exception is throw we don't need to run any logic

            var summary = await StubsManager.Summarize(request, response, watch.ElapsedMilliseconds);

            FileWriter.Write(summary);

            return response;
        }

        
    }
}
