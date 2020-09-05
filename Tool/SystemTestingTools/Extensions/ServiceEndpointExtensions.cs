using System.ServiceModel.Description;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Extensions for ServiceEndpoint (WCF) configuration
    /// </summary>
    public static class ServiceEndpointExtensions
    {
        /// <summary>
        /// This enables the interception of http calls in this endpoint for stubbing:
        /// - in automated testing, by calling
        /// - in development environment, by calling first services.InterceptHttpCallsAfterSending
        /// </summary>
        /// <param name="endpoint"></param>
        public static void EnableHttpInterception(this ServiceEndpoint endpoint)
        {
            if (Constants.GlobalConfiguration == null && !Constants.InterceptHttpBeforeSending) 
                throw new System.Exception("Add the line services.InterceptHttpCallsAfterSending (for development environment interception) or IWebHostBuilder.ConfigureInterceptionOfHttpClientCalls (for automated testing interception) first");

            endpoint.EndpointBehaviors.Add(new CustomEndpointBehaviour(() => new HttpCallInterceptor(true)));
        }
    }
}
