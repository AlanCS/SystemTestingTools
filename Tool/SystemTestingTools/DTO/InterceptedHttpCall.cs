using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Contains details about the intercepted http call
    /// </summary>
    public class InterceptedHttpCall
    {
        /// <summary>
        /// The request sent
        /// </summary>
        public readonly HttpRequestMessage Request;
        /// <summary>
        /// How long it took for a response or exception
        /// </summary>
        public readonly TimeSpan Duration;
        /// <summary>
        /// The response received (if null, there is an exception)
        /// </summary>
        public readonly HttpResponseMessage Response;
        /// <summary>
        /// The exception thrown, there will be one if the response is null
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Get http context
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor
        {
            get
            {
                return Global.httpContextAccessor;
            }
        }

        /// <summary>
        /// The root folder where all the stubs are stored
        /// </summary>
        public FolderAbsolutePath RootStubsFolder
        {
            get
            {
                return Global.InterceptionConfiguration.RootStubsFolder;
            }
        }

        internal HttpResponseMessage NewResponse = null;

        internal bool ShouldKeepUnchanged = false;

        internal InterceptedHttpCall(HttpRequestMessage request, TimeSpan Duration, HttpResponseMessage response)
        {
            this.Request = request;
            this.Response = response;
            this.Duration = Duration;
        }

        internal InterceptedHttpCall(HttpRequestMessage request, TimeSpan Duration, Exception exception)
        {
            this.Request = request;
            this.Exception = exception;
            this.Duration = Duration;
        }

        /// <summary>
        /// Save the request and response to a file; this same file can be used later to be returned as a response
        /// it will not save or throw an error if there is no response (an exception occurred)
        /// </summary>
        /// <param name="relativeFolder">Relative folder inside the base folder where your stubs will be saved</param>
        /// <param name="fileName">stub filename, should not contain the extension, if none passed the HttpStatus will be the name</param>
        /// <param name="howManyFilesToKeep">if 0, it will keep an infinite number of files, and add a number at the end, example: FILENAME_0001, FILENAME_0002
        /// if 1, it will keep just one, and overwrite it everytime
        /// if >1, it will create files with a number at the end, but once the limit is reached, it will stop creating files, no overwritting</param>
        /// <returns>a task to be awaited</returns>
        public async Task SaveAsRecording(FolderRelativePath relativeFolder = null, FileName fileName = null, int howManyFilesToKeep = 0)
        {
            if (Response == null) return; // nothing to save if we have no response (an exception occurred)

            var requestResponse = await RecordingFormatter.Summarize(Request, Response, Duration);

            var finalFileName = Global.GlobalRecordingManager.Save(requestResponse, relativeFolder, fileName, howManyFilesToKeep);

            var recording = new Recording() { File = finalFileName, DateTime = DateTime.Now, Request = Request, Response = Response };

            RecordingCollection.Recordings.Add(recording);
        }

        /// <summary>
        /// Return the same HttpResponse or exception received, without any changes
        /// </summary>
        /// <returns></returns>
        public InterceptedHttpCall KeepResultUnchanged()
        {
            NewResponse = Response;
            ShouldKeepUnchanged = true;
            return this;
        }

        /// <summary>
        /// Return the recording file response
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public InterceptedHttpCall ReturnRecording(Recording recording, Reason reason)
        {
            if (recording == null) throw new ArgumentNullException(nameof(recording));
            NewResponse = recording.Response;

            AddResponseHeader($"Recording [{recording.File}] reason {reason}");
            return this;
        }

        /// <summary>
        /// Returns a stubbed response (obtained via one of the ResponseFactory methods)
        /// </summary>
        /// <param name="stub"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public InterceptedHttpCall ReturnStub(StubHttpResponseMessage stub, Reason reason)
        {
            if (stub == null) throw new ArgumentNullException(nameof(stub));
            NewResponse = stub;
            AddResponseHeader($"Stub [{stub.File}] reason {reason}");
            return this;
        }

        private void AddResponseHeader(string value)
        {
            HttpContextProxy.SetResponseHeaderValue(Constants.responseHeaderName, value);
        }

        /// <summary>
        /// Return a handcrafted http response 
        /// </summary>
        /// <param name="newResponse"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public InterceptedHttpCall ReturnHandCraftedResponse(HttpResponseMessage newResponse, Reason reason)
        {
            if (newResponse == null) throw new ArgumentNullException(nameof(newResponse));
            NewResponse = newResponse;
            AddResponseHeader($"HandCraftedResponse reason {reason}");
            return this;
        }

        /// <summary>
        /// Summarize the request and response
        /// </summary>
        /// <returns></returns>
        public string Summarize()
        {
            return $"{Request.GetEndpoint()} received " + (Exception != null  ? $"exception [{Exception.Message}]" : $"httpStatus [{Response.StatusCode}]");
        }

        public override string ToString()
        {
            return Summarize();
        }
    }
}
