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
        /// <returns></returns>
        public static IEndpointBehavior CreateRequestResponseRecorder(string folderName)
        {
            return new CustomEndpointBehaviour(() => new RequestResponseRecorder(folderName, true));
        }

        /// <summary>
        /// Create an IEndpointBehavior that will not allow requests to leave the machine, and will response with mock responses instead
        /// </summary>
        /// <returns></returns>
        public static IEndpointBehavior CreateInterceptor()
        {
            return new CustomEndpointBehaviour(() => new HttpCallsInterceptorHandler());
        }
    }
}
