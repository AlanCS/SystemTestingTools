using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    public static class IServiceCollectionExtensions
    {
        [Obsolete("Please use InterceptHttpCallsAfterSending instead", true)]
        public static IServiceCollection RecordHttpClientRequestsAndResponses(this IServiceCollection serviceCollection, string folder)
        {
            return serviceCollection;
        }

        [Obsolete("Please use InterceptHttpCallsAfterSending instead", true)]
        public static IServiceCollection RecordHttpRequestsAndResponses(this IServiceCollection serviceCollection, string folder)
        {
            return serviceCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCollection">services</param>
        /// <param name="func">the function that will handle request / responses</param>
        /// <param name="config">the optional configuration for the interception</param>
        /// <returns></returns>
        public static IServiceCollection InterceptHttpCallsAfterSending(this IServiceCollection serviceCollection,
            Func<InterceptedHttpCall, Task<InterceptedHttpCall>> func,
            InterceptionConfiguration config = null)
        {
            var services = serviceCollection.BuildServiceProvider();
            var context = services.GetService<IHttpContextAccessor>();
            Constants.httpContextAccessor = context ?? throw new ApplicationException("Could not get IHttpContextAccessor, please register it in your ServiceCollection at Startup");

            if (config == null) config = new InterceptionConfiguration();

            if(config.RootStubsFolder == null)
            {
                var host = services.GetService<IWebHostEnvironment>() ?? throw new ApplicationException("Could not get an instance of IWebHostEnvironment");
                var finalFolder = Path.Combine(host.ContentRootPath, "App_Data/SystemTestingTools");
                if (!Directory.Exists(finalFolder)) Directory.CreateDirectory(finalFolder);
                config.RootStubsFolder = finalFolder;
            }

            Constants.handlerAfterResponse = func;
            Constants.GlobalConfiguration = config;
            Constants.GlobalRecordingManager = new RecordingManager(config.RootStubsFolder);

            serviceCollection.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, InterceptionFilter>((_) => new InterceptionFilter(() => new HttpCallInterceptor(false))));            

            RecordingCollection.Recordings.AddRange(Constants.GlobalRecordingManager.GetRecordings());

            return serviceCollection;
        }
    }
}
