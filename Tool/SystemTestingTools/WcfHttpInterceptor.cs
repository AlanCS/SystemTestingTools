using System;
using System.ServiceModel.Description;

namespace SystemTestingTools
{
    /// <summary>
    /// methods that allows you to work with WCF http calls
    /// </summary>
    public static class WcfHttpInterceptor
    {
        /// <summary>
        /// obsolete method
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [Obsolete("Please use extension method ServiceEndpoint.EnableHttpInterception() instead; that will enable extension methods IServiceCollection.InterceptHttpCallsAfterSending() or IWebHostBuilder.InterceptHttpCallsBeforeSending() to see it", true)]
        public static IEndpointBehavior CreateRequestResponseRecorder(string folderName)
        {
            return null;
        }

        /// <summary>
        /// Obsolete method
        /// </summary>
        /// <returns></returns>
        [Obsolete("Please use extension method ServiceEndpoint.EnableHttpInterception() instead; that will enable extension methods IServiceCollection.InterceptHttpCallsAfterSending() or IWebHostBuilder.InterceptHttpCallsBeforeSending() to see it", true)]
        public static IEndpointBehavior CreateInterceptor()
        {
            return null;
        }
    }
}
