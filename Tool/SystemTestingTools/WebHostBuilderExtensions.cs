using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    /// <summary>
    /// Extends WebHostBuilder to allow interception of Http calls and logs
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// obsolete method
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        [Obsolete("Please use ConfigureInterceptionOfHttpClientCalls() instead", true)]
        public static IWebHostBuilder ConfigureInterceptionOfHttpCalls(this IWebHostBuilder builder)
        {
            return ConfigureInterceptionOfHttpClientCalls(builder);
        }

        /// <summary>
        /// Intercept outgoing HttpClient calls so we can return stubs and make assertions later
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IWebHostBuilder ConfigureInterceptionOfHttpClientCalls(this IWebHostBuilder builder)
        {
            builder.ConfigureTestServices((c) =>
            {
                var services = c.BuildServiceProvider();
                var context = services.GetService<IHttpContextAccessor>();
                ContextRepo.context = context ?? throw new ApplicationException("Could not get IHttpContextAccessor, please register it in your ServiceCollection at Startup");
                c.AddSingleton<IHttpMessageHandlerBuilderFilter, InterceptionFilter>((_) => new InterceptionFilter(() => new HttpCallsInterceptorHandler()));
            });

            return builder;
        }

        /// <summary>
        /// Intercept NLog logs so we can assert those later
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="minimumLevelToIntercept"></param>
        /// <param name="namespaceToIncludeStart">Beginning of namespaces sources of logs allow; if null, all  to sources will be included. Example: MyNamespaceName</param>
        /// <param name="namespaceToExcludeStart">Beginning of namespaces sources of logs disallow; if null, no exclusion will apply. Exclusions are applied AFTER inclusion filter. Example: Microsoft</param>
        /// <returns></returns>
        public static IWebHostBuilder IntercepLogs(this IWebHostBuilder builder, LogLevel minimumLevelToIntercept = LogLevel.Trace, string[] namespaceToIncludeStart = null, string[] namespaceToExcludeStart = null)
        {
            builder = builder.ConfigureLogging((loggingBuilder) =>
            {
                loggingBuilder.SetMinimumLevel(minimumLevelToIntercept);
                loggingBuilder.AddProvider(new SystemTestingLoggerProvider(namespaceToIncludeStart, namespaceToExcludeStart));
            });

            return builder;
        }
    }
}
