using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Targets;
using System;

namespace SystemTestingTools
{
    /// <summary>
    /// Extends WebHostBuilder to allow interception of Http calls and logs
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Intercept outgoing Http calls so we can return mocks and make assertions later
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IWebHostBuilder ConfigureInterceptionOfHttpCalls(this IWebHostBuilder builder)
        {
            builder.ConfigureTestServices((c) =>
            {
                var services = c.BuildServiceProvider();
                var context = services.GetService<IHttpContextAccessor>();
                MockInstrumentation.context = context ?? throw new ApplicationException("Could not get IHttpContextAccessor, please register it in your ServiceCollection at Startup");
            });

            return builder;
        }

        /// <summary>
        /// Intercept NLog logs so we can assert those later
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="nlogFile"></param>
        /// <returns></returns>
        public static IWebHostBuilder IntercepNLog(this IWebHostBuilder builder, string nlogFile = "NLog.config")
        {
            LogManager.LoadConfiguration(nlogFile);

            // based on https://github.com/NLog/NLog/wiki/MethodCall-target
            var target = new MethodCallTarget("MethodTargetCSharp", Logger.Log);

            foreach (var rules in LogManager.Configuration.LoggingRules)
                rules.Targets.Add(target);

            LogManager.Configuration.Reload();

            return builder;
        }
    }
}
