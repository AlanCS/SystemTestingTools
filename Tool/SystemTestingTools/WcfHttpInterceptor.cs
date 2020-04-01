using System.Runtime.CompilerServices;
using System.ServiceModel.Description;

namespace SystemTestingTools
{
    /// <summary>
    /// methods that allows you to work with WCF http calls
    /// </summary>
    public static class WcfHttpInterceptor
    {
        /// <summary>
        /// Create an IEndpointBehavior that will save requests and responses in a text file in a folder
        /// </summary>
        /// <param name="folderName">where the files will be saved</param>
        /// <param name="callerPath">Please don't pass this parameter, it will be used by .net to track the file that called this method</param>
        /// <returns></returns>
        public static IEndpointBehavior CreateRequestResponseRecorder(string folderName, [CallerFilePath]string callerPath = "")
        {
            return new CustomEndpointBehaviour(() => new RequestResponseRecorder(folderName, true, callerPath));
        }

        /// <summary>
        /// Create an IEndpointBehavior that will not allow requests to leave the machine, and will response with stub responses instead
        /// </summary>
        /// <returns></returns>
        public static IEndpointBehavior CreateInterceptor()
        {
            return new CustomEndpointBehaviour(() => new HttpCallsInterceptorHandler());
        }
    }
}
