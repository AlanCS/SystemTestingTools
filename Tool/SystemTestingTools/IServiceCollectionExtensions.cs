using Microsoft.Extensions.Http;
using SystemTestingTools.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System;

namespace SystemTestingTools
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Will intercept every http call, and record details of the request and response in a text file, this file can be used later to load up stubs, so you can test without hitting external dependencies
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="folder">Folder full path where the response text files will be saved</param>
        /// <param name="callerPath">Please don't pass this parameter, it will be used by .net to track the file that called this method</param>
        /// <returns></returns>
        public static IServiceCollection RecordHttpClientRequestsAndResponses(this IServiceCollection serviceCollection, string folder, [CallerFilePath]string callerPath = "")
        {
            serviceCollection.AddSingleton<IHttpMessageHandlerBuilderFilter, InterceptionFilter>((_) => new InterceptionFilter(() => new SystemTestingTools.RequestResponseRecorder(folder, false, callerPath)));                

            return serviceCollection;
        }

        [Obsolete("Please use RecordHttpClientRequestsAndResponses instead", true)]
        public static IServiceCollection RecordHttpRequestsAndResponses(this IServiceCollection serviceCollection, string folder, [CallerFilePath]string callerPath = "")
        {
            serviceCollection.RecordHttpClientRequestsAndResponses(folder, callerPath);

            return serviceCollection;
        }
    }
}
