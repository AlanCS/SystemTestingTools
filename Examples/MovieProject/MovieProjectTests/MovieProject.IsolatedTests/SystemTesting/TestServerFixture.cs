using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieProject.Web;
using NLog.Web;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.SystemTestings
{
    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; private set; }

        public string MocksFolder { get; private set; }

        public TestServerFixture()
        {
            StartServer();
            SanityCheckServer();
        }

        private void StartServer()
        {
            MocksFolder = new Regex(@"\\bin\\.*").Replace(System.Environment.CurrentDirectory, "") + @"\SystemTesting\Mocks";

            Startup.GlobalLastHandlerFactory = () => new HttpCallsInterceptorHandler();

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // use the same settings as the website (linked referenced)
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); 
                    // make small changes to configuration, such as disabling caching
                    config.AddJsonFile("appsettings.SystemTesting.json", optional: false, reloadOnChange: true);
                })
                .UseNLog()
                .ConfigureInterceptionOfHttpCalls()
                .IntercepLogs(minimumLevelToIntercept: LogLevel.Information, 
                                namespaceToIncludeStart: new[] { "MovieProject" },
                                namespaceToExcludeStart: new[] { "Microsoft" })
                .UseEnvironment("Development");

            Server = new TestServer(builder);   
        }

        /// <summary>
        /// Run a quick sanity check, before running any tests
        /// </summary>
        /// <returns></returns>
        private void SanityCheckServer()
        {
            HttpResponseMessage response = null;
            using (var client = Server.CreateClient())
                response = client.GetAsync("/healthcheck").Result; // we run the async method synchronously because it's called from a contructor, that can't be async

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new ApplicationException("TestServer doesn't respond to basic request to /healthcheck");

            if(MockInstrumentation.UnsessionedLogs.Count > 1)
                throw new ApplicationException("Unexpected logs are showing up when starting application");

            if (MockInstrumentation.UnsessionedLogs.FirstOrDefault()?.ToString() != "Information: Application is starting")
                throw new ApplicationException("Logs don't seem to be wired correctly");
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }

    [CollectionDefinition("SharedServer collection")]
    public class SharedServerCollection : ICollectionFixture<TestServerFixture>
    {
        // as suggested in https://xunit.github.io/docs/shared-context
        // done so this collection is shared between many tests
    }
}
