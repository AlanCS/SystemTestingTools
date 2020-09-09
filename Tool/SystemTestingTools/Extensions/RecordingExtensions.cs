using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Extensions related to recordings (sub type of Stubs, that also contain the request detail)
    /// </summary>
    public static class RecordingExtensions
    {
        private static HttpClient defaultClient = new HttpClient();

        /// <summary>
        /// Re-send the request of the recording, using exactly the same URL and headers
        /// </summary>
        /// <param name="recording">the recording whose request will be re-sent</param>
        /// <param name="client">the httpClient to be used for the call, if none passed; a new one will be created. Pass one if you need non default configurations such as proxy or timeout</param>
        /// <returns>the response for the request</returns>
        public static Task<HttpResponseMessage> ReSendRequest(this Recording recording, HttpClient client = null)
        {
            return (client ?? defaultClient).SendAsync(recording.Request);
        }

        /// <summary>
        /// Re-send the request of the recording, using exactly the same URL and headers, and update the relevant file
        /// </summary>
        /// <param name="recordings">the list of recordings whose request will be re-sent</param>
        /// <param name="client">the httpClient to be used for the calls, if none passed; a new one will be created. Pass one if you need non default configurations such as proxy or timeout</param>
        public static async Task ReSendRequestAndUpdateFile(this IEnumerable<Recording> recordings, HttpClient client = null)
        {
            if (!recordings.Any()) return;

            var recordingManager = new RecordingManager();
            foreach (var recording in recordings)
            {
                var watch = Stopwatch.StartNew();
                recording.Response = await recording.ReSendRequest(client);
                watch.Stop();
                recording.DateTime = DateTime.Now;
                var requestResponse = await RecordingFormatter.Summarize(recording.Request, recording.Response, TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds));
                recordingManager.SaveToFile(recording.FileFullPath, requestResponse);
            }
        }
    }
}
