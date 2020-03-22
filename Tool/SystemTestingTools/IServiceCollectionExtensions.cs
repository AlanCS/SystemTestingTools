using Microsoft.Extensions.Http;
using SystemTestingTools.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace SystemTestingTools
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Will intercept every http call, and record details of the request and response in a text file, this file can be used later to load up stubs, so you can test without hitting external dependencies
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="folder">Folder full path where the response text files will be saved</param>
        /// <returns></returns>
        public static IServiceCollection RecordHttpRequestsAndResponses(this IServiceCollection serviceCollection, string folder)
        {
            serviceCollection.AddSingleton<IHttpMessageHandlerBuilderFilter, InterceptionFilter>((_) => new InterceptionFilter(() => new SystemTestingTools.RequestResponseRecorder(folder, false)));                

            return serviceCollection;
        }
    }
}
